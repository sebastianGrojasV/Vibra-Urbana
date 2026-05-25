using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

[Authorize(Roles = "Administrador,Cajero")]
public class VentaController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
