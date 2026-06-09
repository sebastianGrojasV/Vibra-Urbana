using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IClienteServicio
{
    Task<IReadOnlyList<ClienteListadoItemViewModel>> ObtenerClientesAsync(string? busqueda);

    Task<CrearClienteResult> CrearAsync(CrearClienteViewModel model);

    Task<EditarClienteViewModel?> ObtenerClienteParaEditarAsync(int id);

    Task<ActualizarClienteResult> ActualizarAsync(EditarClienteViewModel model);

    Task<CambiarEstadoClienteResult> CambiarEstadoAsync(int id, bool active);
}
