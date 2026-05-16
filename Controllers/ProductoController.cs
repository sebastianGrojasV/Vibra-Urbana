using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class ProductoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
