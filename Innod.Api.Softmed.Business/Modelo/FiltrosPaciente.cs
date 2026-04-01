namespace Softmed.V3.Softmed.Business.Modelo;

/// <summary>Filtros de paginación para listado de pacientes.</summary>
public sealed class FiltrosPaciente
{
    public string? Busqueda           { get; set; }
    public int     Pagina             { get; set; } = 1;
    public int     RegistrosPorPagina { get; set; } = 50;

    public int Offset => (Pagina < 1 ? 0 : Pagina - 1) * RegistrosPorPagina;
}
