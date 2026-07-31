using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class PedidoServicio : IPedidoServicio
{
    public const string EstadoPendiente = "Pendiente de verificación";
    public const string EstadoConfirmado = "Confirmado";
    public const string EstadoCancelado = "Cancelado";
    public const string EstadoEntregado = "Entregado";

    private static readonly string[] Estados = [EstadoPendiente, EstadoConfirmado, EstadoCancelado, EstadoEntregado];

    private readonly ApplicationDbContext _context;
    private readonly IFechaHoraServicio _fechaHoraServicio;

    public PedidoServicio(ApplicationDbContext context, IFechaHoraServicio fechaHoraServicio)
    {
        _context = context;
        _fechaHoraServicio = fechaHoraServicio;
    }

    public async Task<RegistrarPedidoEnLineaResult> RegistrarPedidoEnLineaAsync(RegistrarPedidoEnLineaViewModel model)
    {
        if (model.Items.Count == 0)
        {
            return RegistrarPedidoEnLineaResult.Failure("Debes agregar al menos un producto al carrito.");
        }

        var productoIds = model.Items.Select(item => item.ProductoId).Distinct().ToList();
        var productos = await _context.Productos
            .Include(producto => producto.Inventario)
            .Where(producto => productoIds.Contains(producto.Id))
            .ToListAsync();

        if (productos.Count != productoIds.Count)
        {
            return RegistrarPedidoEnLineaResult.Failure("Uno o más productos del carrito ya no están disponibles.");
        }

        foreach (var item in model.Items)
        {
            var producto = productos.Single(producto => producto.Id == item.ProductoId);
            var stock = producto.Inventario?.CantidadDisponible ?? 0;

            if (!producto.Activo || stock <= 0)
            {
                return RegistrarPedidoEnLineaResult.Failure($"El producto {producto.Nombre} no está disponible.");
            }

            if (item.Cantidad > stock)
            {
                return RegistrarPedidoEnLineaResult.Failure($"No hay stock suficiente para {producto.Nombre}. Disponible: {stock}.");
            }
        }

        var metodoPago = await _context.MetodosPago
            .SingleOrDefaultAsync(metodo => metodo.Activo && metodo.Nombre == "SINPE");

        if (metodoPago is null)
        {
            return RegistrarPedidoEnLineaResult.Failure("No se encontró el método de pago SINPE.");
        }

        var nombreCliente = model.NombreCliente.Trim();
        var cedulaCliente = model.CedulaCliente.Trim();
        var telefonoCliente = model.TelefonoCliente.Trim();
        var correoCliente = model.CorreoCliente.Trim();
        var direccionEntrega = model.DireccionEntrega.Trim();
        var cliente = await _context.Clientes
            .Where(item =>
                item.Identificacion == cedulaCliente)
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync();
        var subtotal = model.Items.Sum(item =>
        {
            var producto = productos.Single(producto => producto.Id == item.ProductoId);
            return producto.Precio * item.Cantidad;
        });
        var impuesto = subtotal * 0.13m;
        var pedido = new Pedido
        {
            ClienteId = cliente?.Id,
            NombreCliente = nombreCliente,
            Telefono = telefonoCliente,
            Correo = correoCliente,
            DireccionEntrega = direccionEntrega,
            FechaPedido = _fechaHoraServicio.UtcNow,
            MetodoPagoId = metodoPago.Id,
            Subtotal = subtotal,
            Descuento = 0,
            Impuesto = impuesto,
            Total = subtotal + impuesto,
            Estado = EstadoPendiente,
            Observacion = model.ObservacionPedido?.Trim() ?? string.Empty,
            ComprobantePago = new ComprobantePago
            {
                MetodoPagoId = metodoPago.Id,
                ReferenciaPago = model.ReferenciaPago.Trim(),
                EstadoVerificacion = "Pendiente",
                FechaRegistro = _fechaHoraServicio.UtcNow
            }
        };

        if (cliente is null)
        {
            cliente = new Cliente
            {
                Identificacion = cedulaCliente,
                NombreCompleto = nombreCliente,
                Telefono = telefonoCliente,
                Correo = correoCliente,
                Direccion = direccionEntrega,
                Activo = true,
                FechaRegistro = _fechaHoraServicio.UtcNow
            };

            pedido.Cliente = cliente;
        }
        else
        {
            pedido.ClienteId = cliente.Id;
            cliente.Activo = true;
            ActualizarDatosClienteDesdePedido(cliente, nombreCliente, telefonoCliente, correoCliente, direccionEntrega);
        }

        foreach (var item in model.Items)
        {
            var producto = productos.Single(producto => producto.Id == item.ProductoId);
            pedido.DetallePedidos.Add(new DetallePedido
            {
                ProductoId = producto.Id,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio,
                Subtotal = producto.Precio * item.Cantidad
            });
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();
        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();
        await transaction.CommitAsync();

        return RegistrarPedidoEnLineaResult.Success(pedido.Id, pedido.Total);
    }

    public async Task<PedidosIndexViewModel> ObtenerPedidosAsync(string? estado, DateTime? fechaInicio, DateTime? fechaFin, string? cliente)
    {
        var estadoNormalizado = NormalizarEstado(estado);
        var clienteNormalizado = cliente?.Trim();
        var query = _context.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.MetodoPago)
            .Include(pedido => pedido.ComprobantePago)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(estadoNormalizado))
        {
            query = query.Where(pedido => pedido.Estado == estadoNormalizado);
        }

        if (fechaInicio.HasValue || fechaFin.HasValue)
        {
            var inicio = fechaInicio ?? fechaFin!.Value;
            var fin = fechaFin ?? fechaInicio!.Value;
            var rango = _fechaHoraServicio.ObtenerRangoUtcCostaRica(inicio, fin);

            query = query.Where(pedido => pedido.FechaPedido >= rango.InicioUtc && pedido.FechaPedido < rango.FinExclusivoUtc);
        }

        if (!string.IsNullOrWhiteSpace(clienteNormalizado))
        {
            query = query.Where(pedido =>
                pedido.NombreCliente.Contains(clienteNormalizado) ||
                pedido.Telefono.Contains(clienteNormalizado) ||
                pedido.Correo.Contains(clienteNormalizado));
        }

        var pedidos = await query
            .OrderByDescending(pedido => pedido.FechaPedido)
            .Select(pedido => new PedidoListadoItemViewModel
            {
                Id = pedido.Id,
                FechaPedido = pedido.FechaPedido,
                Cliente = pedido.NombreCliente,
                Telefono = pedido.Telefono,
                MetodoPago = pedido.MetodoPago.Nombre,
                ReferenciaPago = pedido.ComprobantePago == null ? string.Empty : pedido.ComprobantePago.ReferenciaPago,
                Total = pedido.Total,
                Estado = pedido.Estado
            })
            .ToListAsync();

        foreach (var pedido in pedidos)
        {
            pedido.FechaPedido = _fechaHoraServicio.ConvertirUtcACostaRica(pedido.FechaPedido);
        }

        return new PedidosIndexViewModel
        {
            Estado = estadoNormalizado,
            FechaInicio = fechaInicio,
            FechaFin = fechaFin,
            Cliente = clienteNormalizado,
            Pedidos = pedidos,
            Estados = ObtenerEstadosSelect(estadoNormalizado)
        };
    }

    public async Task<PedidoDetalleViewModel?> ObtenerDetalleAsync(int id)
    {
        var pedido = await _context.Pedidos
            .AsNoTracking()
            .Include(item => item.MetodoPago)
            .Include(item => item.ComprobantePago)
                .ThenInclude(comprobante => comprobante!.MetodoPago)
            .Include(item => item.DetallePedidos)
                .ThenInclude(detalle => detalle.Producto)
                    .ThenInclude(producto => producto.Categoria)
            .Where(item => item.Id == id)
            .Select(item => new PedidoDetalleViewModel
            {
                Id = item.Id,
                FechaPedido = item.FechaPedido,
                NombreCliente = item.NombreCliente,
                Telefono = item.Telefono,
                Correo = item.Correo,
                DireccionEntrega = item.DireccionEntrega,
                MetodoPago = item.MetodoPago.Nombre,
                Subtotal = item.Subtotal,
                Descuento = item.Descuento,
                Impuesto = item.Impuesto,
                Total = item.Total,
                Estado = item.Estado,
                Observacion = item.Observacion,
                ReferenciaPago = item.ComprobantePago == null ? string.Empty : item.ComprobantePago.ReferenciaPago,
                ImagenComprobanteUrl = item.ComprobantePago == null ? string.Empty : item.ComprobantePago.ImagenComprobanteUrl,
                EstadoVerificacion = item.ComprobantePago == null ? string.Empty : item.ComprobantePago.EstadoVerificacion,
                FechaComprobante = item.ComprobantePago == null ? null : item.ComprobantePago.FechaRegistro,
                Productos = item.DetallePedidos
                    .Select(detalle => new PedidoDetalleProductoViewModel
                    {
                        Producto = detalle.Producto.Nombre,
                        Categoria = detalle.Producto.Categoria.Nombre,
                        Talla = detalle.Producto.Talla,
                        Color = detalle.Producto.Color,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal
                    })
                    .ToList()
            })
            .SingleOrDefaultAsync();

        if (pedido is null)
        {
            return null;
        }

        pedido.FechaPedido = _fechaHoraServicio.ConvertirUtcACostaRica(pedido.FechaPedido);

        if (pedido.FechaComprobante.HasValue)
        {
            pedido.FechaComprobante = _fechaHoraServicio.ConvertirUtcACostaRica(pedido.FechaComprobante.Value);
        }

        return pedido;
    }

    public async Task<PedidoOperacionResult> CambiarEstadoAsync(int pedidoId, string nuevoEstado, int usuarioId)
    {
        var estadoNormalizado = NormalizarEstado(nuevoEstado);

        if (string.IsNullOrWhiteSpace(estadoNormalizado))
        {
            return PedidoOperacionResult.Failure("El estado seleccionado no es válido.");
        }

        var pedido = await _context.Pedidos
            .Include(item => item.ComprobantePago)
            .SingleOrDefaultAsync(item => item.Id == pedidoId);

        if (pedido is null)
        {
            return PedidoOperacionResult.Failure("No se encontró el pedido seleccionado.");
        }

        if (!PuedeCambiarEstado(pedido.Estado, estadoNormalizado))
        {
            return PedidoOperacionResult.Failure("El cambio de estado solicitado no está permitido para este pedido.");
        }

        var estadoAnterior = pedido.Estado;
        pedido.Estado = estadoNormalizado;

        if (pedido.ComprobantePago is not null)
        {
            pedido.ComprobantePago.EstadoVerificacion = estadoNormalizado == EstadoConfirmado || estadoNormalizado == EstadoEntregado
                ? "Verificado"
                : estadoNormalizado == EstadoCancelado ? "Rechazado" : "Pendiente";
        }

        _context.Bitacora.Add(new Bitacora
        {
            UsuarioId = usuarioId,
            Modulo = "Pedidos",
            Accion = "Cambiar estado",
            Descripcion = $"Pedido PED-{pedido.Id:D6} cambió de {estadoAnterior} a {estadoNormalizado}.",
            FechaRegistro = _fechaHoraServicio.UtcNow
        });

        await _context.SaveChangesAsync();
        return PedidoOperacionResult.Success($"Pedido PED-{pedido.Id:D6} actualizado a {estadoNormalizado}.");
    }

    private static bool PuedeCambiarEstado(string estadoActual, string nuevoEstado)
    {
        if (estadoActual == nuevoEstado)
        {
            return false;
        }

        return nuevoEstado switch
        {
            EstadoConfirmado => estadoActual == EstadoPendiente,
            EstadoCancelado => estadoActual is EstadoPendiente or EstadoConfirmado,
            EstadoEntregado => estadoActual == EstadoConfirmado,
            _ => false
        };
    }

    private static string? NormalizarEstado(string? estado)
    {
        return Estados.FirstOrDefault(item => string.Equals(item, estado?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static void ActualizarDatosClienteDesdePedido(
        Cliente cliente,
        string nombre,
        string telefono,
        string correo,
        string direccion)
    {
        if (string.IsNullOrWhiteSpace(cliente.NombreCompleto))
        {
            cliente.NombreCompleto = nombre;
        }

        if (string.IsNullOrWhiteSpace(cliente.Telefono))
        {
            cliente.Telefono = telefono;
        }

        if (string.IsNullOrWhiteSpace(cliente.Correo))
        {
            cliente.Correo = correo;
        }

        if (string.IsNullOrWhiteSpace(cliente.Direccion))
        {
            cliente.Direccion = direccion;
        }
    }

    private static IEnumerable<SelectListItem> ObtenerEstadosSelect(string? selected)
    {
        return Estados.Select(estado => new SelectListItem
        {
            Value = estado,
            Text = estado,
            Selected = estado == selected
        });
    }
}
