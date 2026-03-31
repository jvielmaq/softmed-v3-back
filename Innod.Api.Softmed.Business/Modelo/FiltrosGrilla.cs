namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Parámetros de búsqueda para la grilla de exámenes. Todos opcionales excepto paginación.</summary>
public sealed class FiltrosGrilla
{
    public DateTime? FechaDesde         { get; set; }
    public DateTime? FechaHasta         { get; set; }
    public int?      EstadoId           { get; set; }
    public int?      InstitucionId      { get; set; }
    public int       Pagina             { get; set; } = 1;
    public int       RegistrosPorPagina { get; set; } = 50;

    public int Offset => (Pagina < 1 ? 0 : Pagina - 1) * RegistrosPorPagina;
}
