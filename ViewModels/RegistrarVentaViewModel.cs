using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class RegistrarVentaViewModel
{
    [Range(1, int.MaxValue, ErrorMessage = "Selecciona un cliente valido.")]
    public int ClienteId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El metodo de pago es obligatorio.")]
    public int MetodoPagoId { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El descuento no puede ser negativo.")]
    public decimal Descuento { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "El impuesto no puede ser negativo.")]
    public decimal Impuesto { get; set; }

    [MaxLength(500, ErrorMessage = "La observacion no puede superar los 500 caracteres.")]
    public string? Observacion { get; set; } = string.Empty;

    [MinLength(1, ErrorMessage = "La venta debe incluir al menos un producto.")]
    public List<DetalleVentaViewModel> Detalles { get; set; } = new();
}
