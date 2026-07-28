using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class CarritoController : Controller
{
    [HttpGet]
    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Confirmacion()
    {
        return View();
    }
}
