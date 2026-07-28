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
    public async Task<IActionResult> Index(
        int? categoriaId,
        string? busqueda,
        string? talla,
        string? color,
        decimal? precioMinimo,
        decimal? precioMaximo)
    {
        var model = await _productoServicio.ObtenerCatalogoAsync(
            categoriaId,
            busqueda,
            talla,
            color,
            precioMinimo,
            precioMaximo);

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
        {
            return PartialView("_Productos", model);
        }

        return View(model);
    }
}
