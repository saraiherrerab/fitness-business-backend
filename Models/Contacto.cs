using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("contacto")]
public class Contacto
{
    [Key]
    [Column("id_contacto")]
    public long IdContacto { get; set; }

    [Column("teléfono")]
    public string? Telefono { get; set; }

    [Column("correo")]
    public string? Correo { get; set; }

    [Column("dirección")]
    public string? Direccion { get; set; }

    [Column("ciudad")]
    public string? Ciudad { get; set; }

    [Column("pais")]
    public string? Pais { get; set; }

    [Column("url_google_maps")]
    public string? UrlGoogleMaps { get; set; }
}
