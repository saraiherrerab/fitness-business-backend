using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("categoria")]
public class Categoria
{
    [Key]
    [Column("id_categoria")]
    public long IdCategoria { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    // Propiedad de navegación (Un Categoría tiene muchos Productos)
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
