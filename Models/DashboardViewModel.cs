namespace Dashboard.Models;

public enum VistaPeriodo
{
    Dia,
    Semana,
    Mes,
}

public record Periodo(VistaPeriodo Vista, DateOnly Desde, DateOnly Hasta, DateOnly Referencia, string Etiqueta);

public class DistribucionCategoria
{
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = "";
    public string Color { get; set; } = "";
    public int Minutos { get; set; }
    public decimal Porcentaje { get; set; }
}

public class TotalPorDia
{
    public DateOnly Fecha { get; set; }
    public int Minutos { get; set; }
}

public class DashboardViewModel
{
    public Periodo Periodo { get; set; } = default!;
    public DateOnly Hoy { get; set; }
    public int? FiltroCategoriaId { get; set; }
    public IReadOnlyList<Categoria> Categorias { get; set; } = [];

    public int TotalMinutos { get; set; }
    public IReadOnlyList<DistribucionCategoria> Distribucion { get; set; } = [];
    public IReadOnlyList<TotalPorDia> DesgloseDiario { get; set; } = [];
    public int MinutosFinDeSemana { get; set; }
    public IReadOnlyList<Registro> Registros { get; set; } = [];
    public int TotalRegistros { get; set; }
    public int Pagina { get; set; } = 1;
    public int TotalPaginas { get; set; } = 1;
    public int RegistrosPorPagina { get; set; }
    public bool UsaPaginacion { get; set; }

    public RegistroFormModel Captura { get; set; } = new();

    public DateOnly Anterior { get; set; }
    public DateOnly Siguiente { get; set; }
}
