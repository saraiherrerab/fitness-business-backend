using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("registro_pesos")]
public class RegistroPeso
{
    [Key]
    [Column("id_registro_pesos")]
    public long IdRegistroPesos { get; set; }

    [Column("peso")]
    public decimal Peso { get; set; }

    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; }

    [Column("id_miembro")]
    public long IdMiembro { get; set; }

    // Propiedad de navegación
    [ForeignKey(nameof(IdMiembro))]
    public Miembro? Miembro { get; set; }
}
