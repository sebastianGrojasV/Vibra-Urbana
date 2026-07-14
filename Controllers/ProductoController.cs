using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.ProductosGestionar)]
public class ProductoController : Controller
{
    private readonly IProductoServicio _productoServicio;

    public ProductoController(IProductoServicio productoServicio)
    {
        _productoServicio = productoServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? categoriaId, string? talla, string? color, bool? activo)
    {
        return View(await _productoServicio.ObtenerProductosAsync(categoriaId, talla, color, activo));
    }

    [HttpGet]
    public async Task<IActionResult> Crear()
    {
        return View(await _productoServicio.PrepararCrearAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(ProductoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Revisa los campos marcados antes de guardar el producto.");
            return View(await RepoblarCategoriasParaCrearAsync(model));
        }

        var result = await _productoServicio.CrearAsync(model);

        if (result == ProductoOperacionResult.Success)
        {
            TempData["SuccessMessage"] = "Producto registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        AddProductError(result);
        return View(await RepoblarCategoriasParaCrearAsync(model));
    }

    [HttpGet]
    public async Task<IActionResult> Editar(int id)
    {
        var model = await _productoServicio.ObtenerProductoParaEditarAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(ProductoFormViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Revisa los campos marcados antes de actualizar el producto.");
            return View(await RepoblarCategoriasParaEditarAsync(model));
        }

        var result = await _productoServicio.ActualizarAsync(model);

        if (result == ProductoOperacionResult.Success)
        {
            TempData["SuccessMessage"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (result == ProductoOperacionResult.NotFound)
        {
            return NotFound();
        }

        AddProductError(result);
        return View(await RepoblarCategoriasParaEditarAsync(model));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ConfirmarCambiarEstado(int id, bool active)
    {
        var result = await _productoServicio.CambiarEstadoAsync(id, active);

        if (result == CambiarEstadoProductoResult.NotFound)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = active
            ? "Producto activado correctamente."
            : "Producto desactivado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    private async Task<ProductoFormViewModel> RepoblarCategoriasParaCrearAsync(ProductoFormViewModel model)
    {
        model.Categorias = (await _productoServicio.PrepararCrearAsync()).Categorias;
        return model;
    }

    private async Task<ProductoFormViewModel> RepoblarCategoriasParaEditarAsync(ProductoFormViewModel model)
    {
        var persistedModel = await _productoServicio.ObtenerProductoParaEditarAsync(model.Id);
        model.Categorias = persistedModel?.Categorias ?? Enumerable.Empty<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
        return model;
    }

    private void AddProductError(ProductoOperacionResult result)
    {
        var message = result switch
        {
            ProductoOperacionResult.CategoriaNotFound => "La categoría seleccionada no está disponible.",
            ProductoOperacionResult.InvalidPrice => "El precio en colones debe ser mayor o igual a ₡0.",
            ProductoOperacionResult.InvalidInventoryQuantity => "La cantidad disponible debe ser mayor o igual a cero.",
            ProductoOperacionResult.InvalidMinimumStock => "El stock mínimo debe ser mayor o igual a cero.",
            _ => "No fue posible guardar el producto."
        };

        ModelState.AddModelError(string.Empty, message);
    }
}
