using Dashboard.Models;

namespace Dashboard.Services;

public class PeriodoService
{
    private static readonly TimeZoneInfo ZonaMexico = ObtenerZona();

    private static TimeZoneInfo ObtenerZona()
    {
        try
        {
            return TimeZoneInfo.FindSystemTimeZoneById("America/Mexico_City");
        }
        catch (TimeZoneNotFoundException)
        {
            return TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)");
        }
    }

    public DateOnly Hoy()
    {
        var ahora = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, ZonaMexico);
        return DateOnly.FromDateTime(ahora);
    }

    public static VistaPeriodo ParseVista(string? vista) => vista?.ToLowerInvariant() switch
    {
        "semana" => VistaPeriodo.Semana,
        "mes" => VistaPeriodo.Mes,
        _ => VistaPeriodo.Dia,
    };

    public static string VistaSlug(VistaPeriodo vista) => vista switch
    {
        VistaPeriodo.Semana => "semana",
        VistaPeriodo.Mes => "mes",
        _ => "dia",
    };

    public Periodo Calcular(VistaPeriodo vista, DateOnly referencia)
    {
        switch (vista)
        {
            case VistaPeriodo.Semana:
            {
                var desde = InicioSemana(referencia);
                var hasta = desde.AddDays(6);
                return new Periodo(vista, desde, hasta, referencia, EtiquetaSemana(desde, hasta));
            }
            case VistaPeriodo.Mes:
            {
                var desde = new DateOnly(referencia.Year, referencia.Month, 1);
                var hasta = desde.AddMonths(1).AddDays(-1);
                var etiqueta = TiempoFormato.Cultura.TextInfo.ToTitleCase(
                    desde.ToString("MMMM yyyy", TiempoFormato.Cultura));
                return new Periodo(vista, desde, hasta, referencia, etiqueta);
            }
            default:
                return new Periodo(vista, referencia, referencia, referencia, TiempoFormato.FechaLarga(referencia));
        }
    }

    public DateOnly Anterior(VistaPeriodo vista, DateOnly referencia) => vista switch
    {
        VistaPeriodo.Semana => InicioSemana(referencia).AddDays(-7),
        VistaPeriodo.Mes => new DateOnly(referencia.Year, referencia.Month, 1).AddMonths(-1),
        _ => referencia.AddDays(-1),
    };

    public DateOnly Siguiente(VistaPeriodo vista, DateOnly referencia) => vista switch
    {
        VistaPeriodo.Semana => InicioSemana(referencia).AddDays(7),
        VistaPeriodo.Mes => new DateOnly(referencia.Year, referencia.Month, 1).AddMonths(1),
        _ => referencia.AddDays(1),
    };

    public static DateOnly InicioSemana(DateOnly fecha)
    {
        // Lunes = 0 ... Domingo = 6, sin depender de DATEFIRST ni de la cultura.
        var offset = ((int)fecha.DayOfWeek + 6) % 7;
        return fecha.AddDays(-offset);
    }

    public static bool EsFinDeSemana(DateOnly fecha) =>
        fecha.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday;

    private static string EtiquetaSemana(DateOnly desde, DateOnly hasta)
    {
        var c = TiempoFormato.Cultura;
        string d = desde.Month == hasta.Month
            ? $"{desde.Day} al {hasta.Day} de {hasta.ToString("MMMM", c)}"
            : $"{desde.Day} de {desde.ToString("MMM", c).TrimEnd('.')} al {hasta.Day} de {hasta.ToString("MMM", c).TrimEnd('.')}";
        var anio = desde.Year == hasta.Year ? $" {hasta.Year}" : $" {desde.Year}/{hasta.Year}";
        return $"Semana del {d}{anio}";
    }
}
