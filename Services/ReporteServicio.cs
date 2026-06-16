using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class ReporteServicio : IReporteServicio
{
    private readonly ApplicationDbContext _context;

    public ReporteServicio(ApplicationDbContext context)
    {
        _context = context;
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
}
