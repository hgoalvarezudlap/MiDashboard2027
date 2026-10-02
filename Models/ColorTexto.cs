namespace Dashboard.Models;

public static class ColorTexto
{
    /// <summary>Texto oscuro o blanco según la luminosidad del fondo, para que la insignia se lea.</summary>
    public static string Sobre(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex) || hex.Length != 7 || hex[0] != '#')
        {
            return "#1C1917";
        }

        var r = Convert.ToInt32(hex[1..3], 16);
        var g = Convert.ToInt32(hex[3..5], 16);
        var b = Convert.ToInt32(hex[5..7], 16);
        var luminosidad = 0.2126 * Lineal(r) + 0.7152 * Lineal(g) + 0.0722 * Lineal(b);
        return luminosidad > 0.45 ? "#1C1917" : "#FFFFFF";
    }

    private static double Lineal(int canal)
    {
        var s = canal / 255d;
        return s <= 0.03928 ? s / 12.92 : Math.Pow((s + 0.055) / 1.055, 2.4);
    }
}
