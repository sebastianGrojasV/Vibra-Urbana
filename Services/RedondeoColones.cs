namespace VibraUrbana.Services;

public static class RedondeoColones
{
    public static decimal TotalACobrar(decimal importe, string metodoPago)
    {
        if (importe < 0) throw new ArgumentOutOfRangeException(nameof(importe));

        var total = decimal.Round(importe, 2, MidpointRounding.AwayFromZero);
        return string.Equals(metodoPago.Trim(), "Efectivo", StringComparison.OrdinalIgnoreCase)
            ? decimal.Round(total / 10m, 0, MidpointRounding.AwayFromZero) * 10m
            : total;
    }
}
