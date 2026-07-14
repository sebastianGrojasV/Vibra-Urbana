using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class RecuperarAccesoViewModel
{
    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electrónico válido.")]
    [Display(Name = "Correo electrónico")]
    public string Correo { get; set; } = string.Empty;
}
