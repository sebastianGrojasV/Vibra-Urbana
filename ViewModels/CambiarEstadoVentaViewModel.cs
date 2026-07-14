using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class CambiarEstadoVentaViewModel
{
    [Required]
    public int VentaId { get; set; }

    [Required(ErrorMessage = "Selecciona el nuevo estado de la venta.")]
    public string NuevoEstado { get; set; } = string.Empty;

    [Required(ErrorMessage = "El motivo del cambio de estado es obligatorio.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre 5 y 300 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}
