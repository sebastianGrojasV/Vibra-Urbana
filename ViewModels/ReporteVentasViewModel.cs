using System;
using System.Collections.Generic;
using VibraUrbana.Models;

namespace VibraUrbana.ViewModels;

public class ReporteVentasViewModel
{
    public string Filtro { get; set; } = "dia"; // dia, semana, mes, personalizado
    public DateTime? FechaInicio { get; set; }
    public DateTime? FechaFin { get; set; }

    public int TotalVentas { get; set; }
    public decimal MontoTotal { get; set; }
    public int VentasAprobadas { get; set; }
    public int VentasAnuladas { get; set; }

    public IReadOnlyList<Venta> Ventas { get; set; } = new List<Venta>();
}
