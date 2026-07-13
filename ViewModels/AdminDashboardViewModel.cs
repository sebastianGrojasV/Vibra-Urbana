namespace VibraUrbana.ViewModels;

public class AdminDashboardViewModel
{
    public int VentasDelDia { get; set; }
    public decimal IngresosDelDia { get; set; }
    public decimal TotalVendido { get; set; }
    public int ProductosStockBajo { get; set; }
    public int ClientesRegistrados { get; set; }
}
