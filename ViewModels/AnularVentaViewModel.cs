using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class AnularVentaViewModel
{
    [Required]
    public int VentaId { get; set; }

    [Required(ErrorMessage = "El motivo de anulación es obligatorio.")]
    [StringLength(300, MinimumLength = 5, ErrorMessage = "El motivo debe tener entre 5 y 300 caracteres.")]
    public string Motivo { get; set; } = string.Empty;
}
