using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

public class AccountController : Controller
{
    private readonly IUsuarioAuthenticationService _authenticationService;
    private readonly IUsuarioRegistrationService _usuarioRegistrationService;

    public AccountController(
        IUsuarioAuthenticationService authenticationService,
        IUsuarioRegistrationService usuarioRegistrationService)
    {
        _authenticationService = authenticationService;
        _usuarioRegistrationService = usuarioRegistrationService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRolePanel(User.FindFirstValue(ClaimTypes.Role));
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para iniciar sesión.");
            return View(model);
        }

        var result = await _authenticationService.ValidateCredentialsAsync(model.Correo, model.Password);

        if (result.Result == LoginResult.InactiveUser)
        {
            ModelState.AddModelError(string.Empty, "El usuario está desactivado. Contacta al administrador.");
            return View(model);
        }

        if (result.Result == LoginResult.InvalidCredentials || result.Usuario is null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(model);
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.Usuario.Id.ToString()),
            new(ClaimTypes.Name, result.Usuario.NombreCompleto),
            new(ClaimTypes.Email, result.Usuario.Correo),
            new(ClaimTypes.Role, result.Usuario.Rol.Nombre)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = model.Recordarme,
                ExpiresUtc = model.Recordarme ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            });

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToRolePanel(result.Usuario.Rol.Nombre);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult RecuperarAcceso()
    {
        return View(new RecuperarAccesoViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RecuperarAcceso(RecuperarAccesoViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Ingresa un correo electrónico válido para iniciar la recuperación.");
            return View(model);
        }

        await _usuarioRegistrationService.ExistsByEmailAsync(model.Correo);

        TempData["RecoveryMessage"] = "Si el correo está registrado, un administrador podrá restablecer el acceso a la cuenta.";
        return RedirectToAction(nameof(RecuperarAcceso));
    }

    [HttpGet]
    [Authorize]
    public IActionResult AccessDenied()
    {
        Response.StatusCode = StatusCodes.Status403Forbidden;
        return View();
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction(nameof(Login));
    }

    private IActionResult RedirectToRolePanel(string? roleName)
    {
        return roleName switch
        {
            "Administrador" => RedirectToAction("Index", "Admin"),
            "Cajero" => RedirectToAction("Index", "Venta"),
            "Inventario" => RedirectToAction("Index", "Inventario"),
            "Consulta" => RedirectToAction("Index", "Reporte"),
            _ => RedirectToAction("Index", "Home")
        };
    }
}
