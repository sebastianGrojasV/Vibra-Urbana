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

        public async Task<bool> ActivarRolAsync(int id)
        {
            var rol = await _rolRepositorio.ObtenerRolPorIdAsync(id);
            if (rol == null)
                return false;

            if (rol.Activo)
                return false; // Ya está activo

            return await _rolRepositorio.ActivarRolAsync(id);

        }

        public async Task<bool> AgregarRolAsync(Rol rol)
        {
            var rolesExistentes = await _rolRepositorio.ObtenerRolesAsync();
            if (rolesExistentes.Any(r => r.Nombre == rol.Nombre))
                return false;

            await _rolRepositorio.AgregarRolAsync(rol);
            return true;

        }

        public async Task<bool> EliminarRolAsync(int id)
        {
            //  verificar que el rol exista y esté activo
            var rol = await _rolRepositorio.ObtenerRolPorIdAsync(id);
            if (rol == null)
                return false;

            if (!rol.Activo)
                return false; // Ya estaba inactivo

            //  marcarlo como inactivo
            return await _rolRepositorio.EliminarRolAsync(id);

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
