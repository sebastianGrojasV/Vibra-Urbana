using Microsoft.AspNetCore.Mvc;

namespace VibraUrbana.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }
}
