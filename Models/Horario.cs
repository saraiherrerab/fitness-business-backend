using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("horarios")]
public class Horario
{
    [Key]
    [Column("id_horarios")]
    public long IdHorarios { get; set; }

    [Required]
    [Column("dia_semana")]
    public string DiaSemana { get; set; } = string.Empty;

    [Column("hora_inicio")]
    public TimeSpan HoraInicio { get; set; }

    [Column("hora_fin")]
    public TimeSpan HoraFin { get; set; }

    // Relación muchos a muchos mediante tabla intermedia
    public ICollection<ClaseHorario> ClasesHorarios { get; set; } = new List<ClaseHorario>();
}
