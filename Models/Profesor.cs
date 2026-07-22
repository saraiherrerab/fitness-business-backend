using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("profesores")]
public class Profesor
{
    [Key]
    [Column("id_profesores")]
    public long IdProfesores { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Column("fecha_de_nacimiento")]
    public DateTime FechaDeNacimiento { get; set; }

    // Propiedad de navegación (Un Profesor impone varias Clases)
    public ICollection<Clase> Clases { get; set; } = new List<Clase>();
}
