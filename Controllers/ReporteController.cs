using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class ReporteController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
