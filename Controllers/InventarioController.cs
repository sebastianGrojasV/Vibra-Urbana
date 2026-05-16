using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class InventarioController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
