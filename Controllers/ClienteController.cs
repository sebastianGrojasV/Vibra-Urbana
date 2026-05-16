using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class ClienteController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
