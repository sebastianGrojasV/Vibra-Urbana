using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

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
}
