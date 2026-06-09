namespace VibraUrbana.ViewModels;

public class RolListadoItemViewModel
{
    public int Id { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; }
}
