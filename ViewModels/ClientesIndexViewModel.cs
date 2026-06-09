namespace VibraUrbana.ViewModels;

public class ClientesIndexViewModel
{
    public string? Busqueda { get; set; }

    public IReadOnlyList<ClienteListadoItemViewModel> Clientes { get; set; } = [];
}
