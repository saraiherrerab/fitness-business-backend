using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("miembro")]
public class Miembro
{
    [Key]
    [Column("id_miembro")]
    public long IdMiembro { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [Column("correo")]
    public string Correo { get; set; } = string.Empty;

    [Column("fecha_ingreso")]
    public DateTime FechaIngreso { get; set; }

    [Column("plan")]
    public long? Plan { get; set; }

    [Column("estado")]
    public string? Estado { get; set; }

    // Propiedades de navegación
    public ICollection<RegistroPeso> RegistrosPesos { get; set; } = new List<RegistroPeso>();
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
}
