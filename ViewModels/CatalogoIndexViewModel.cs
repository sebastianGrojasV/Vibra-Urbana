using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class CatalogoIndexViewModel
{
    public int? CategoriaId { get; set; }

    public string? Busqueda { get; set; }

    public IReadOnlyList<CatalogoProductoViewModel> Productos { get; set; } = [];

    public IEnumerable<SelectListItem> Categorias { get; set; } = Enumerable.Empty<SelectListItem>();
}
