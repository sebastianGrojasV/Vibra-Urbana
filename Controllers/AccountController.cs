using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using System.Security.Cryptography;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[ResponseCache(Location = ResponseCacheLocation.None, NoStore = true)]
public class AccountController : Controller
{
    private readonly IUsuarioAuthenticationService _authenticationService;
    private readonly IUsuarioRegistrationService _usuarioRegistrationService;
    private readonly INavigationMenuService _navigationMenuService;
    private readonly IDataProtector _rememberedUserProtector;
    private const string RememberedUserCookie = "VibraUrbana.RememberedUser";

    public AccountController(
        IUsuarioAuthenticationService authenticationService,
        IUsuarioRegistrationService usuarioRegistrationService,
        INavigationMenuService navigationMenuService,
        IDataProtectionProvider dataProtectionProvider)
    {
        _authenticationService = authenticationService;
        _usuarioRegistrationService = usuarioRegistrationService;
        _navigationMenuService = navigationMenuService;
        _rememberedUserProtector = dataProtectionProvider.CreateProtector("VibraUrbana.RememberedUser.v1");
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return await RedirectToPanelAsync(User);
        }

        var model = new LoginViewModel { ReturnUrl = returnUrl };
        if (Request.Cookies.TryGetValue(RememberedUserCookie, out var rememberedUser))
        {
            try
            {
                model.Correo = _rememberedUserProtector.Unprotect(rememberedUser);
                model.Recordarme = true;
            }
            catch (CryptographicException)
            {
                Response.Cookies.Delete(RememberedUserCookie);
            }
        }
        return View(model);
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
            ModelState.AddModelError(string.Empty, "La cuenta o su rol está desactivado. Contacta al administrador.");
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
            new("RolId", result.Usuario.RolId.ToString()),
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
                AllowRefresh = true,
                ExpiresUtc = model.Recordarme ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddHours(8)
            });

        if (model.Recordarme)
        {
            Response.Cookies.Append(RememberedUserCookie,
                _rememberedUserProtector.Protect(result.Usuario.Correo), new CookieOptions
                {
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax,
                    IsEssential = true,
                    Expires = DateTimeOffset.UtcNow.AddDays(30)
                });
        }
        else
        {
            Response.Cookies.Delete(RememberedUserCookie);
        }

        if (!string.IsNullOrWhiteSpace(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return await RedirectToPanelAsync(principal);
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

    private async Task<IActionResult> RedirectToPanelAsync(ClaimsPrincipal principal)
    {
        var items = await _navigationMenuService.GetItemsAsync(principal);
        var panel = items.FirstOrDefault(item => item.Controller != "Home");
        return panel is null
            ? RedirectToAction("Index", "Home")
            : RedirectToAction(panel.Action, panel.Controller);
    }
}
