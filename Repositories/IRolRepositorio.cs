using VibraUrbana.Models;

namespace VibraUrbana.Repositories
{
    public interface IRolRepositorio
    {
        Task<List<Rol>> ObtenerRolesAsync();
        Task<Rol> ObtenerRolPorIdAsync(int id);
        Task<bool> AgregarRolAsync(Rol rol);
        Task<bool> ActualizarRolAsync(Rol rol);
        Task<bool> EliminarRolAsync(int id);



    }
}
