using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class FacturaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
