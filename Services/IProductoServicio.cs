using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IProductoServicio
{
    Task<ProductosIndexViewModel> ObtenerProductosAsync(int? categoriaId, string? talla, string? color, bool? activo);

    Task<ProductoFormViewModel> PrepararCrearAsync();

    Task<ProductoFormViewModel?> ObtenerProductoParaEditarAsync(int id);

    Task<ProductoOperacionResult> CrearAsync(ProductoFormViewModel model);

    Task<ProductoOperacionResult> ActualizarAsync(ProductoFormViewModel model);

    Task<CambiarEstadoProductoResult> CambiarEstadoAsync(int id, bool active);

    Task<InventarioIndexViewModel> ObtenerInventarioAsync(int? categoriaId, string? talla, string? color, bool? activo, string? estadoStock);

    Task<AjustarInventarioViewModel?> ObtenerAjusteInventarioAsync(int productoId);

    Task<AjustarInventarioResult> AjustarInventarioAsync(AjustarInventarioViewModel model, int usuarioId);

    Task<EntradaInventarioViewModel?> ObtenerEntradaInventarioAsync(int productoId);

    Task<EntradaInventarioResult> RegistrarEntradaInventarioAsync(EntradaInventarioViewModel model, int usuarioId);
}
