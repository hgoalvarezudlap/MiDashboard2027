using System.Globalization;

namespace Dashboard.Models;

public static class TiempoFormato
{
    public static readonly CultureInfo Cultura = new("es-MX");

    public static string Horas(int minutos)
    {
        var h = minutos / 60;
        var m = minutos % 60;
        if (h == 0) return $"{m} min";
        if (m == 0) return $"{h} h";
        return $"{h} h {m} min";
    }

    public static string HorasDecimal(int minutos) =>
        (minutos / 60m).ToString("0.##", Cultura);

    public static IEnumerable<(int Minutos, string Texto)> OpcionesTiempo()
    {
        for (var m = 15; m <= 480; m += 15)
        {
            yield return (m, Horas(m));
        }
    }

    public static string DiaCorto(DateOnly fecha) =>
        Cultura.TextInfo.ToTitleCase(fecha.ToString("ddd d", Cultura).Replace(".", ""));

    public static string FechaLarga(DateOnly fecha) =>
        PrimeraMayuscula(fecha.ToString("dddd d 'de' MMMM 'de' yyyy", Cultura));

    public static string PrimeraMayuscula(string texto) =>
        string.IsNullOrEmpty(texto) ? texto : char.ToUpper(texto[0], Cultura) + texto[1..];

    public static string FechaCorta(DateOnly fecha) =>
        fecha.ToString("dd/MM/yyyy", Cultura);
}
