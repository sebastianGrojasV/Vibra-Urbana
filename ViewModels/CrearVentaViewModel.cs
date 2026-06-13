using System.Collections.Generic;
using VibraUrbana.Models;

namespace VibraUrbana.ViewModels;

public class CrearVentaViewModel
{
    public IReadOnlyList<ClienteListadoItemViewModel> Clientes { get; set; } = new List<ClienteListadoItemViewModel>();

    public IReadOnlyList<ProductoListadoItemViewModel> Productos { get; set; } = new List<ProductoListadoItemViewModel>();

    public IReadOnlyList<MetodoPago> MetodosPago { get; set; } = new List<MetodoPago>();
}
