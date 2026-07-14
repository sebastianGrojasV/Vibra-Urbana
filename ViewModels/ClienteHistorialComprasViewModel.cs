namespace VibraUrbana.ViewModels;

public class ClienteHistorialComprasViewModel
{
    public int ClienteId { get; set; }

    public string Identificacion { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public IReadOnlyList<ClienteHistorialCompraItemViewModel> Compras { get; set; } = [];

    public int TotalCompras => Compras.Count;

    public decimal MontoTotal => Compras.Sum(compra => compra.Total);
}
