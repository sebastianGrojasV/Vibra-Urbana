using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class AdminDashboardServicio : IAdminDashboardServicio
{
    private readonly ApplicationDbContext _context;

    public AdminDashboardServicio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AdminDashboardViewModel> ObtenerDashboardAsync()
    {
        var fechaInicio = DateTime.Today;
        var fechaFin = fechaInicio.AddDays(1);

        var ventasDelDia = _context.Ventas
            .AsNoTracking()
            .Where(venta => venta.FechaVenta >= fechaInicio && venta.FechaVenta < fechaFin);

        var ventasAprobadasDelDia = ventasDelDia
            .Where(venta => venta.Estado == "Aprobada");

        return new AdminDashboardViewModel
        {
            Fecha = fechaInicio,
            VentasDelDia = await ventasDelDia.CountAsync(),
            TotalVendidoDia = await ventasAprobadasDelDia.SumAsync(venta => (decimal?)venta.Total) ?? 0,
            ProductosStockBajo = await _context.Inventario
                .AsNoTracking()
                .CountAsync(inventario => inventario.CantidadDisponible <= inventario.StockMinimo),
            ClientesRegistrados = await _context.Clientes
                .AsNoTracking()
                .CountAsync(),
            VentasAprobadasDia = await ventasDelDia.CountAsync(venta => venta.Estado == "Aprobada"),
            VentasPendientesDia = await ventasDelDia.CountAsync(venta => venta.Estado == "Pendiente"),
            VentasAnuladasDia = await ventasDelDia.CountAsync(venta => venta.Estado == "Anulada"),
            VentasPorMetodoPago = await ventasAprobadasDelDia
                .GroupBy(venta => venta.MetodoPago.Nombre)
                .Select(grupo => new AdminDashboardMetodoPagoViewModel
                {
                    MetodoPago = grupo.Key,
                    CantidadVentas = grupo.Count(),
                    Total = grupo.Sum(venta => venta.Total)
                })
                .OrderByDescending(item => item.Total)
                .ToListAsync(),
            ProductosCriticos = await _context.Inventario
                .AsNoTracking()
                .Where(inventario => inventario.CantidadDisponible <= inventario.StockMinimo)
                .OrderBy(inventario => inventario.CantidadDisponible)
                .ThenBy(inventario => inventario.Producto.Nombre)
                .Select(inventario => new AdminDashboardStockItemViewModel
                {
                    Producto = inventario.Producto.Nombre,
                    Categoria = inventario.Producto.Categoria.Nombre,
                    CantidadDisponible = inventario.CantidadDisponible,
                    StockMinimo = inventario.StockMinimo
                })
                .Take(5)
                .ToListAsync()
        };
    }
}
