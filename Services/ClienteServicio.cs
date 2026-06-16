using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class ClienteServicio : IClienteServicio
{
    private readonly ApplicationDbContext _dbContext;

    public ClienteServicio(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ClienteListadoItemViewModel>> ObtenerClientesAsync(string? busqueda)
    {
        var query = _dbContext.Clientes.AsNoTracking();
        var criterio = busqueda?.Trim();

        if (!string.IsNullOrWhiteSpace(criterio))
        {
            query = query.Where(cliente =>
                cliente.Identificacion.Contains(criterio) ||
                cliente.NombreCompleto.Contains(criterio) ||
                cliente.Telefono.Contains(criterio) ||
                cliente.Correo.Contains(criterio));
        }

        return await query
            .OrderBy(cliente => cliente.NombreCompleto)
            .Select(cliente => new ClienteListadoItemViewModel
            {
                Id = cliente.Id,
                Identificacion = cliente.Identificacion,
                NombreCompleto = cliente.NombreCompleto,
                Telefono = cliente.Telefono,
                Correo = cliente.Correo,
                Direccion = cliente.Direccion,
                Activo = cliente.Activo
            })
            .ToListAsync();
    }

    public async Task<CrearClienteResult> CrearAsync(CrearClienteViewModel model)
    {
        var identificacion = model.Identificacion.Trim();

        if (await ExisteIdentificacionAsync(identificacion))
        {
            return CrearClienteResult.DuplicateIdentificacion;
        }

        var cliente = new Cliente
        {
            Identificacion = identificacion,
            NombreCompleto = model.NombreCompleto.Trim(),
            Telefono = model.Telefono.Trim(),
            Correo = model.Correo.Trim(),
            Direccion = model.Direccion.Trim(),
            Activo = true,
            FechaRegistro = DateTime.UtcNow
        };

        _dbContext.Clientes.Add(cliente);
        await _dbContext.SaveChangesAsync();

        return CrearClienteResult.Success;
    }

    public async Task<EditarClienteViewModel?> ObtenerClienteParaEditarAsync(int id)
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .Where(cliente => cliente.Id == id)
            .Select(cliente => new EditarClienteViewModel
            {
                Id = cliente.Id,
                Identificacion = cliente.Identificacion,
                NombreCompleto = cliente.NombreCompleto,
                Telefono = cliente.Telefono,
                Correo = cliente.Correo,
                Direccion = cliente.Direccion,
                Activo = cliente.Activo
            })
            .FirstOrDefaultAsync();
    }

    public async Task<ActualizarClienteResult> ActualizarAsync(EditarClienteViewModel model)
    {
        var cliente = await _dbContext.Clientes.FindAsync(model.Id);

        if (cliente is null)
        {
            return ActualizarClienteResult.NotFound;
        }

        var identificacion = model.Identificacion.Trim();

        if (await ExisteIdentificacionAsync(identificacion, model.Id))
        {
            return ActualizarClienteResult.DuplicateIdentificacion;
        }

        cliente.Identificacion = identificacion;
        cliente.NombreCompleto = model.NombreCompleto.Trim();
        cliente.Telefono = model.Telefono.Trim();
        cliente.Correo = model.Correo.Trim();
        cliente.Direccion = model.Direccion.Trim();
        cliente.Activo = model.Activo;

        await _dbContext.SaveChangesAsync();

        return ActualizarClienteResult.Success;
    }

    public async Task<CambiarEstadoClienteResult> CambiarEstadoAsync(int id, bool active)
    {
        var cliente = await _dbContext.Clientes.FindAsync(id);

        if (cliente is null)
        {
            return CambiarEstadoClienteResult.NotFound;
        }

        if (cliente.Activo == active)
        {
            return CambiarEstadoClienteResult.NoChange;
        }

        cliente.Activo = active;
        await _dbContext.SaveChangesAsync();

        return CambiarEstadoClienteResult.Success;
    }

    public async Task<ClienteHistorialComprasViewModel?> ObtenerHistorialComprasAsync(int clienteId)
    {
        var cliente = await _dbContext.Clientes
            .AsNoTracking()
            .Where(item => item.Id == clienteId)
            .Select(item => new
            {
                item.Id,
                item.Identificacion,
                item.NombreCompleto
            })
            .FirstOrDefaultAsync();

        if (cliente is null)
        {
            return null;
        }

        var compras = await _dbContext.Ventas
            .AsNoTracking()
            .Where(venta => venta.ClienteId == clienteId)
            .OrderByDescending(venta => venta.FechaVenta)
            .Select(venta => new ClienteHistorialCompraItemViewModel
            {
                VentaId = venta.Id,
                FechaVenta = venta.FechaVenta,
                Total = venta.Total,
                Estado = venta.Estado,
                NumeroComprobante = venta.Factura == null ? null : venta.Factura.NumeroFactura
            })
            .ToListAsync();

        return new ClienteHistorialComprasViewModel
        {
            ClienteId = cliente.Id,
            Identificacion = cliente.Identificacion,
            NombreCompleto = cliente.NombreCompleto,
            Compras = compras
        };
    }

    public async Task<ClienteCompraDetalleViewModel?> ObtenerDetalleCompraAsync(int clienteId, int ventaId)
    {
        return await _dbContext.Ventas
            .AsNoTracking()
            .Where(venta => venta.Id == ventaId && venta.ClienteId == clienteId)
            .Select(venta => new ClienteCompraDetalleViewModel
            {
                ClienteId = venta.ClienteId,
                Cliente = venta.Cliente.NombreCompleto,
                Identificacion = venta.Cliente.Identificacion,
                VentaId = venta.Id,
                FechaVenta = venta.FechaVenta,
                Cajero = venta.Usuario.NombreCompleto,
                MetodoPago = venta.MetodoPago.Nombre,
                Estado = venta.Estado,
                Observacion = venta.Observacion,
                NumeroComprobante = venta.Factura == null ? null : venta.Factura.NumeroFactura,
                Subtotal = venta.Subtotal,
                Descuento = venta.Descuento,
                Impuesto = venta.Impuesto,
                Total = venta.Total,
                Productos = venta.DetalleVentas
                    .OrderBy(detalle => detalle.Producto.Nombre)
                    .Select(detalle => new ClienteCompraDetalleItemViewModel
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
            .FirstOrDefaultAsync();
    }

    private async Task<bool> ExisteIdentificacionAsync(string identificacion, int? excludedId = null)
    {
        var normalizedIdentificacion = identificacion.Trim().ToLowerInvariant();

        return await _dbContext.Clientes.AnyAsync(cliente =>
            cliente.Identificacion.ToLower() == normalizedIdentificacion &&
            (!excludedId.HasValue || cliente.Id != excludedId.Value));
    }
}
