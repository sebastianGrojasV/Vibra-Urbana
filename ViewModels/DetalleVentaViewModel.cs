using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class DetalleVentaViewModel
{
    [Required]
    public int ProductoId { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser al menos 1.")]
    public int Cantidad { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "El precio unitario debe ser mayor que 0.")]
    public decimal PrecioUnitario { get; set; }
}
