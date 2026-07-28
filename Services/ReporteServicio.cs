using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class ReporteServicio : IReporteServicio
{
    private readonly ApplicationDbContext _context;
    private readonly IFechaHoraServicio _fechaHoraServicio;

    public ReporteServicio(
        ApplicationDbContext context,
        IFechaHoraServicio fechaHoraServicio)
    {
        _context = context;
        _fechaHoraServicio = fechaHoraServicio;
    }

    public async Task<ReporteInventarioViewModel> ObtenerInventarioAsync(
        int? categoriaId,
        string? estadoStock)
    {
        var query = _context.Productos
            .AsNoTracking()
            .Where(producto => producto.Inventario != null);

        if (categoriaId.HasValue)
        {
            query = query.Where(producto => producto.CategoriaId == categoriaId.Value);
        }

        estadoStock = NormalizarEstadoStock(estadoStock);

        if (estadoStock == ReporteInventarioViewModel.EstadoStockBajo)
        {
            query = query.Where(producto =>
                producto.Inventario!.CantidadDisponible <= producto.Inventario.StockMinimo);
        }
        else if (estadoStock == ReporteInventarioViewModel.EstadoStockDisponible)
        {
            query = query.Where(producto =>
                producto.Inventario!.CantidadDisponible > producto.Inventario.StockMinimo);
        }

        return new ReporteInventarioViewModel
        {
            CategoriaId = categoriaId,
            EstadoStock = estadoStock,
            Inventario = await query
                .OrderBy(producto => producto.Nombre)
                .Select(producto => new InventarioListadoItemViewModel
                {
                    ProductoId = producto.Id,
                    Producto = producto.Nombre,
                    Categoria = producto.Categoria.Nombre,
                    Talla = producto.Talla,
                    Color = producto.Color,
                    Activo = producto.Activo,
                    CantidadDisponible = producto.Inventario!.CantidadDisponible,
                    StockMinimo = producto.Inventario.StockMinimo,
                    FechaActualizacion = producto.Inventario.FechaActualizacion
                })
                .ToListAsync(),
            Categorias = await ObtenerCategoriasAsync(categoriaId)
        };
    }

    public async Task<ReporteVentasViewModel> ObtenerVentasAsync(
        string? periodo,
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        var rango = CalcularRangoVentas(periodo, fechaInicio, fechaFin);
        var rangoUtc = _fechaHoraServicio.ObtenerRangoUtcCostaRica(rango.FechaInicio, rango.FechaFin);

        var ventas = await _context.Ventas
            .AsNoTracking()
            .Include(venta => venta.Cliente)
            .Include(venta => venta.Usuario)
            .Include(venta => venta.MetodoPago)
            .Include(venta => venta.Factura)
            .Where(venta => venta.FechaVenta >= rangoUtc.InicioUtc && venta.FechaVenta < rangoUtc.FinExclusivoUtc)
            .OrderByDescending(venta => venta.FechaVenta)
            .Select(venta => new ReporteVentasItemViewModel
            {
                VentaId = venta.Id,
                NumeroFactura = venta.Factura == null ? "Sin comprobante" : venta.Factura.NumeroFactura,
                FechaVenta = venta.FechaVenta,
                Cliente = venta.Cliente.NombreCompleto,
                Cajero = venta.Usuario.NombreCompleto,
                MetodoPago = venta.MetodoPago.Nombre,
                Subtotal = venta.Subtotal,
                Descuento = venta.Descuento,
                Impuesto = venta.Impuesto,
                Total = venta.Total,
                Estado = venta.Estado
            })
            .ToListAsync();

        foreach (var venta in ventas)
        {
            venta.FechaVenta = _fechaHoraServicio.ConvertirUtcACostaRica(venta.FechaVenta);
        }

        return new ReporteVentasViewModel
        {
            Periodo = rango.Periodo,
            FechaInicio = rango.FechaInicio,
            FechaFin = rango.FechaFin,
            Ventas = ventas,
            TotalesPorMetodoPago = ventas
                .Where(venta => venta.Estado == "Aprobada")
                .GroupBy(venta => venta.MetodoPago)
                .OrderBy(grupo => grupo.Key)
                .Select(grupo => new ReporteVentasMetodoPagoViewModel
                {
                    MetodoPago = grupo.Key,
                    CantidadVentas = grupo.Count(),
                    Total = grupo.Sum(venta => venta.Total)
                })
                .ToList()
        };
    }

    private async Task<IReadOnlyList<SelectListItem>> ObtenerCategoriasAsync(int? categoriaId)
    {
        return await _context.Categorias
            .AsNoTracking()
            .Where(categoria => categoria.Activo)
            .OrderBy(categoria => categoria.Nombre)
            .Select(categoria => new SelectListItem
            {
                Value = categoria.Id.ToString(),
                Text = categoria.Nombre,
                Selected = categoriaId == categoria.Id
            })
            .ToListAsync();
    }

    private static string? NormalizarEstadoStock(string? estadoStock)
    {
        return estadoStock?.Trim().ToLowerInvariant() switch
        {
            ReporteInventarioViewModel.EstadoStockBajo => ReporteInventarioViewModel.EstadoStockBajo,
            ReporteInventarioViewModel.EstadoStockDisponible => ReporteInventarioViewModel.EstadoStockDisponible,
            _ => null
        };
    }

    private (string Periodo, DateTime FechaInicio, DateTime FechaFin) CalcularRangoVentas(
        string? periodo,
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        var periodoNormalizado = NormalizarPeriodoVentas(periodo);
        var fechaBase = (fechaInicio ?? _fechaHoraServicio.HoyCostaRica).Date;

        return periodoNormalizado switch
        {
            ReporteVentasViewModel.PeriodoSemana => (
                periodoNormalizado,
                fechaBase.AddDays(-(((int)fechaBase.DayOfWeek + 6) % 7)),
                fechaBase.AddDays(-(((int)fechaBase.DayOfWeek + 6) % 7)).AddDays(6)),
            ReporteVentasViewModel.PeriodoMes => (
                periodoNormalizado,
                new DateTime(fechaBase.Year, fechaBase.Month, 1),
                new DateTime(fechaBase.Year, fechaBase.Month, 1).AddMonths(1).AddDays(-1)),
            ReporteVentasViewModel.PeriodoRango => CalcularRangoPersonalizado(fechaInicio, fechaFin),
            _ => (ReporteVentasViewModel.PeriodoDia, fechaBase, fechaBase)
        };
    }

    private (string Periodo, DateTime FechaInicio, DateTime FechaFin) CalcularRangoPersonalizado(
        DateTime? fechaInicio,
        DateTime? fechaFin)
    {
        var inicio = (fechaInicio ?? _fechaHoraServicio.HoyCostaRica).Date;
        var fin = (fechaFin ?? inicio).Date;

        if (fin < inicio)
        {
            (inicio, fin) = (fin, inicio);
        }

        return (ReporteVentasViewModel.PeriodoRango, inicio, fin);
    }

    private static string NormalizarPeriodoVentas(string? periodo)
    {
        return periodo?.Trim().ToLowerInvariant() switch
        {
            ReporteVentasViewModel.PeriodoSemana => ReporteVentasViewModel.PeriodoSemana,
            ReporteVentasViewModel.PeriodoMes => ReporteVentasViewModel.PeriodoMes,
            ReporteVentasViewModel.PeriodoRango => ReporteVentasViewModel.PeriodoRango,
            _ => ReporteVentasViewModel.PeriodoDia
        };
    }
}
