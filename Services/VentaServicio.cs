using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class VentaServicio : IVentaServicio
{
    private readonly ApplicationDbContext _context;

    public VentaServicio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Venta>> ObtenerVentasAsync(int? clienteId = null)
    {
        var query = _context.Ventas
            .Include(v => v.Cliente)
            .Include(v => v.Usuario)
            .Include(v => v.MetodoPago)
            .AsQueryable();

        if (clienteId.HasValue)
        {
            query = query.Where(v => v.ClienteId == clienteId.Value);
        }

        return await query
            .OrderByDescending(v => v.FechaVenta)
            .ToListAsync();
    }

    public async Task<VentaOperacionResult> RegistrarVentaAsync(RegistrarVentaViewModel model, int usuarioId)
    {
        if (model.Detalles == null || !model.Detalles.Any())
        {
            return VentaOperacionResult.Failure("La venta debe tener al menos un producto.");
        }

        // Validar Cliente
        var cliente = await _context.Clientes.FindAsync(model.ClienteId);
        if (cliente == null)
        {
            return VentaOperacionResult.Failure("El cliente especificado no existe.");
        }
        if (!cliente.Activo)
        {
            return VentaOperacionResult.Failure("El cliente seleccionado está inactivo.");
        }

        // Validar Método de Pago
        var metodoPago = await _context.MetodosPago.FindAsync(model.MetodoPagoId);
        if (metodoPago == null)
        {
            return VentaOperacionResult.Failure("El método de pago especificado no existe.");
        }
        if (!metodoPago.Activo)
        {
            return VentaOperacionResult.Failure("El método de pago seleccionado está inactivo.");
        }

        // Iniciar transacción
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Cargar productos en memoria para validar stock
            var productIds = model.Detalles.Select(d => d.ProductoId).Distinct().ToList();
            var productos = await _context.Productos
                .Include(p => p.Inventario)
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id);

            decimal subtotal = 0;

            // Validar existencia y stock de cada producto
            foreach (var detail in model.Detalles)
            {
                if (!productos.TryGetValue(detail.ProductoId, out var producto))
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure($"El producto con ID {detail.ProductoId} no existe.");
                }

                if (!producto.Activo)
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure($"El producto '{producto.Nombre}' está inactivo.");
                }

                if (producto.Inventario == null)
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure($"El producto '{producto.Nombre}' no está registrado en el inventario.");
                }

                if (producto.Inventario.CantidadDisponible < detail.Cantidad)
                {
                    await transaction.RollbackAsync();
                    return VentaOperacionResult.Failure($"Stock insuficiente para '{producto.Nombre}'. Disponible: {producto.Inventario.CantidadDisponible}, Solicitado: {detail.Cantidad}.");
                }

                // Sumar al subtotal usando el precio real de base de datos para evitar alteraciones del cliente
                subtotal += detail.Cantidad * producto.Precio;
            }

            // Calcular totales finales
            decimal descuento = model.Descuento;
            decimal impuesto = model.Impuesto;
            decimal total = subtotal - descuento + impuesto;

            if (total < 0)
            {
                await transaction.RollbackAsync();
                return VentaOperacionResult.Failure("El total de la venta no puede ser negativo.");
            }

            // Crear Venta
            var venta = new Venta
            {
                ClienteId = model.ClienteId,
                UsuarioId = usuarioId,
                MetodoPagoId = model.MetodoPagoId,
                FechaVenta = DateTime.UtcNow,
                Subtotal = subtotal,
                Descuento = descuento,
                Impuesto = impuesto,
                Total = total,
                Estado = "Registrada",
                Observacion = model.Observacion?.Trim() ?? string.Empty,
                CierreCajaId = null
            };

            _context.Ventas.Add(venta);
            await _context.SaveChangesAsync(); // Guarda venta para generar ID

            // Crear Detalles y actualizar Inventario
            foreach (var detail in model.Detalles)
            {
                var producto = productos[detail.ProductoId];
                var inventario = producto.Inventario!;

                // 1. Detalle Venta
                var detalleVenta = new DetalleVenta
                {
                    VentaId = venta.Id,
                    ProductoId = detail.ProductoId,
                    Cantidad = detail.Cantidad,
                    PrecioUnitario = producto.Precio,
                    Subtotal = detail.Cantidad * producto.Precio
                };
                _context.DetalleVentas.Add(detalleVenta);

                // 2. Descontar Stock
                int cantidadAnterior = inventario.CantidadDisponible;
                inventario.CantidadDisponible -= detail.Cantidad;
                inventario.FechaActualizacion = DateTime.UtcNow;

                // 3. Registrar Movimiento de Inventario
                var movimiento = new MovimientoInventario
                {
                    ProductoId = producto.Id,
                    TipoMovimiento = "Salida",
                    Cantidad = detail.Cantidad,
                    CantidadAnterior = cantidadAnterior,
                    CantidadNueva = inventario.CantidadDisponible,
                    Motivo = $"Venta #{venta.Id}",
                    FechaMovimiento = DateTime.UtcNow,
                    UsuarioId = usuarioId
                };
                _context.MovimientosInventario.Add(movimiento);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return VentaOperacionResult.Success(venta.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return VentaOperacionResult.Failure($"Error al registrar la venta: {ex.Message}");
        }
    }
}
