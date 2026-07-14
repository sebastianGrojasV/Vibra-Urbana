using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IAdminDashboardServicio
{
    Task<AdminDashboardViewModel> ObtenerDashboardAsync();
}
