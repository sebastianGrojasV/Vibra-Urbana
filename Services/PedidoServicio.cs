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
    public const string EstadoEnviado = "Enviado";
    public const string EstadoEnTransito = "En tránsito";
    public const string EstadoCancelado = "Cancelado";
    public const string EstadoEntregado = "Entregado";
    private const string EstadoVentaAprobada = "Aprobada";
    private const string EstadoVentaPendiente = "Pendiente";
    private const string EstadoVentaAnulada = "Anulada";
    private const string EstadoFacturaEmitida = "Emitida";
    private const string EstadoFacturaAnulada = "Anulada";

    private static readonly string[] Estados = [EstadoPendiente, EstadoConfirmado, EstadoEnviado, EstadoEnTransito, EstadoCancelado, EstadoEntregado];

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
            .Include(producto => producto.Categoria)
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

            if (!producto.Activo || !producto.Categoria.Activo || stock <= 0)
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
        var correoCliente = model.CorreoCliente.Trim().ToLowerInvariant();
        var direccionEntrega = model.DireccionEntrega.Trim();
        var cliente = await _context.Clientes
            .Where(item =>
                item.Identificacion == cedulaCliente)
            .OrderBy(item => item.Id)
            .FirstOrDefaultAsync();
        if (await _context.Clientes.AnyAsync(item => item.Correo.Trim().ToLower() == correoCliente &&
            (cliente == null || item.Id != cliente.Id)))
        {
            return RegistrarPedidoEnLineaResult.Failure("El correo electrónico ya está asociado a otro cliente. Revisa la cédula y el correo.");
        }
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
            TokenEntrega = CrearTokenEntrega(),
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
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException exception) when
            (exception.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        {
            await transaction.RollbackAsync();
            return RegistrarPedidoEnLineaResult.Failure("La cédula o el correo ya se registró en otra solicitud. Revisa los datos e inténtalo de nuevo.");
        }
        await transaction.CommitAsync();

        return RegistrarPedidoEnLineaResult.Success(pedido.Id, pedido.Total);
    }

    public async Task<PedidosIndexViewModel> ObtenerPedidosAsync(string? estado, DateTime? fechaInicio, DateTime? fechaFin, string? cliente)
    {
        var estadoNormalizado = NormalizarEstado(estado);
        var clienteNormalizado = cliente?.Trim();
        var query = _context.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.Cliente)
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
                pedido.Correo.Contains(clienteNormalizado) ||
                (pedido.Cliente != null && pedido.Cliente.Identificacion.Contains(clienteNormalizado)));
        }

        var pedidos = await query
            .OrderByDescending(pedido => pedido.FechaPedido)
            .Select(pedido => new PedidoListadoItemViewModel
            {
                Id = pedido.Id,
                FechaPedido = pedido.FechaPedido,
                Cliente = pedido.NombreCliente,
                IdentificacionCliente = pedido.Cliente == null ? string.Empty : pedido.Cliente.Identificacion,
                Telefono = pedido.Telefono,
                MetodoPago = pedido.MetodoPago.Nombre,
                ReferenciaPago = pedido.ComprobantePago == null ? string.Empty : pedido.ComprobantePago.ReferenciaPago,
                Total = pedido.Total,
                Estado = pedido.Estado,
                FechaEntrega = pedido.FechaEntrega
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
                TokenEntrega = item.TokenEntrega,
                FechaEnvio = item.FechaEnvio,
                FechaTransito = item.FechaTransito,
                FechaEntrega = item.FechaEntrega,
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

        if (pedido.FechaEnvio.HasValue)
        {
            pedido.FechaEnvio = _fechaHoraServicio.ConvertirUtcACostaRica(pedido.FechaEnvio.Value);
        }

        if (pedido.FechaTransito.HasValue)
        {
            pedido.FechaTransito = _fechaHoraServicio.ConvertirUtcACostaRica(pedido.FechaTransito.Value);
        }

        if (pedido.FechaEntrega.HasValue)
        {
            pedido.FechaEntrega = _fechaHoraServicio.ConvertirUtcACostaRica(pedido.FechaEntrega.Value);
        }

        return pedido;
    }

    public async Task<PedidoSeguimientoViewModel?> ObtenerSeguimientoAsync(string token)
    {
        var tokenNormalizado = token?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tokenNormalizado))
        {
            return null;
        }

        var model = await _context.Pedidos
            .AsNoTracking()
            .Where(pedido => pedido.TokenEntrega == tokenNormalizado)
            .Select(pedido => new PedidoSeguimientoViewModel
            {
                Id = pedido.Id,
                TokenEntrega = pedido.TokenEntrega,
                NombreCliente = pedido.NombreCliente,
                Estado = pedido.Estado,
                FechaPedido = pedido.FechaPedido,
                FechaEnvio = pedido.FechaEnvio,
                FechaTransito = pedido.FechaTransito,
                FechaEntrega = pedido.FechaEntrega,
                PuedeConfirmarEntrega = pedido.Estado == EstadoConfirmado ||
                    pedido.Estado == EstadoEnviado ||
                    pedido.Estado == EstadoEnTransito
            })
            .SingleOrDefaultAsync();

        if (model is null)
        {
            return null;
        }

        model.FechaPedido = _fechaHoraServicio.ConvertirUtcACostaRica(model.FechaPedido);

        if (model.FechaEnvio.HasValue)
        {
            model.FechaEnvio = _fechaHoraServicio.ConvertirUtcACostaRica(model.FechaEnvio.Value);
        }

        if (model.FechaTransito.HasValue)
        {
            model.FechaTransito = _fechaHoraServicio.ConvertirUtcACostaRica(model.FechaTransito.Value);
        }

        if (model.FechaEntrega.HasValue)
        {
            model.FechaEntrega = _fechaHoraServicio.ConvertirUtcACostaRica(model.FechaEntrega.Value);
        }

        return model;
    }

    public async Task<PedidoOperacionResult> ConfirmarEntregaPorTokenAsync(string token)
    {
        var tokenNormalizado = token?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(tokenNormalizado))
        {
            return PedidoOperacionResult.Failure("El código de seguimiento no es válido.");
        }

        var pedido = await _context.Pedidos
            .SingleOrDefaultAsync(item => item.TokenEntrega == tokenNormalizado);

        if (pedido is null)
        {
            return PedidoOperacionResult.Failure("No encontramos el pedido asociado a este código.");
        }

        if (pedido.Estado == EstadoEntregado)
        {
            return PedidoOperacionResult.Success($"El pedido PED-{pedido.Id:D6} ya estaba marcado como entregado.");
        }

        if (pedido.Estado == EstadoCancelado)
        {
            return PedidoOperacionResult.Failure("Este pedido fue cancelado y no puede marcarse como entregado.");
        }

        if (pedido.Estado == EstadoPendiente)
        {
            return PedidoOperacionResult.Failure("Este pedido aún no ha sido confirmado por la tienda.");
        }

        var estadoAnterior = pedido.Estado;
        var fechaEntrega = _fechaHoraServicio.UtcNow;
        pedido.Estado = EstadoEntregado;
        pedido.FechaEntrega = fechaEntrega;

        if (!pedido.FechaTransito.HasValue)
        {
            pedido.FechaTransito = fechaEntrega;
        }

        if (!pedido.FechaEnvio.HasValue)
        {
            pedido.FechaEnvio = fechaEntrega;
        }

        _context.Bitacora.Add(new Bitacora
        {
            UsuarioId = null,
            Modulo = "Pedidos",
            Accion = "Confirmar entrega QR",
            Descripcion = $"Pedido PED-{pedido.Id:D6} confirmado como entregado por escaneo del cliente. Estado anterior: {estadoAnterior}.",
            FechaRegistro = fechaEntrega
        });

        await _context.SaveChangesAsync();

        return PedidoOperacionResult.Success($"Pedido PED-{pedido.Id:D6} marcado como entregado.");
    }

    public async Task<PedidoOperacionResult> CambiarEstadoAsync(int pedidoId, string nuevoEstado, int usuarioId, string? observacion)
    {
        var estadoNormalizado = NormalizarEstado(nuevoEstado);
        var observacionNormalizada = observacion?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(estadoNormalizado))
        {
            return PedidoOperacionResult.Failure("El estado seleccionado no es válido.");
        }

        if (observacionNormalizada.Length > 250)
        {
            return PedidoOperacionResult.Failure("La observación no puede superar 250 caracteres.");
        }

        if (estadoNormalizado == EstadoCancelado && string.IsNullOrWhiteSpace(observacionNormalizada))
        {
            return PedidoOperacionResult.Failure("Debes indicar una observación para cancelar el pedido.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var pedido = await _context.Pedidos
                .Include(item => item.ComprobantePago)
                .Include(item => item.Venta)
                    .ThenInclude(venta => venta!.Factura)
                .Include(item => item.Venta)
                    .ThenInclude(venta => venta!.DetalleVentas)
                .Include(item => item.DetallePedidos)
                    .ThenInclude(detalle => detalle.Producto)
                        .ThenInclude(producto => producto.Inventario)
                .SingleOrDefaultAsync(item => item.Id == pedidoId);

            if (pedido is null)
            {
                await transaction.RollbackAsync();
                return PedidoOperacionResult.Failure("No se encontró el pedido seleccionado.");
            }

            if (!PuedeCambiarEstado(pedido.Estado, estadoNormalizado))
            {
                await transaction.RollbackAsync();
                return PedidoOperacionResult.Failure("El cambio de estado solicitado no está permitido para este pedido.");
            }

            var estadoAnterior = pedido.Estado;
            var fechaCambio = _fechaHoraServicio.UtcNow;

            if (estadoAnterior == EstadoPendiente && estadoNormalizado == EstadoConfirmado)
            {
                var errorInventario = AplicarSalidaInventarioPorPedido(pedido, usuarioId, fechaCambio);

                if (!string.IsNullOrWhiteSpace(errorInventario))
                {
                    await transaction.RollbackAsync();
                    return PedidoOperacionResult.Failure(errorInventario);
                }

                var errorVenta = await AsegurarVentaAprobadaPorPedidoAsync(pedido, usuarioId, fechaCambio);

                if (!string.IsNullOrWhiteSpace(errorVenta))
                {
                    await transaction.RollbackAsync();
                    return PedidoOperacionResult.Failure(errorVenta);
                }
            }
            else if (estadoAnterior is EstadoConfirmado or EstadoEnviado or EstadoEnTransito && estadoNormalizado == EstadoCancelado)
            {
                RevertirInventarioPorPedido(pedido, usuarioId, fechaCambio, observacionNormalizada);
                MarcarVentaPedidoAnulada(pedido, usuarioId, fechaCambio, observacionNormalizada);
            }
            else if (estadoAnterior == EstadoPendiente && estadoNormalizado == EstadoCancelado)
            {
                MarcarVentaPedidoAnulada(pedido, usuarioId, fechaCambio, observacionNormalizada);
            }

            pedido.Estado = estadoNormalizado;
            AsignarFechaEstado(pedido, estadoNormalizado, fechaCambio);

            if (pedido.ComprobantePago is not null)
            {
                pedido.ComprobantePago.EstadoVerificacion = estadoNormalizado is EstadoConfirmado or EstadoEnviado or EstadoEnTransito or EstadoEntregado
                    ? "Verificado"
                    : estadoNormalizado == EstadoCancelado ? "Rechazado" : "Pendiente";
            }

            _context.Bitacora.Add(new Bitacora
            {
                UsuarioId = usuarioId,
                Modulo = "Pedidos",
                Accion = "Cambiar estado",
                Descripcion = CrearDescripcionCambioEstadoPedido(pedido.Id, estadoAnterior, estadoNormalizado, observacionNormalizada),
                FechaRegistro = fechaCambio
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return PedidoOperacionResult.Success($"Pedido PED-{pedido.Id:D6} actualizado a {estadoNormalizado}.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return PedidoOperacionResult.Failure(
                "El inventario cambió mientras se actualizaba el pedido. Actualiza la página y vuelve a intentarlo.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            return PedidoOperacionResult.Failure("No se pudo actualizar el pedido. Inténtalo nuevamente.");
        }
    }

    private string? AplicarSalidaInventarioPorPedido(Pedido pedido, int usuarioId, DateTime fechaMovimiento)
    {
        var detallesAgrupados = pedido.DetallePedidos
            .GroupBy(detalle => detalle.ProductoId)
            .Select(grupo => new
            {
                ProductoId = grupo.Key,
                Producto = grupo.First().Producto,
                Cantidad = grupo.Sum(detalle => (long)detalle.Cantidad)
            })
            .ToList();

        foreach (var detalle in detallesAgrupados)
        {
            if (detalle.Cantidad <= 0 || detalle.Cantidad > int.MaxValue)
            {
                return "El pedido contiene cantidades inválidas.";
            }

            if (!detalle.Producto.Activo)
            {
                return $"El producto '{detalle.Producto.Nombre}' está inactivo y no puede confirmarse.";
            }

            if (detalle.Producto.Inventario is null)
            {
                return $"El producto '{detalle.Producto.Nombre}' no está registrado en inventario.";
            }

            if (detalle.Producto.Inventario.CantidadDisponible < detalle.Cantidad)
            {
                return $"Stock insuficiente para '{detalle.Producto.Nombre}'. Disponible: {detalle.Producto.Inventario.CantidadDisponible}, solicitado: {detalle.Cantidad}.";
            }
        }

        foreach (var detalle in detallesAgrupados)
        {
            var inventario = detalle.Producto.Inventario!;
            var cantidad = (int)detalle.Cantidad;
            var cantidadAnterior = inventario.CantidadDisponible;

            if (inventario.CantidadDisponible < cantidad)
            {
                return $"Stock insuficiente para '{detalle.Producto.Nombre}'. Disponible: {inventario.CantidadDisponible}, solicitado: {cantidad}.";
            }

            inventario.CantidadDisponible -= cantidad;
            inventario.FechaActualizacion = fechaMovimiento;

            _context.MovimientosInventario.Add(new MovimientoInventario
            {
                ProductoId = detalle.ProductoId,
                TipoMovimiento = "Salida",
                Cantidad = cantidad,
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = inventario.CantidadDisponible,
                Motivo = $"Confirmación pedido PED-{pedido.Id:D6}",
                FechaMovimiento = fechaMovimiento,
                UsuarioId = usuarioId
            });
        }

        return null;
    }

    private void RevertirInventarioPorPedido(Pedido pedido, int usuarioId, DateTime fechaMovimiento, string observacion)
    {
        var detallesAgrupados = pedido.DetallePedidos
            .GroupBy(detalle => detalle.ProductoId)
            .Select(grupo => new
            {
                ProductoId = grupo.Key,
                Producto = grupo.First().Producto,
                Cantidad = grupo.Sum(detalle => detalle.Cantidad)
            });

        foreach (var detalle in detallesAgrupados)
        {
            var inventario = detalle.Producto.Inventario;

            if (inventario is null)
            {
                continue;
            }

            var cantidadAnterior = inventario.CantidadDisponible;
            inventario.CantidadDisponible += detalle.Cantidad;
            inventario.FechaActualizacion = fechaMovimiento;

            var motivo = string.IsNullOrWhiteSpace(observacion)
                ? $"Cancelación pedido PED-{pedido.Id:D6}"
                : $"Cancelación pedido PED-{pedido.Id:D6}: {observacion}";

            _context.MovimientosInventario.Add(new MovimientoInventario
            {
                ProductoId = detalle.ProductoId,
                TipoMovimiento = "Reversión",
                Cantidad = detalle.Cantidad,
                CantidadAnterior = cantidadAnterior,
                CantidadNueva = inventario.CantidadDisponible,
                Motivo = motivo.Length > 300 ? motivo[..300] : motivo,
                FechaMovimiento = fechaMovimiento,
                UsuarioId = usuarioId
            });
        }
    }

    private static string CrearDescripcionCambioEstadoPedido(int pedidoId, string estadoAnterior, string estadoNuevo, string observacion)
    {
        var descripcion = $"Pedido PED-{pedidoId:D6} cambió de {estadoAnterior} a {estadoNuevo}.";

        if (!string.IsNullOrWhiteSpace(observacion))
        {
            descripcion += $" Observación: {observacion}";
        }

        return descripcion;
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
            EstadoEnviado => estadoActual == EstadoConfirmado,
            EstadoEnTransito => estadoActual is EstadoConfirmado or EstadoEnviado,
            EstadoCancelado => estadoActual is EstadoPendiente or EstadoConfirmado or EstadoEnviado or EstadoEnTransito,
            EstadoEntregado => estadoActual is EstadoConfirmado or EstadoEnviado or EstadoEnTransito,
            _ => false
        };
    }

    private static void AsignarFechaEstado(Pedido pedido, string estado, DateTime fecha)
    {
        if (estado == EstadoEnviado && !pedido.FechaEnvio.HasValue)
        {
            pedido.FechaEnvio = fecha;
        }

        if (estado == EstadoEnTransito)
        {
            if (!pedido.FechaEnvio.HasValue)
            {
                pedido.FechaEnvio = fecha;
            }

            if (!pedido.FechaTransito.HasValue)
            {
                pedido.FechaTransito = fecha;
            }
        }

        if (estado == EstadoEntregado)
        {
            if (!pedido.FechaEnvio.HasValue)
            {
                pedido.FechaEnvio = fecha;
            }

            if (!pedido.FechaTransito.HasValue)
            {
                pedido.FechaTransito = fecha;
            }

            if (!pedido.FechaEntrega.HasValue)
            {
                pedido.FechaEntrega = fecha;
            }
        }
    }

    private async Task<string?> AsegurarVentaAprobadaPorPedidoAsync(Pedido pedido, int usuarioId, DateTime fechaCambio)
    {
        if (!pedido.ClienteId.HasValue)
        {
            return "El pedido no tiene cliente asociado y no puede registrarse como venta.";
        }

        var venta = pedido.Venta;

        if (venta is null)
        {
            venta = new Venta
            {
                ClienteId = pedido.ClienteId.Value,
                UsuarioId = usuarioId,
                MetodoPagoId = pedido.MetodoPagoId,
                FechaVenta = pedido.FechaPedido,
                Subtotal = pedido.Subtotal,
                Descuento = pedido.Descuento,
                Impuesto = pedido.Impuesto,
                Total = pedido.Total,
                Estado = EstadoVentaAprobada,
                Observacion = CrearObservacionVentaPedido(pedido),
                CierreCajaId = null
            };

            foreach (var detalle in pedido.DetallePedidos)
            {
                venta.DetalleVentas.Add(new DetalleVenta
                {
                    ProductoId = detalle.ProductoId,
                    Cantidad = detalle.Cantidad,
                    PrecioUnitario = detalle.PrecioUnitario,
                    Subtotal = detalle.Subtotal
                });
            }

            _context.Ventas.Add(venta);
            pedido.Venta = venta;
            await _context.SaveChangesAsync();
        }
        else
        {
            venta.UsuarioId = usuarioId;
            venta.Estado = EstadoVentaAprobada;
            venta.MotivoAnulacion = string.Empty;
            venta.FechaAnulacion = null;
            venta.UsuarioAnulacionId = null;
            venta.Subtotal = pedido.Subtotal;
            venta.Descuento = pedido.Descuento;
            venta.Impuesto = pedido.Impuesto;
            venta.Total = pedido.Total;

            if (!venta.DetalleVentas.Any())
            {
                foreach (var detalle in pedido.DetallePedidos)
                {
                    venta.DetalleVentas.Add(new DetalleVenta
                    {
                        VentaId = venta.Id,
                        ProductoId = detalle.ProductoId,
                        Cantidad = detalle.Cantidad,
                        PrecioUnitario = detalle.PrecioUnitario,
                        Subtotal = detalle.Subtotal
                    });
                }
            }
        }

        if (venta.Factura is null)
        {
            var numeroFactura = await GenerarNumeroFacturaUnicoAsync(venta.Id, venta.FechaVenta);
            venta.Factura = new Factura
            {
                VentaId = venta.Id,
                NumeroFactura = numeroFactura,
                FechaEmision = fechaCambio,
                Subtotal = venta.Subtotal,
                Descuento = venta.Descuento,
                Impuesto = venta.Impuesto,
                Total = venta.Total,
                Estado = EstadoFacturaEmitida
            };
        }
        else
        {
            venta.Factura.Estado = EstadoFacturaEmitida;
            venta.Factura.Subtotal = venta.Subtotal;
            venta.Factura.Descuento = venta.Descuento;
            venta.Factura.Impuesto = venta.Impuesto;
            venta.Factura.Total = venta.Total;
        }

        return null;
    }

    private void MarcarVentaPedidoAnulada(Pedido pedido, int usuarioId, DateTime fechaCambio, string observacion)
    {
        if (pedido.Venta is null)
        {
            return;
        }

        var motivo = string.IsNullOrWhiteSpace(observacion)
            ? $"Pedido PED-{pedido.Id:D6} cancelado."
            : observacion;

        pedido.Venta.Estado = EstadoVentaAnulada;
        pedido.Venta.MotivoAnulacion = motivo.Length > 300 ? motivo[..300] : motivo;
        pedido.Venta.FechaAnulacion = fechaCambio;
        pedido.Venta.UsuarioAnulacionId = usuarioId;

        if (pedido.Venta.Factura is not null)
        {
            pedido.Venta.Factura.Estado = EstadoFacturaAnulada;
        }
    }

    private async Task<string> GenerarNumeroFacturaUnicoAsync(int ventaId, DateTime fechaVenta)
    {
        var numeroBase = $"FAC-{fechaVenta:yyyyMMdd}-{ventaId:D6}";

        if (!await _context.Facturas.AnyAsync(factura => factura.NumeroFactura == numeroBase))
        {
            return numeroBase;
        }

        for (var intento = 1; intento <= 20; intento++)
        {
            var numeroAlterno = $"{numeroBase}-{intento:D2}";

            if (!await _context.Facturas.AnyAsync(factura => factura.NumeroFactura == numeroAlterno))
            {
                return numeroAlterno;
            }
        }

        throw new InvalidOperationException("No fue posible generar un número único de factura.");
    }

    private static string CrearObservacionVentaPedido(Pedido pedido)
    {
        var observacion = $"Pedido en línea PED-{pedido.Id:D6}";

        if (!string.IsNullOrWhiteSpace(pedido.Observacion))
        {
            observacion += $": {pedido.Observacion}";
        }

        return observacion.Length > 500 ? observacion[..500] : observacion;
    }

    private static string? NormalizarEstado(string? estado)
    {
        return Estados.FirstOrDefault(item => string.Equals(item, estado?.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private static string CrearTokenEntrega()
    {
        return Convert.ToHexString(Guid.NewGuid().ToByteArray()).ToLowerInvariant();
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
