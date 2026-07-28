using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;

namespace VibraUrbana.Controllers;

public class CatalogoController : Controller
{
    private readonly IProductoServicio _productoServicio;

    public CatalogoController(IProductoServicio productoServicio)
    {
        _productoServicio = productoServicio;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int? categoriaId, string? busqueda)
    {
        return View(await _productoServicio.ObtenerCatalogoAsync(categoriaId, busqueda));
    }
}
