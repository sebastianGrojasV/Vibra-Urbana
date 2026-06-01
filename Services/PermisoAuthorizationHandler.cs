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
        var roleName = context.User.FindFirstValue(ClaimTypes.Role);

        if (string.IsNullOrWhiteSpace(roleName))
        {
            return;
        }

        var tienePermiso = await _context.Roles
            .Where(rol => rol.Activo && rol.Nombre == roleName)
            .SelectMany(rol => rol.RolPermisos)
            .AnyAsync(rolPermiso =>
                rolPermiso.Permiso.Activo &&
                rolPermiso.Permiso.Nombre == requirement.Permiso);

        if (tienePermiso)
        {
            context.Succeed(requirement);
        }
    }
}
