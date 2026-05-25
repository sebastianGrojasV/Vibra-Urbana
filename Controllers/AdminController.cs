using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
