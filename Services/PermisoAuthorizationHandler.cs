using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;

namespace VibraUrbana.Services;

public class PermisoAuthorizationHandler : AuthorizationHandler<PermisoRequirement>
{
    private readonly ApplicationDbContext _context;

    public PermisoAuthorizationHandler(ApplicationDbContext context)
    {
        _context = context;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermisoRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true ||
            !int.TryParse(context.User.FindFirstValue(ClaimTypes.NameIdentifier), out var userId))
        {
            return;
        }

        var tienePermiso = await _context.Usuarios
            .Where(usuario => usuario.Id == userId && usuario.Activo && usuario.Rol.Activo)
            .SelectMany(usuario => usuario.Rol.RolPermisos)
            .AnyAsync(rolPermiso =>
                rolPermiso.Permiso.Activo &&
                rolPermiso.Permiso.Nombre == requirement.Permiso);

        if (tienePermiso)
        {
            context.Succeed(requirement);
        }
    }
}
