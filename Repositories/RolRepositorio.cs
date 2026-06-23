using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;

namespace VibraUrbana.Repositories;

public class RolRepositorio : IRolRepositorio
{
    private readonly ApplicationDbContext _context;

    public RolRepositorio(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Rol>> ObtenerRolesAsync()
    {
        return await _context.Roles.ToListAsync();
    }

    public async Task<List<Permiso>> ObtenerPermisosActivosAsync()
    {
        return await _context.Permisos
            .Where(permiso => permiso.Activo)
            .OrderBy(permiso => permiso.Nombre)
            .ToListAsync();
    }

    public async Task<Rol?> ObtenerRolPorIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<Rol?> ObtenerRolConPermisosAsync(int id)
    {
        return await _context.Roles
            .Include(rol => rol.RolPermisos)
            .ThenInclude(rolPermiso => rolPermiso.Permiso)
            .SingleOrDefaultAsync(rol => rol.Id == id);
    }

    public async Task<bool> ExisteRolPorNombreAsync(string nombre, int? excluirId = null)
    {
        var normalizedName = nombre.Trim().ToLower();

        return await _context.Roles.AnyAsync(rol =>
            rol.Nombre.ToLower() == normalizedName &&
            (!excluirId.HasValue || rol.Id != excluirId.Value));
    }

    public async Task<bool> ExisteOtroAdministradorActivoAsync(int id)
    {
        return await _context.Roles.AnyAsync(rol =>
            rol.Id != id &&
            rol.Activo &&
            rol.Nombre == "Administrador");
    }

    public async Task<bool> AgregarRolAsync(Rol rol)
    {
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarRolConPermisosAsync(Rol rol, IEnumerable<int> permisoIds)
    {
        var idsSeleccionados = permisoIds.Distinct().ToList();
        var permisosValidos = await _context.Permisos
            .Where(permiso => permiso.Activo && idsSeleccionados.Contains(permiso.Id))
            .Select(permiso => permiso.Id)
            .ToListAsync();

        if (permisosValidos.Count != idsSeleccionados.Count)
        {
            return false;
        }

        _context.RolPermisos.RemoveRange(rol.RolPermisos);

        var nuevosPermisos = permisosValidos
            .Select(permisoId => new RolPermiso
            {
                RolId = rol.Id,
                PermisoId = permisoId
            });

        await _context.RolPermisos.AddRangeAsync(nuevosPermisos);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> EliminarRolAsync(int id)
    {
        var rol = await _context.Roles.FindAsync(id);

        if (rol is null)
        {
            return false;
        }

        rol.Activo = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActivarRolAsync(int id)
    {
        var rol = await _context.Roles.FindAsync(id);

        if (rol is null)
        {
            return false;
        }

        rol.Activo = true;
        await _context.SaveChangesAsync();
        return true;
    }
}
