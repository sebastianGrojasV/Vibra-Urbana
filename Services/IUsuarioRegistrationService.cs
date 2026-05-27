using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IUsuarioRegistrationService
{
    Task<RegistroUsuarioResult> CreateAsync(CrearUsuarioViewModel model);

    Task<IReadOnlyList<Rol>> GetActiveRolesAsync();

    Task<IReadOnlyList<UsuarioListadoItemViewModel>> GetUsuariosAsync();

    Task<UsuarioDetalleViewModel?> GetUsuarioDetalleAsync(int id);

    Task<EditarUsuarioViewModel?> GetUsuarioParaEditarAsync(int id);

    Task<ActualizarUsuarioResult> UpdateAsync(EditarUsuarioViewModel model);

    Task<CambiarEstadoUsuarioResult> ChangeStatusAsync(int id, bool active);
}
