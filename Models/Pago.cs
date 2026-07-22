using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitwomanAPI.Models;

[Table("pagos")]
public class Pago
{
    [Key]
    [Column("id_pagos")]
    public long IdPagos { get; set; }

    [Required]
    [Column("mes_facturado")]
    public string MesFacturado { get; set; } = string.Empty;

    [Column("monto")]
    public decimal Monto { get; set; }

    [Column("fecha_vencimiento")]
    public DateTime FechaVencimiento { get; set; }

    [Column("fecha_pago")]
    public DateTime? FechaPago { get; set; }

    [Required]
    [Column("estado")]
    public string Estado { get; set; } = string.Empty;

    [Column("id_miembro")]
    public long IdMiembro { get; set; }

    // Propiedad de navegación
    [ForeignKey(nameof(IdMiembro))]
    public Miembro? Miembro { get; set; }
}
