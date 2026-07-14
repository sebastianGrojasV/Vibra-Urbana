namespace VibraUrbana.Services;

public enum EntradaInventarioResult
{
    Success,
    NotFound,
    InvalidQuantity,
    QuantityOverflow,
    ConcurrencyConflict
}
