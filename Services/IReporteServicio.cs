using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IReporteServicio
{
    Task<ReporteInventarioViewModel> ObtenerInventarioAsync(int? categoriaId, string? estadoStock);
}
