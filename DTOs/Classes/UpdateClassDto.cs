using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Classes;

public class UpdateClassDto
{
    [MaxLength(100)]
    public string? Tipo { get; set; }

    [MaxLength(50)]
    public string? Modalidad { get; set; }

    [Range(1, 480, ErrorMessage = "La duración debe estar entre 1 y 480 minutos.")]
    public int? Duracion { get; set; }

    [MaxLength(50)]
    public string? Nivel { get; set; }

    [Range(1, 1000, ErrorMessage = "Los cupos deben ser al menos 1.")]
    public int? Cupos { get; set; }

    [MaxLength(500)]
    public string? Descripcion { get; set; }

    public long? IdProfesor { get; set; }
}

public class AssignScheduleDto
{
    [Required(ErrorMessage = "El ID del horario es obligatorio.")]
    public long IdHorario { get; set; }

    [MaxLength(50)]
    public string? Aula { get; set; }
}
