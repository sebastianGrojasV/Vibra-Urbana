using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class InventarioIndexViewModel
{
    public int? CategoriaId { get; set; }

    public string? Talla { get; set; }

    public string? Color { get; set; }

    public bool? Activo { get; set; }

    public IReadOnlyList<InventarioListadoItemViewModel> Inventario { get; set; } = [];

    public IEnumerable<SelectListItem> Categorias { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Tallas { get; set; } = Enumerable.Empty<SelectListItem>();

    public IEnumerable<SelectListItem> Colores { get; set; } = Enumerable.Empty<SelectListItem>();
}
