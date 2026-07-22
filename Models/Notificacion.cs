using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("notificaciones")]
public class Notificacion
{
    [Key]
    [Column("id_notificaciones")]
    public long IdNotificaciones { get; set; }

    [Column("tipo")]
    public bool Tipo { get; set; }

    [Required]
    [Column("mensaje")]
    public string Mensaje { get; set; } = string.Empty;

    [Column("leida")]
    public bool Leida { get; set; }

    [Column("fecha_creacion")]
    public DateTime FechaCreacion { get; set; }

    [Column("enlace_referencia")]
    public string? EnlaceReferencia { get; set; }
}
