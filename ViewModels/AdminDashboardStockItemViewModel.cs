namespace VibraUrbana.ViewModels;

public class AdminDashboardStockItemViewModel
{
    public string Producto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public int CantidadDisponible { get; set; }

    public int StockMinimo { get; set; }
}
