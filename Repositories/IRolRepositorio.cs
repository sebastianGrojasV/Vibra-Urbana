using VibraUrbana.Models;

namespace VibraUrbana.Repositories;

public interface IRolRepositorio
{
    Task<List<Rol>> ObtenerRolesAsync();

    Task<List<Permiso>> ObtenerPermisosActivosAsync();

    Task<Rol?> ObtenerRolPorIdAsync(int id);

    Task<Rol?> ObtenerRolConPermisosAsync(int id);

    Task<bool> ExisteRolPorNombreAsync(string nombre, int? excluirId = null);

    Task<bool> ExisteOtroAdministradorActivoAsync(int id);

    Task<bool> AgregarRolAsync(Rol rol);

    Task<bool> ActualizarRolConPermisosAsync(Rol rol, IEnumerable<int> permisoIds);

    Task<bool> EliminarRolAsync(int id);

    Task<bool> ActivarRolAsync(int id);
}
