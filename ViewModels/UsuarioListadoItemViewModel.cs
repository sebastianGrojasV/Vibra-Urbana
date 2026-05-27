namespace VibraUrbana.ViewModels;

public class UsuarioListadoItemViewModel
{
    public int Id { get; set; }

    public string Cedula { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Rol { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
