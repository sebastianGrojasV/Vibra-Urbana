using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IUsuarioRegistrationService
{
    Task<RegistroUsuarioResult> CreateAsync(CrearUsuarioViewModel model);

    Task<IReadOnlyList<Rol>> GetActiveRolesAsync();
}
