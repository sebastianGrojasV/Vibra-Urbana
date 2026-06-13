using System.Collections.Generic;
using System.Threading.Tasks;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IVentaServicio
{
    Task<IReadOnlyList<Venta>> ObtenerVentasAsync();

    Task<VentaOperacionResult> RegistrarVentaAsync(RegistrarVentaViewModel model, int usuarioId);
}
