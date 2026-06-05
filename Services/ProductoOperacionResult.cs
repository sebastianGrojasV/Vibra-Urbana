namespace VibraUrbana.Services;

public enum ProductoOperacionResult
{
    Success,
    NotFound,
    CategoriaNotFound,
    InvalidPrice,
    InvalidInventoryQuantity,
    InvalidMinimumStock
}
