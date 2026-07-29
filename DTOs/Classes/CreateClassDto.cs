using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Classes;

public class CreateClassDto
{
    [Required(ErrorMessage = "El tipo de clase es obligatorio (ej. Pole Dance, TRX, Pilates).")]
    [MaxLength(100)]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La modalidad es obligatoria (ej. Presencial, Virtual).")]
    [MaxLength(50)]
    public string Modalidad { get; set; } = string.Empty;

    [Range(1, 480, ErrorMessage = "La duración debe estar entre 1 y 480 minutos.")]
    public int Duracion { get; set; }

    [Required(ErrorMessage = "El nivel es obligatorio (ej. Principiante, Intermedio, Avanzado).")]
    [MaxLength(50)]
    public string Nivel { get; set; } = string.Empty;

    [Range(1, 1000, ErrorMessage = "Los cupos deben ser al menos 1.")]
    public int Cupos { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    [Required(ErrorMessage = "El profesor asignado es obligatorio.")]
    public long IdProfesor { get; set; }

    public List<long>? HorarioIds { get; set; }
}
