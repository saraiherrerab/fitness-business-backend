using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    public string Contraseña { get; set; } = string.Empty;
}
