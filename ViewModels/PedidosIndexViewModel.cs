using Microsoft.AspNetCore.Mvc.Rendering;

namespace VibraUrbana.ViewModels;

public class PedidosIndexViewModel
{
    public string? Estado { get; set; }

    public DateTime? FechaInicio { get; set; }

    public DateTime? FechaFin { get; set; }

    public string? Cliente { get; set; }

    public IReadOnlyList<PedidoListadoItemViewModel> Pedidos { get; set; } = [];

    public IEnumerable<SelectListItem> Estados { get; set; } = Enumerable.Empty<SelectListItem>();
}

public class PedidoListadoItemViewModel
{
    public int Id { get; set; }

    public string Codigo => $"PED-{Id:D6}";

    public DateTime FechaPedido { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public string ReferenciaPago { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;
}
