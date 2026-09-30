namespace Dashboard.Models;

public class Categoria
{
    public int CategoriaId { get; set; }
    public string Nombre { get; set; } = "";
    public string Color { get; set; } = "";
    public bool EsPredeterminada { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int TotalRegistros { get; set; }
}
