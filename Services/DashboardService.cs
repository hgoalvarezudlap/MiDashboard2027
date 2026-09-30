using Dashboard.Data;
using Dashboard.Models;

namespace Dashboard.Services;

public class DashboardService
{
    public const int RegistrosPorPagina = 10;

    private readonly RegistroRepository _registros;
    private readonly CategoriaRepository _categorias;
    private readonly PeriodoService _periodos;

    public DashboardService(RegistroRepository registros, CategoriaRepository categorias, PeriodoService periodos)
    {
        _registros = registros;
        _categorias = categorias;
        _periodos = periodos;
    }

    public async Task<DashboardViewModel> ConstruirAsync(
        string? vistaSlug,
        DateOnly? fecha,
        int? categoriaId,
        int pagina = 1,
        RegistroFormModel? captura = null)
    {
        var hoy = _periodos.Hoy();
        var vista = PeriodoService.ParseVista(vistaSlug);
        var referencia = fecha ?? hoy;
        var periodo = _periodos.Calcular(vista, referencia);

        var categorias = await _categorias.ListarAsync();
        if (categoriaId is not null && categorias.All(c => c.CategoriaId != categoriaId))
        {
            categoriaId = null;
        }

        var total = await _registros.TotalMinutosAsync(periodo.Desde, periodo.Hasta, categoriaId);
        var distribucion = await _registros.DistribucionAsync(periodo.Desde, periodo.Hasta, categoriaId);
        var (registros, totalRegistros, paginaActual, totalPaginas, usaPaginacion) =
            await ListarRegistrosAsync(vista, periodo, categoriaId, pagina);

        var desglose = new List<TotalPorDia>();
        var minutosFinDeSemana = 0;
        if (vista != VistaPeriodo.Dia)
        {
            var porDia = (await _registros.TotalesPorDiaAsync(periodo.Desde, periodo.Hasta, categoriaId))
                .ToDictionary(t => t.Fecha, t => t.Minutos);

            for (var d = periodo.Desde; d <= periodo.Hasta; d = d.AddDays(1))
            {
                var minutos = porDia.GetValueOrDefault(d);
                if (vista == VistaPeriodo.Semana && PeriodoService.EsFinDeSemana(d))
                {
                    minutosFinDeSemana += minutos;
                    continue;
                }
                desglose.Add(new TotalPorDia { Fecha = d, Minutos = minutos });
            }
        }

        captura ??= new RegistroFormModel { Fecha = hoy };
        captura.Vista = PeriodoService.VistaSlug(vista);
        captura.FechaVista = referencia;
        captura.FiltroCategoriaId = categoriaId;

        return new DashboardViewModel
        {
            Periodo = periodo,
            Hoy = hoy,
            FiltroCategoriaId = categoriaId,
            Categorias = categorias,
            TotalMinutos = total,
            Distribucion = distribucion,
            DesgloseDiario = desglose,
            MinutosFinDeSemana = minutosFinDeSemana,
            Registros = registros,
            TotalRegistros = totalRegistros,
            Pagina = paginaActual,
            TotalPaginas = totalPaginas,
            RegistrosPorPagina = RegistrosPorPagina,
            UsaPaginacion = usaPaginacion,
            Captura = captura,
            Anterior = _periodos.Anterior(vista, referencia),
            Siguiente = _periodos.Siguiente(vista, referencia),
        };
    }

    private async Task<(IReadOnlyList<Registro> Registros, int Total, int Pagina, int TotalPaginas, bool UsaPaginacion)>
        ListarRegistrosAsync(VistaPeriodo vista, Periodo periodo, int? categoriaId, int pagina)
    {
        if (vista == VistaPeriodo.Dia)
        {
            var todos = await _registros.ListarAsync(periodo.Desde, periodo.Hasta, categoriaId);
            return (todos, todos.Count, 1, 1, false);
        }

        var total = await _registros.ContarAsync(periodo.Desde, periodo.Hasta, categoriaId);
        if (total == 0)
        {
            return ([], 0, 1, 1, true);
        }

        var totalPaginas = (int)Math.Ceiling(total / (double)RegistrosPorPagina);
        var paginaActual = Math.Clamp(pagina, 1, totalPaginas);
        var offset = (paginaActual - 1) * RegistrosPorPagina;
        var paginaRegistros = await _registros.ListarPaginaAsync(
            periodo.Desde, periodo.Hasta, categoriaId, offset, RegistrosPorPagina);
        return (paginaRegistros, total, paginaActual, totalPaginas, true);
    }
}
