using VibraUrbana.Models;
using VibraUrbana.Repositories;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public class RolServicio : IRolServicio
{
    private const string AdministratorRoleName = "Administrador";

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

        if (await _rolRepositorio.ExisteRolPorNombreAsync(nombre))
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

    public async Task<EditarRolViewModel?> ObtenerRolParaEditarAsync(int id)
    {
        var rol = await _rolRepositorio.ObtenerRolConPermisosAsync(id);

        if (rol is null)
        {
            return null;
        }

        var permisos = await _rolRepositorio.ObtenerPermisosActivosAsync();
        var permisosAsignados = rol.RolPermisos.Select(rolPermiso => rolPermiso.PermisoId).ToHashSet();

        return new EditarRolViewModel
        {
            Id = rol.Id,
            Nombre = rol.Nombre,
            Descripcion = rol.Descripcion,
            Activo = rol.Activo,
            PermisosSeleccionados = permisosAsignados.ToList(),
            Permisos = permisos.Select(permiso => new PermisoSeleccionViewModel
            {
                Id = permiso.Id,
                Nombre = permiso.Nombre,
                Descripcion = permiso.Descripcion,
                Seleccionado = permisosAsignados.Contains(permiso.Id)
            }).ToList()
        };
    }

    public async Task<ActualizarRolResult> ActualizarRolAsync(EditarRolViewModel model)
    {
        var rol = await _rolRepositorio.ObtenerRolConPermisosAsync(model.Id);

        if (rol is null)
        {
            return ActualizarRolResult.NotFound;
        }

        var nombre = model.Nombre.Trim();

        if (await _rolRepositorio.ExisteRolPorNombreAsync(nombre, model.Id))
        {
            return ActualizarRolResult.DuplicateName;
        }

        if (DebeProtegerUltimoAdministrador(rol, nombre, model.Activo) &&
            !await _rolRepositorio.ExisteOtroAdministradorActivoAsync(rol.Id))
        {
            return ActualizarRolResult.LastActiveAdministrator;
        }

        rol.Nombre = nombre;
        rol.Descripcion = model.Descripcion.Trim();
        rol.Activo = model.Activo;

        await _rolRepositorio.ActualizarRolAsync(rol);
        await _rolRepositorio.ActualizarPermisosAsync(rol.Id, model.PermisosSeleccionados);

        return ActualizarRolResult.Success;
    }

    public async Task<CambiarEstadoRolResult> CambiarEstadoAsync(int id, bool active)
    {
        var rol = await _rolRepositorio.ObtenerRolPorIdAsync(id);

        if (rol is null)
        {
            return CambiarEstadoRolResult.NotFound;
        }

        if (rol.Activo == active)
        {
            return CambiarEstadoRolResult.NoChange;
        }

        if (!active &&
            rol.Nombre == AdministratorRoleName &&
            !await _rolRepositorio.ExisteOtroAdministradorActivoAsync(rol.Id))
        {
            return CambiarEstadoRolResult.LastActiveAdministrator;
        }

        var actualizado = active
            ? await _rolRepositorio.ActivarRolAsync(id)
            : await _rolRepositorio.EliminarRolAsync(id);

        return actualizado ? CambiarEstadoRolResult.Success : CambiarEstadoRolResult.NotFound;
    }

    private static bool DebeProtegerUltimoAdministrador(Rol rol, string nuevoNombre, bool nuevoActivo)
    {
        return rol.Nombre == AdministratorRoleName &&
            rol.Activo &&
            (!nuevoActivo || nuevoNombre != AdministratorRoleName);
    }
}
