namespace VibraUrbana.Services;

public interface IFechaHoraServicio
{
    DateTime UtcNow { get; }

    DateTime HoyCostaRica { get; }

    DateTime ConvertirUtcACostaRica(DateTime fechaUtc);

    (DateTime InicioUtc, DateTime FinExclusivoUtc) ObtenerRangoUtcCostaRica(DateTime fechaInicio, DateTime fechaFin);
}
