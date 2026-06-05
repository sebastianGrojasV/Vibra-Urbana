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
            model.Categorias = (await _productoServicio.PrepararCrearAsync()).Categorias;
            return View(model);
        }

        await _productoServicio.CrearAsync(model);
        TempData["SuccessMessage"] = "Producto registrado correctamente.";
        return RedirectToAction(nameof(Index));
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
            var persistedModel = await _productoServicio.ObtenerProductoParaEditarAsync(model.Id);
            model.Categorias = persistedModel?.Categorias ?? Enumerable.Empty<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>();
            return View(model);
        }

        var actualizado = await _productoServicio.ActualizarAsync(model);

        if (!actualizado)
        {
            return NotFound();
        }

        TempData["SuccessMessage"] = "Producto actualizado correctamente.";
        return RedirectToAction(nameof(Index));
    }
}
