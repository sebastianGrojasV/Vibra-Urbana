using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VibraUrbana.Services;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Controllers;

[Authorize(Roles = "Administrador")]
public class AdminController : Controller
{
    private readonly IUsuarioRegistrationService _usuarioRegistrationService;

    public AdminController(IUsuarioRegistrationService usuarioRegistrationService)
    {
        _usuarioRegistrationService = usuarioRegistrationService;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Usuarios()
    {
        return View(await _usuarioRegistrationService.GetUsuariosAsync());
    }

    [HttpGet]
    public async Task<IActionResult> VerUsuario(int id)
    {
        var usuario = await _usuarioRegistrationService.GetUsuarioDetalleAsync(id);

        if (usuario is null)
        {
            return NotFound();
        }

        return View(usuario);
    }

    [HttpGet]
    public async Task<IActionResult> CrearUsuario()
    {
        return View(await BuildCrearUsuarioViewModelAsync());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CrearUsuario(CrearUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para registrar el usuario.");
            model.Roles = await BuildRoleSelectListAsync(model.RolId);
            return View(model);
        }

        var result = await _usuarioRegistrationService.CreateAsync(model);

        if (result == RegistroUsuarioResult.Success)
        {
            TempData["SuccessMessage"] = "Usuario registrado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        AddRegistrationError(result);
        model.Roles = await BuildRoleSelectListAsync(model.RolId);
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> ModificarUsuario(int id)
    {
        var model = await _usuarioRegistrationService.GetUsuarioParaEditarAsync(id);

        if (model is null)
        {
            return NotFound();
        }

        model.Roles = await BuildRoleSelectListAsync(model.RolId);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ModificarUsuario(EditarUsuarioViewModel model)
    {
        if (!ModelState.IsValid)
        {
            ModelState.AddModelError(string.Empty, "Completa los campos obligatorios para actualizar el usuario.");
            model.Roles = await BuildRoleSelectListAsync(model.RolId);
            return View(model);
        }

        var result = await _usuarioRegistrationService.UpdateAsync(model);

        if (result == ActualizarUsuarioResult.Success)
        {
            TempData["SuccessMessage"] = "Usuario actualizado correctamente.";
            return RedirectToAction(nameof(Usuarios));
        }

        if (result == ActualizarUsuarioResult.NotFound)
        {
            return NotFound();
        }

        AddUpdateError(result);
        model.Roles = await BuildRoleSelectListAsync(model.RolId);
        return View(model);
    }

    private async Task<CrearUsuarioViewModel> BuildCrearUsuarioViewModelAsync()
    {
        return new CrearUsuarioViewModel
        {
            Roles = await BuildRoleSelectListAsync()
        };
    }

    private async Task<IEnumerable<SelectListItem>> BuildRoleSelectListAsync(int? selectedRolId = null)
    {
        var roles = await _usuarioRegistrationService.GetActiveRolesAsync();

        return roles.Select(rol => new SelectListItem
        {
            Value = rol.Id.ToString(),
            Text = rol.Nombre,
            Selected = selectedRolId == rol.Id
        });
    }

    private void AddRegistrationError(RegistroUsuarioResult result)
    {
        var message = result switch
        {
            RegistroUsuarioResult.DuplicateCedula => "La cedula ya se encuentra registrada.",
            RegistroUsuarioResult.DuplicateCorreo => "El correo electronico ya se encuentra registrado.",
            RegistroUsuarioResult.RolNotFound => "El rol seleccionado no esta disponible.",
            _ => "No fue posible registrar el usuario."
        };

        ModelState.AddModelError(string.Empty, message);
    }

    private void AddUpdateError(ActualizarUsuarioResult result)
    {
        var message = result switch
        {
            ActualizarUsuarioResult.DuplicateCedula => "La cedula ya se encuentra registrada por otro usuario.",
            ActualizarUsuarioResult.DuplicateCorreo => "El correo electronico ya se encuentra registrado por otro usuario.",
            ActualizarUsuarioResult.RolNotFound => "El rol seleccionado no esta disponible.",
            _ => "No fue posible actualizar el usuario."
        };

        ModelState.AddModelError(string.Empty, message);
    }
}
