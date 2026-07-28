using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class CatalogoIndexViewModel
{
    public int? CategoriaId { get; set; }

    public string? Busqueda { get; set; }

    public string? Talla { get; set; }

    public string? Color { get; set; }

    public decimal? PrecioMinimo { get; set; }

    public decimal? PrecioMaximo { get; set; }

    public IReadOnlyList<CatalogoProductoViewModel> Productos { get; set; } = [];

    public IEnumerable<SelectListItem> Categorias { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Tallas { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Colores { get; set; } = Enumerable.Empty<SelectListItem>();
}
