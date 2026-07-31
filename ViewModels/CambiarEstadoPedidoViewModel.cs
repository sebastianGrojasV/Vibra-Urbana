using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class CambiarEstadoPedidoViewModel
{
    [Required]
    public int PedidoId { get; set; }

    [Required]
    [StringLength(40)]
    public string NuevoEstado { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La observación no puede superar 250 caracteres.")]
    public string? Observacion { get; set; }
}
