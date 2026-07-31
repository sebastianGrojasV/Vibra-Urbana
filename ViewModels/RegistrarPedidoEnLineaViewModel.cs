using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class RegistrarPedidoEnLineaViewModel
{
    [Required(ErrorMessage = "El nombre completo es obligatorio.")]
    [StringLength(120, ErrorMessage = "El nombre completo no puede superar 120 caracteres.")]
    public string NombreCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [RegularExpression(@"^[0-9]{9}$", ErrorMessage = "La cédula debe tener 9 dígitos numéricos.")]
    public string CedulaCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El teléfono debe tener 8 dígitos numéricos.")]
    public string TelefonoCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo válido.")]
    [StringLength(120, ErrorMessage = "El correo no puede superar 120 caracteres.")]
    public string CorreoCliente { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección de entrega es obligatoria.")]
    [StringLength(250, ErrorMessage = "La dirección no puede superar 250 caracteres.")]
    public string DireccionEntrega { get; set; } = string.Empty;

    [Required(ErrorMessage = "La referencia SINPE es obligatoria.")]
    [StringLength(80, ErrorMessage = "La referencia SINPE no puede superar 80 caracteres.")]
    public string ReferenciaPago { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La observación no puede superar 250 caracteres.")]
    public string? ObservacionPedido { get; set; }

    [Required(ErrorMessage = "Debes agregar al menos un producto al carrito.")]
    [MinLength(1, ErrorMessage = "Debes agregar al menos un producto al carrito.")]
    public List<RegistrarPedidoItemViewModel> Items { get; set; } = [];
}

public class RegistrarPedidoItemViewModel
{
    [Required(ErrorMessage = "El producto es obligatorio.")]
    public int ProductoId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero.")]
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
