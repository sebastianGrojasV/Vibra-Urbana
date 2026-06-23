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
    public async Task<IActionResult> Index(int? categoriaId, string? talla, string? color, bool? activo, string? estadoStock)
    {
        return View(await _productoServicio.ObtenerInventarioAsync(categoriaId, talla, color, activo, estadoStock));
    }

    [HttpGet]
    public async Task<IActionResult> Entrada(int id)
    {
        var model = await _productoServicio.ObtenerEntradaInventarioAsync(id);

        return model is null ? NotFound() : View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Entrada(EntradaInventarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Ingresa una cantidad válida para registrar la entrada.");
            return View(await RecargarEntradaAsync(model));
        }

        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(userIdValue, out var usuarioId))
        {
            return Challenge();
        }

        var result = await _productoServicio.RegistrarEntradaInventarioAsync(model, usuarioId);

        if (result == EntradaInventarioResult.Success)
        {
            TempData["SuccessMessage"] = "Entrada de inventario registrada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        if (result == EntradaInventarioResult.NotFound)
        {
            return NotFound();
        }

        ModelState.AddModelError(string.Empty, GetEntryError(result));
        ModelState.Remove(nameof(EntradaInventarioViewModel.Version));
        return View(await RecargarEntradaAsync(model));
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

    private async Task<EntradaInventarioViewModel> RecargarEntradaAsync(EntradaInventarioViewModel model)
    {
        var persistedModel = await _productoServicio.ObtenerEntradaInventarioAsync(model.ProductoId);

        if (persistedModel is null)
        {
            return model;
        }

        persistedModel.CantidadIngresada = model.CantidadIngresada;
        persistedModel.Observacion = model.Observacion;
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

    private static string GetEntryError(EntradaInventarioResult result)
    {
        return result switch
        {
            EntradaInventarioResult.InvalidQuantity => "La cantidad ingresada debe ser mayor a cero.",
            EntradaInventarioResult.QuantityOverflow => "La entrada supera la cantidad máxima permitida para inventario.",
            EntradaInventarioResult.ConcurrencyConflict => "El inventario cambió mientras registrabas la entrada. Revisa la cantidad actual e inténtalo nuevamente.",
            _ => "No fue posible registrar la entrada de inventario."
        };
    }
}
