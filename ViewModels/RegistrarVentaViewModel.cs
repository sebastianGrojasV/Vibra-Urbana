using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class RegistrarVentaViewModel
{
    [Required(ErrorMessage = "El cliente es obligatorio.")]
    public int ClienteId { get; set; }

    [Required(ErrorMessage = "El método de pago es obligatorio.")]
    public int MetodoPagoId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo.")]
    public decimal Descuento { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El impuesto no puede ser negativo.")]
    public decimal Impuesto { get; set; }

    [MaxLength(500, ErrorMessage = "La observación no puede superar los 500 caracteres.")]
    public string? Observacion { get; set; } = string.Empty;

    [Required(ErrorMessage = "La venta debe incluir al menos un detalle.")]
    public List<DetalleVentaViewModel> Detalles { get; set; } = new();
}
