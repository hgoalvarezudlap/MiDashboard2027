using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models;

public class CategoriaFormModel
{
    public int? CategoriaId { get; set; }

    [Required(ErrorMessage = "Escribe el nombre de la categoría.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = "";

    [Required(ErrorMessage = "Elige un color.")]
    [RegularExpression("^#[0-9A-Fa-f]{6}$", ErrorMessage = "El color debe tener el formato #RRGGBB.")]
    [Display(Name = "Color")]
    public string Color { get; set; } = "#F47A20";

    public bool EsPredeterminada { get; set; }
}
