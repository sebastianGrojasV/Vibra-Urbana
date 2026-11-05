using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;

namespace VibraUrbana.Services;

public class ActiveUserCookieAuthenticationEvents : CookieAuthenticationEvents
{
    private readonly ApplicationDbContext _context;

    public ActiveUserCookieAuthenticationEvents(ApplicationDbContext context)
    {
        _context = context;
    }

    public override async Task ValidatePrincipal(CookieValidatePrincipalContext context)
    {
        var userIdValue = context.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
        var roleName = context.Principal?.FindFirstValue(ClaimTypes.Role);
        var roleIdValue = context.Principal?.FindFirstValue("RolId");

        if (!int.TryParse(userIdValue, out var userId) ||
            !int.TryParse(roleIdValue, out var roleId) || string.IsNullOrWhiteSpace(roleName))
        {
            await RejectPrincipalAsync(context);
            return;
        }

        var usuario = await _context.Usuarios
            .AsNoTracking()
            .Where(item => item.Id == userId)
            .Select(item => new
            {
                item.Activo,
                item.RolId,
                RolActivo = item.Rol.Activo,
                RolNombre = item.Rol.Nombre
            })
            .SingleOrDefaultAsync();

        if (usuario is null ||
            !usuario.Activo ||
            !usuario.RolActivo ||
            usuario.RolId != roleId ||
            usuario.RolNombre != roleName)
        {
            await RejectPrincipalAsync(context);
        }
    }

    private static async Task RejectPrincipalAsync(CookieValidatePrincipalContext context)
    {
        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
