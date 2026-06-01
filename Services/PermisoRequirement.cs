using Microsoft.AspNetCore.Authorization;

namespace VibraUrbana.Services;

public class PermisoRequirement : IAuthorizationRequirement
{
    public PermisoRequirement(string permiso)
    {
        Permiso = permiso;
    }

    public string Permiso { get; }
}
