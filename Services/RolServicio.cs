using VibraUrbana.Models;
using VibraUrbana.Repositories;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class RolServicio : IRolServicio
{
    private readonly IRolRepositorio _rolRepositorio;

    public RolServicio(IRolRepositorio rolRepositorio)
    {
        _rolRepositorio = rolRepositorio;
    }

    public async Task<List<Rol>> ObtenerRolesAsync()
    {
        var roles = await _rolRepositorio.ObtenerRolesAsync();
        return roles.OrderBy(rol => rol.Nombre).ToList();
    }

    public async Task<Rol?> ObtenerRolPorIdAsync(int id)
    {
        return await _rolRepositorio.ObtenerRolPorIdAsync(id);
    }

    public async Task<bool> AgregarRolAsync(CrearRolViewModel model)
    {
        var nombre = model.Nombre.Trim();
        var rolesExistentes = await _rolRepositorio.ObtenerRolesAsync();

        if (rolesExistentes.Any(rol => rol.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        var rol = new Rol
        {
            Nombre = nombre,
            Descripcion = model.Descripcion.Trim(),
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        };

        await _rolRepositorio.AgregarRolAsync(rol);
        return true;
    }

    public async Task<bool> EliminarRolAsync(int id)
    {
        var rol = await _rolRepositorio.ObtenerRolPorIdAsync(id);

        if (rol is null || !rol.Activo)
        {
            return false;
        }

        return await _rolRepositorio.EliminarRolAsync(id);
    }

    public async Task<bool> ActivarRolAsync(int id)
    {
        var rol = await _rolRepositorio.ObtenerRolPorIdAsync(id);

        if (rol is null || rol.Activo)
        {
            return false;
        }

        return await _rolRepositorio.ActivarRolAsync(id);
    }
}
