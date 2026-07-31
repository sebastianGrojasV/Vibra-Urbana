using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class RegistrarPedidoEnLineaViewModel
{
    [Required]
    [StringLength(120)]
    public string NombreCliente { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^[0-9]{8}$")]
    public string TelefonoCliente { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(120)]
    public string CorreoCliente { get; set; } = string.Empty;

    [Required]
    [StringLength(250)]
    public string DireccionEntrega { get; set; } = string.Empty;

    [Required]
    [StringLength(80)]
    public string ReferenciaPago { get; set; } = string.Empty;

    [StringLength(250)]
    public string? ObservacionPedido { get; set; }

    [Required]
    [MinLength(1)]
    public List<RegistrarPedidoItemViewModel> Items { get; set; } = [];
}

public class RegistrarPedidoItemViewModel
{
    [Required]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Cantidad { get; set; }
}

public class RegistrarPedidoEnLineaResult
{
    private RegistrarPedidoEnLineaResult(bool esExitoso, string mensaje, int? pedidoId = null, string? codigo = null, decimal total = 0)
    {
        EsExitoso = esExitoso;
        Mensaje = mensaje;
        PedidoId = pedidoId;
        Codigo = codigo;
        Total = total;
    }

    public bool EsExitoso { get; }

    public string Mensaje { get; }

    public int? PedidoId { get; }

    public string? Codigo { get; }

    public decimal Total { get; }

    public static RegistrarPedidoEnLineaResult Success(int pedidoId, decimal total)
    {
        return new(true, "Pedido registrado correctamente.", pedidoId, $"PED-{pedidoId:D6}", total);
    }

    public static RegistrarPedidoEnLineaResult Failure(string mensaje)
    {
        return new(false, mensaje);
    }
}
