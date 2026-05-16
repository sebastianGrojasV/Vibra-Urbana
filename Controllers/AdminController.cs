using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
