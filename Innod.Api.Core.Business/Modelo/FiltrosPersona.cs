namespace Softmed.V3.Core.Business.Modelo;

/// <summary>Filtros de búsqueda y paginación para personas.</summary>
public sealed class FiltrosPersona
{
    public string? Identificador      { get; set; }
    public string? NombresApellidos   { get; set; }
    public int     Pagina             { get; set; } = 1;
    public int     RegistrosPorPagina { get; set; } = 50;

    public int Offset => (Pagina < 1 ? 0 : Pagina - 1) * RegistrosPorPagina;
}
