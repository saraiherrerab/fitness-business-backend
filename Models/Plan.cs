using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("planes")]
public class Plan
{
    [Key]
    [Column("id_planes")]
    public int IdPlanes { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("precio")]
    public double Precio { get; set; }

    [Column("estado")]
    public bool Estado { get; set; }

    [Column("destacado")]
    public bool Destacado { get; set; }

    [Column("mensaje_whatsapp")]
    public string? MensajeWhatsapp { get; set; }

    [Column("items_incluidos", TypeName = "jsonb")]
    public string? ItemsIncluidos { get; set; }
}
