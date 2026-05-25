using System.ComponentModel.DataAnnotations;

namespace VibraUrbana.ViewModels;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo electronico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingresa un correo electronico valido.")]
    [Display(Name = "Correo electronico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contrasena es obligatoria.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contrasena")]
    public string Password { get; set; } = string.Empty;

    [Display(Name = "Recordarme")]
    public bool Recordarme { get; set; }

    public string? ReturnUrl { get; set; }
}
