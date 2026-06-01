using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IRolServicio
{
    Task<List<Rol>> ObtenerRolesAsync();

    Task<bool> AgregarRolAsync(CrearRolViewModel model);

    Task<Rol?> ObtenerRolPorIdAsync(int id);

    Task<bool> EliminarRolAsync(int id);

    Task<bool> ActivarRolAsync(int id);
}
