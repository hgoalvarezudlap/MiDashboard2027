using System.ComponentModel.DataAnnotations;

namespace Dashboard.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "Escribe el usuario.")]
    [Display(Name = "Usuario")]
    public string Username { get; set; } = "";

    [Required(ErrorMessage = "Escribe la contraseña.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña")]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}
