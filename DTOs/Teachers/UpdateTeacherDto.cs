using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Teachers;

public class UpdateTeacherDto
{
    [MaxLength(100)]
    public string? Nombre { get; set; }

    [MaxLength(100)]
    public string? Apellido { get; set; }

    public DateTime? FechaDeNacimiento { get; set; }
}
