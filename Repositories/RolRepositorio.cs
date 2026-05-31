using Microsoft.EntityFrameworkCore;
using VibraUrbana.Data;
using VibraUrbana.Models;

namespace VibraUrbana.Repositories
{
    public class RolRepositorio : IRolRepositorio
    {
        private readonly ApplicationDbContext _context;

        public RolRepositorio(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ActivarRolAsync(int id)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
                return false;

            if (rol.Activo)
                return false; // Ya estaba activo

            rol.Activo = true;

            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();

            return true;

        }

        public Task<bool> ActualizarRolAsync(Rol rol)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> AgregarRolAsync(Rol rol)
        {
            _context.Roles.Add(rol);
            await _context.SaveChangesAsync();
            return true;

        }

        public async Task<bool> EliminarRolAsync(int id)
        {
            var rol = await _context.Roles.FindAsync(id);

            if (rol == null)
                return false; // No se encontró el rol

            //eliminado logico como inactivo
            rol.Activo = false;

            _context.Roles.Update(rol);
            await _context.SaveChangesAsync();

            return true; // Se actualizó correctamente

        }

        public async Task<List<Rol>> ObtenerRolesAsync()
        {
            return await _context.Roles.ToListAsync();

        }

        public async Task<Rol> ObtenerRolPorIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);

        }
    }
}
