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

        public Task<bool> EliminarRolAsync(int id)
        {
            throw new NotImplementedException();
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
