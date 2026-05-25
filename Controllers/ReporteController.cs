using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

[Authorize(Roles = "Administrador,Consulta")]
public class ReporteController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
