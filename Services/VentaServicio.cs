using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class VentaServicio : IVentaServicio
{
    private const decimal TarifaIva = 0.13m;
    private const string EstadoVentaAprobada = "Aprobada";
    private const string EstadoFacturaEmitida = "Emitida";
    private readonly ApplicationDbContext _context;

    public VentaServicio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Venta>> ObtenerVentasAsync(int? clienteId = null)
    {
        var query = _context.Ventas
            .Include(venta => venta.Cliente)
            .Include(venta => venta.Usuario)
            .Include(venta => venta.MetodoPago)
            .Include(venta => venta.Factura)
            .AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(venta => venta.ClienteId == clienteId.Value);
        }

        return await query
            .OrderByDescending(venta => venta.FechaVenta)
            .ToListAsync();
    }

    public async Task<VentaOperacionResult> RegistrarVentaAsync(RegistrarVentaViewModel model, int usuarioId)
    {
        var validationMessage = ValidarDatosMinimos(model);

        if (!string.IsNullOrWhiteSpace(validationMessage))
        {
            return VentaOperacionResult.Failure(validationMessage);
        }

        var detallesAgrupados = model.Detalles
            .GroupBy(detalle => detalle.ProductoId)
            .Select(grupo => new VentaDetalleAgrupado
            {
                ProductoId = grupo.Key,
                Cantidad = grupo.Sum(detalle => (long)detalle.Cantidad)
            })
            .ToList();

        if (detallesAgrupados.Any(detalle => detalle.Cantidad > int.MaxValue))
        {
            return VentaOperacionResult.Failure("La cantidad solicitada para un producto es demasiado alta.");
        }

        var metodoPago = await _context.MetodosPago.FindAsync(model.MetodoPagoId);

        if (metodoPago is null)
        {
            return VentaOperacionResult.Failure("El metodo de pago especificado no existe.");
        }

        if (!metodoPago.Activo)
        {
            return VentaOperacionResult.Failure("El metodo de pago seleccionado esta inactivo.");
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var cliente = await _context.Clientes.FindAsync(model.ClienteId);

            if (cliente is null)
            {
                await transaction.RollbackAsync();
                return VentaOperacionResult.Failure("El cliente especificado no existe.");
            }

            if (cliente.Identificacion == "000000000")
            {
                await transaction.RollbackAsync();
                return VentaOperacionResult.Failure("Selecciona un cliente registrado para la venta.");
            }

            if (!cliente.Activo)
            {
                await transaction.RollbackAsync();
                return VentaOperacionResult.Failure("El cliente seleccionado esta inactivo.");
            }

            var productIds = detallesAgrupados.Select(detalle => detalle.ProductoId).ToList();
            var productos = await _context.Productos
                .Include(producto => producto.Inventario)
                .Where(producto => productIds.Contains(producto.Id))
                .ToDictionaryAsync(producto => producto.Id);

            var subtotal = 0m;

            foreach (var detalle in detallesAgrupados)
            {
                if (!productos.TryGetValue(detalle.ProductoId, out var producto))
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure($"El producto con ID {detalle.ProductoId} no existe.");
                }

                if (!producto.Activo)
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure($"El producto '{producto.Nombre}' esta inactivo.");
                }

                if (producto.Inventario is null)
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure($"El producto '{producto.Nombre}' no esta registrado en inventario.");
                }

                if (producto.Inventario.CantidadDisponible < detalle.Cantidad)
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure(
                        $"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Inventario.CantidadDisponible}, solicitado: {detalle.Cantidad}.");
                }

                subtotal += detalle.Cantidad * producto.Precio;
            }

            var descuento = decimal.Round(model.Descuento, 2, MidpointRounding.AwayFromZero);

            if (descuento > subtotal)
            {
                await transaction.RollbackAsync();
                return VentaOperacionResult.Failure("El descuento no puede ser mayor que el subtotal.");
            }

            var impuesto = decimal.Round((subtotal - descuento) * TarifaIva, 2, MidpointRounding.AwayFromZero);
            var total = subtotal - descuento + impuesto;
            var fechaVenta = DateTime.UtcNow;

            var venta = new Venta
            {
                ClienteId = cliente.Id,
                UsuarioId = usuarioId,
                MetodoPagoId = model.MetodoPagoId,
                FechaVenta = fechaVenta,
                Subtotal = subtotal,
                Descuento = descuento,
                Impuesto = impuesto,
                Total = total,
                Estado = EstadoVentaAprobada,
                Observacion = model.Observacion?.Trim() ?? string.Empty,
                CierreCajaId = null
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync();

            foreach (var detalle in detallesAgrupados)
            {
                var producto = productos[detalle.ProductoId];
                var inventario = producto.Inventario!;
                var cantidadVendida = (int)detalle.Cantidad;
                var cantidadAnterior = inventario.CantidadDisponible;

                _context.DetalleVentas.Add(new DetalleVenta
                {
                    VentaId = venta.Id,
                    ProductoId = detalle.ProductoId,
                    Cantidad = cantidadVendida,
                    PrecioUnitario = producto.Precio,
                    Subtotal = cantidadVendida * producto.Precio
                });

                inventario.CantidadDisponible -= cantidadVendida;
                inventario.FechaActualizacion = fechaVenta;

                _context.MovimientosInventario.Add(new MovimientoInventario
                {
                    ProductoId = producto.Id,
                    TipoMovimiento = "Salida",
                    Cantidad = cantidadVendida,
                    CantidadAnterior = cantidadAnterior,
                    CantidadNueva = inventario.CantidadDisponible,
                    Motivo = $"Venta #{venta.Id}",
                    FechaMovimiento = fechaVenta,
                    UsuarioId = usuarioId
                });
            }

            var numeroFactura = GenerarNumeroFactura(venta.Id, fechaVenta);

            _context.Facturas.Add(new Factura
            {
                VentaId = venta.Id,
                NumeroFactura = numeroFactura,
                FechaEmision = fechaVenta,
                Subtotal = subtotal,
                Descuento = descuento,
                Impuesto = impuesto,
                Total = total,
                Estado = EstadoFacturaEmitida
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return VentaOperacionResult.Success(
                venta.Id,
                numeroFactura,
                $"Venta aprobada correctamente. Comprobante generado: {numeroFactura}.");
        }
        catch (DbUpdateConcurrencyException)
        {
            await transaction.RollbackAsync();
            return VentaOperacionResult.Failure(
                "El inventario cambio mientras se registraba la venta. Actualiza la pagina y vuelve a intentarlo.");
        }
        catch (DbUpdateException)
        {
            await transaction.RollbackAsync();
            return VentaOperacionResult.Failure("No fue posible registrar la venta. Revisa los datos e intentalo nuevamente.");
        }
    }

    private static string? ValidarDatosMinimos(RegistrarVentaViewModel model)
    {
        if (model is null)
        {
            return "Los datos de la venta no son validos.";
        }

        if (model.ClienteId <= 0)
        {
            return "Selecciona un cliente.";
        }

        if (model.MetodoPagoId <= 0)
        {
            return "Selecciona un metodo de pago.";
        }

        if (model.Detalles is null || !model.Detalles.Any())
        {
            return "La venta debe tener al menos un producto.";
        }

        if (model.Detalles.Any(detalle => detalle.ProductoId <= 0 || detalle.Cantidad <= 0))
        {
            return "Los productos y cantidades de la venta no son validos.";
        }

        if (model.Descuento < 0)
        {
            return "El descuento no puede ser negativo.";
        }

        if (model.Observacion?.Length > 500)
        {
            return "La observacion no puede superar los 500 caracteres.";
        }

        return null;
    }

    private static string GenerarNumeroFactura(int ventaId, DateTime fechaVenta) =>
        $"FAC-{fechaVenta:yyyyMMdd}-{ventaId:D6}";

    private sealed class VentaDetalleAgrupado
    {
        public int ProductoId { get; set; }

        public long Cantidad { get; set; }
    }
}
