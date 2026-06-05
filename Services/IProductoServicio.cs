using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IProductoServicio
{
    Task<ProductosIndexViewModel> ObtenerProductosAsync(int? categoriaId, string? talla, string? color, bool? activo);

    Task<ProductoFormViewModel> PrepararCrearAsync();

    Task<ProductoFormViewModel?> ObtenerProductoParaEditarAsync(int id);

    Task CrearAsync(ProductoFormViewModel model);

    Task<bool> ActualizarAsync(ProductoFormViewModel model);

    Task<InventarioIndexViewModel> ObtenerInventarioAsync(int? categoriaId, string? talla, string? color, bool? activo);
}
