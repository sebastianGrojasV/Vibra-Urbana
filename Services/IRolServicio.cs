using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IRolServicio
{
    Task<List<Rol>> ObtenerRolesAsync();

    Task<bool> AgregarRolAsync(CrearRolViewModel model);

    Task<EditarRolViewModel?> ObtenerRolParaEditarAsync(int id);

    Task<ActualizarRolResult> ActualizarRolAsync(EditarRolViewModel model);

    Task<Rol?> ObtenerRolPorIdAsync(int id);

    Task<CambiarEstadoRolResult> CambiarEstadoAsync(int id, bool active);
}
