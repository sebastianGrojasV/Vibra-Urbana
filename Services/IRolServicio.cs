using VibraUrbana.Models;

namespace VibraUrbana.Services
{
    public interface IRolServicio
    {

        Task<List<Rol>> ObtenerRolesAsync();

        Task<bool> AgregarRolAsync(Rol rol);

        Task<Rol> ObtenerRolPorIdAsync(int id);

        Task<bool> EliminarRolAsync(int id);

        Task<bool> ActivarRolAsync(int id);

    }
}
