using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("clases_horarios")]
public class ClaseHorario
{
    [Column("id_clases")]
    public long IdClases { get; set; }

    [Column("id_horarios")]
    public long IdHorarios { get; set; }

    [Column("aula")]
    public string? Aula { get; set; }

    // Propiedades de navegación
    [ForeignKey(nameof(IdClases))]
    public Clase? Clase { get; set; }

    [ForeignKey(nameof(IdHorarios))]
    public Horario? Horario { get; set; }
}
