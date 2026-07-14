namespace VibraUrbana.Services;

public enum AjustarInventarioResult
{
    Success,
    NotFound,
    InvalidQuantity,
    InvalidReason,
    NoChange,
    ConcurrencyConflict
}
