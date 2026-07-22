using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("clases")]
public class Clase
{
    [Key]
    [Column("id_clases")]
    public long IdClases { get; set; }

    [Required]
    [Column("tipo")]
    public string Tipo { get; set; } = string.Empty;

    [Required]
    [Column("modalidad")]
    public string Modalidad { get; set; } = string.Empty;

    [Column("duración")]
    public int Duracion { get; set; }

    [Required]
    [Column("nivel")]
    public string Nivel { get; set; } = string.Empty;

    [Column("cupos")]
    public int Cupos { get; set; }

    [Column("descripción")]
    public string? Descripcion { get; set; }

    [Column("id_profesor")]
    public long IdProfesor { get; set; }

    // Propiedad de navegación hacia Profesor
    [ForeignKey(nameof(IdProfesor))]
    public Profesor? Profesor { get; set; }

    // Relación muchos a muchos mediante tabla intermedia
    public ICollection<ClaseHorario> ClasesHorarios { get; set; } = new List<ClaseHorario>();
}
