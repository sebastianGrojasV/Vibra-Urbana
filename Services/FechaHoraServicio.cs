namespace VibraUrbana.Services;

public class FechaHoraServicio : IFechaHoraServicio
{
    private readonly TimeZoneInfo _zonaHorariaCostaRica;

    public FechaHoraServicio()
    {
        _zonaHorariaCostaRica = ObtenerZonaHorariaCostaRica();
    }

    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime HoyCostaRica => ConvertirUtcACostaRica(UtcNow).Date;

    public DateTime ConvertirUtcACostaRica(DateTime fechaUtc)
    {
        var fechaUtcNormalizada = fechaUtc.Kind == DateTimeKind.Utc
            ? fechaUtc
            : DateTime.SpecifyKind(fechaUtc, DateTimeKind.Utc);

        return DateTime.SpecifyKind(
            TimeZoneInfo.ConvertTimeFromUtc(fechaUtcNormalizada, _zonaHorariaCostaRica),
            DateTimeKind.Unspecified);
    }

    public (DateTime InicioUtc, DateTime FinExclusivoUtc) ObtenerRangoUtcCostaRica(DateTime fechaInicio, DateTime fechaFin)
    {
        var inicioLocal = fechaInicio.Date;
        var finLocalExclusivo = fechaFin.Date.AddDays(1);

        if (finLocalExclusivo <= inicioLocal)
        {
            finLocalExclusivo = inicioLocal.AddDays(1);
        }

        return (
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(inicioLocal, DateTimeKind.Unspecified), _zonaHorariaCostaRica),
            TimeZoneInfo.ConvertTimeToUtc(DateTime.SpecifyKind(finLocalExclusivo, DateTimeKind.Unspecified), _zonaHorariaCostaRica));
    }

    private static TimeZoneInfo ObtenerZonaHorariaCostaRica()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central America Standard Time");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Costa_Rica");
        }
    }
}
