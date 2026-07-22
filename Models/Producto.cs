using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("producto")]
public class Producto
{
    [Key]
    [Column("id_producto")]
    public long IdProducto { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Column("precio")]
    public decimal Precio { get; set; }

    [Column("tallas")]
    public string? Tallas { get; set; }

    [Column("estado")]
    public int Estado { get; set; }

    [Column("visibilidad")]
    public bool Visibilidad { get; set; }

    [Column("fecha_registro")]
    public DateTime FechaRegistro { get; set; }

    [Column("imagen")]
    public string? Imagen { get; set; }

    [Column("id_categoria")]
    public long IdCategoria { get; set; }

    // Propiedad de navegación
    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }
}
