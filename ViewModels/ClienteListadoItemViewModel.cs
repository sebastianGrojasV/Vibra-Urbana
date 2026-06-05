namespace VibraUrbana.ViewModels;

public class ClienteListadoItemViewModel
{
    public int Id { get; set; }

    public string Identificacion { get; set; } = string.Empty;

    public string NombreCompleto { get; set; } = string.Empty;

    public string Telefono { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
