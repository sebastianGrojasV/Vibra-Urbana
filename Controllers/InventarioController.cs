using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Policy = PermisosSistema.InventarioGestionar)]
public class InventarioController : Controller
{
    private readonly IProductoServicio _productoServicio;

    public InventarioController(IProductoServicio productoServicio)
    {
        _productoServicio = productoServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? categoriaId, string? talla, string? color, bool? activo)
    {
        return View(await _productoServicio.ObtenerInventarioAsync(categoriaId, talla, color, activo));
    }

    [HttpGet]
    public async Task<IActionResult> Ajustar(int id)
    {
        var model = await _productoServicio.ObtenerAjusteInventarioAsync(id);

        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Ajustar(AjustarInventarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Revisa la cantidad y el motivo antes de guardar el ajuste.");
            return View(await RecargarAjusteAsync(model));
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var usuarioId))
        {
            return Challenge();
        }

        var result = await _productoServicio.AjustarInventarioAsync(model, usuarioId);

        if (result == AjustarInventarioResult.Success)
        {
            TempData["SuccessMessage"] = "Inventario ajustado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (result == AjustarInventarioResult.NotFound)
        {
            return NotFound();
        }

        ModelState.AddModelError(string.Empty, GetAdjustmentError(result));
        ModelState.Remove(nameof(AjustarInventarioViewModel.Version));
        return View(await RecargarAjusteAsync(model));
    }

    private async Task<AjustarInventarioViewModel> RecargarAjusteAsync(AjustarInventarioViewModel model)
    {
        var persistedModel = await _productoServicio.ObtenerAjusteInventarioAsync(model.ProductoId);

        if (persistedModel is null)
        {
            return model;
        }

        persistedModel.NuevaCantidad = model.NuevaCantidad;
        persistedModel.Motivo = model.Motivo;
        return persistedModel;
    }

    private static string GetAdjustmentError(AjustarInventarioResult result)
    {
        return result switch
        {
            AjustarInventarioResult.InvalidQuantity => "La nueva cantidad debe ser mayor o igual a cero.",
            AjustarInventarioResult.InvalidReason => "Ingresa un motivo válido de entre 5 y 300 caracteres.",
            AjustarInventarioResult.NoChange => "La nueva cantidad debe ser diferente de la cantidad actual.",
            AjustarInventarioResult.ConcurrencyConflict => "El inventario cambió mientras realizabas el ajuste. Revisa la cantidad actual e inténtalo nuevamente.",
            _ => "No fue posible realizar el ajuste de inventario."
        };
    }
}
