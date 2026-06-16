using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class ReporteInventarioViewModel
{
    public const string EstadoStockBajo = "bajo";
    public const string EstadoStockDisponible = "disponible";

    public int? CategoriaId { get; set; }

    public string? EstadoStock { get; set; }

    public IReadOnlyList<InventarioListadoItemViewModel> Inventario { get; set; } = [];

    public IEnumerable<SelectListItem> Categorias { get; set; } = Enumerable.Empty<SelectListItem>();

    public int TotalProductos => Inventario.Count;

    public int TotalUnidades => Inventario.Sum(item => item.CantidadDisponible);

    public int ProductosStockBajo => Inventario.Count(item => item.StockBajo);
}
