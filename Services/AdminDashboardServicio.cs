using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class AdminDashboardServicio : IAdminDashboardServicio
{
    private readonly ApplicationDbContext _context;
    private readonly IFechaHoraServicio _fechaHoraServicio;

    public AdminDashboardServicio(
        ApplicationDbContext context,
        IFechaHoraServicio fechaHoraServicio)
    {
        _context = context;
        _fechaHoraServicio = fechaHoraServicio;
    }

    public async Task<AdminDashboardViewModel> ObtenerDashboardAsync()
    {
        var fechaInicio = _fechaHoraServicio.HoyCostaRica;
        var rangoUtc = _fechaHoraServicio.ObtenerRangoUtcCostaRica(fechaInicio, fechaInicio);

        var ventasDelDia = _context.Ventas
            .AsNoTracking()
            .Where(venta => venta.FechaVenta >= rangoUtc.InicioUtc && venta.FechaVenta < rangoUtc.FinExclusivoUtc);

        var ventasAprobadasDelDia = ventasDelDia
            .Where(venta => venta.Estado == "Aprobada");

        var pedidosWebDelDia = await _context.Pedidos
            .AsNoTracking()
            .Include(pedido => pedido.MetodoPago)
            .Where(pedido => pedido.VentaId == null)
            .Where(pedido => pedido.FechaPedido >= rangoUtc.InicioUtc && pedido.FechaPedido < rangoUtc.FinExclusivoUtc)
            .Select(pedido => new
            {
                pedido.Estado,
                pedido.Total,
                MetodoPago = pedido.MetodoPago.Nombre
            })
            .ToListAsync();

        var pedidosWebAprobados = pedidosWebDelDia
            .Where(pedido => EsPedidoAprobadoParaReporte(pedido.Estado))
            .ToList();

        var ventasPorMetodo = await ventasAprobadasDelDia
            .GroupBy(venta => venta.MetodoPago.Nombre)
            .Select(grupo => new AdminDashboardMetodoPagoViewModel
            {
                MetodoPago = grupo.Key,
                CantidadVentas = grupo.Count(),
                Total = grupo.Sum(venta => venta.Total)
            })
            .ToListAsync();

        foreach (var grupoPedido in pedidosWebAprobados.GroupBy(pedido => pedido.MetodoPago))
        {
            var existente = ventasPorMetodo.SingleOrDefault(item => item.MetodoPago == grupoPedido.Key);

            if (existente is null)
            {
                ventasPorMetodo.Add(new AdminDashboardMetodoPagoViewModel
                {
                    MetodoPago = grupoPedido.Key,
                    CantidadVentas = grupoPedido.Count(),
                    Total = grupoPedido.Sum(pedido => pedido.Total)
                });
            }
            else
            {
                existente.CantidadVentas += grupoPedido.Count();
                existente.Total += grupoPedido.Sum(pedido => pedido.Total);
            }
        }

        return new AdminDashboardViewModel
        {
            Fecha = fechaInicio,
            VentasDelDia = await ventasDelDia.CountAsync() + pedidosWebDelDia.Count,
            TotalVendidoDia = (await ventasAprobadasDelDia.SumAsync(venta => (decimal?)venta.Total) ?? 0) +
                pedidosWebAprobados.Sum(pedido => pedido.Total),
            ProductosStockBajo = await _context.Inventario
                .AsNoTracking()
                .CountAsync(inventario => inventario.CantidadDisponible <= inventario.StockMinimo),
            ClientesRegistrados = await _context.Clientes
                .AsNoTracking()
                .CountAsync(),
            VentasAprobadasDia = await ventasDelDia.CountAsync(venta => venta.Estado == "Aprobada") +
                pedidosWebDelDia.Count(pedido => EsPedidoAprobadoParaReporte(pedido.Estado)),
            VentasPendientesDia = await ventasDelDia.CountAsync(venta => venta.Estado == "Pendiente") +
                pedidosWebDelDia.Count(pedido => pedido.Estado == PedidoServicio.EstadoPendiente),
            VentasAnuladasDia = await ventasDelDia.CountAsync(venta => venta.Estado == "Anulada") +
                pedidosWebDelDia.Count(pedido => pedido.Estado == PedidoServicio.EstadoCancelado),
            VentasPorMetodoPago = ventasPorMetodo
                .OrderByDescending(item => item.Total)
                .ToList(),
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

    private static bool EsPedidoAprobadoParaReporte(string estado)
    {
        return estado is PedidoServicio.EstadoConfirmado or PedidoServicio.EstadoEnviado or PedidoServicio.EstadoEnTransito or PedidoServicio.EstadoEntregado;
    }
}
