using System.Collections.Generic;
using System.Threading.Tasks;
using VibraUrbana.Models;
using VibraUrbana.ViewModels;

namespace VibraUrbana.Services;

public interface IVentaServicio
{
    Task<IReadOnlyList<Venta>> ObtenerVentasAsync(int? clienteId = null);

    Task<VentaOperacionResult> RegistrarVentaAsync(RegistrarVentaViewModel model, int usuarioId);

    Task<AnularVentaResult> AnularVentaAsync(int ventaId, string motivo, int usuarioId);

    Task<CambiarEstadoVentaResult> CambiarEstadoVentaAsync(int ventaId, string nuevoEstado, string motivo, int usuarioId);
}
