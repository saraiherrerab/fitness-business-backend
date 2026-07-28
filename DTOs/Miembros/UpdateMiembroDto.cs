using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Miembros;

public class UpdateMiembroDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo es inválido.")]
    public string Correo { get; set; } = string.Empty;

    public long? Plan { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    public string Estado { get; set; } = "Activo";
}
