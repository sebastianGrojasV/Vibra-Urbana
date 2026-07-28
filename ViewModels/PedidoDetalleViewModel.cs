namespace VibraUrbana.ViewModels;

public class PedidoDetalleViewModel
{
    public int Id { get; set; }

    public string Codigo => $"PED-{Id:D6}";

    public DateTime FechaPedido { get; set; }

    public string NombreCliente { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string DireccionEntrega { get; set; } = string.Empty;

    public string MetodoPago { get; set; } = string.Empty;

    public decimal Subtotal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = string.Empty;

    public string Observacion { get; set; } = string.Empty;

    public string ReferenciaPago { get; set; } = string.Empty;

    public string ImagenComprobanteUrl { get; set; } = string.Empty;

    public string EstadoVerificacion { get; set; } = string.Empty;

    public DateTime? FechaComprobante { get; set; }

    public IReadOnlyList<PedidoDetalleProductoViewModel> Productos { get; set; } = [];
}

public class PedidoDetalleProductoViewModel
{
    public string Producto { get; set; } = string.Empty;

    public string Categoria { get; set; } = string.Empty;

    public string Talla { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Subtotal { get; set; }
}
