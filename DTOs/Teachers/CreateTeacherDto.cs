using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Teachers;

public class CreateTeacherDto
{
    [Required(ErrorMessage = "El nombre del profesor es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido del profesor es obligatorio.")]
    [MaxLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public DateTime FechaDeNacimiento { get; set; }
}
