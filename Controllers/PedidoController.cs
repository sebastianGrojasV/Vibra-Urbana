using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class PedidoController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
