using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class VentaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
