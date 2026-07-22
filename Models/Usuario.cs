using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("usuarios")]
public class Usuario
{
    [Key]
    [Column("id_usuarios")]
    public long IdUsuarios { get; set; }

    [Required]
    [Column("nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [Column("apellido")]
    public string Apellido { get; set; } = string.Empty;

    [Required]
    [Column("correo")]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [Column("contraseña")]
    public string Contraseña { get; set; } = string.Empty;
}
