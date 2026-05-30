using VibraUrbana.Models;
using VibraUrbana.Repositories;

namespace VibraUrbana.Services
{
    public class RolServicio : IRolServicio
    {

        private readonly IRolRepositorio _rolRepositorio;
        public RolServicio(IRolRepositorio rolRepositorio)
        {
            _rolRepositorio = rolRepositorio;
        }

        public async Task<bool> AgregarRolAsync(Rol rol)
        {
            var rolesExistentes = await _rolRepositorio.ObtenerRolesAsync();
            if (rolesExistentes.Any(r => r.Nombre == rol.Nombre))
                return false;

            await _rolRepositorio.AgregarRolAsync(rol);
            return true;

        }

        public async Task<List<Rol>> ObtenerRolesAsync()
        {
            var roles = await _rolRepositorio.ObtenerRolesAsync();
            return roles.ToList();

        }

        public async Task<Rol> ObtenerRolPorIdAsync(int id)
        {
            var rol = await _rolRepositorio.ObtenerRolPorIdAsync(id);
            return rol;

        }
    }
}
