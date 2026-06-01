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

    public async Task<Rol?> ObtenerRolPorIdAsync(int id)
    {
        return await _context.Roles.FindAsync(id);
    }

    public async Task<bool> AgregarRolAsync(Rol rol)
    {
        _context.Roles.Add(rol);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ActualizarRolAsync(Rol rol)
    {
        _context.Roles.Update(rol);
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
