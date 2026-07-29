using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Payments;

public class CreatePaymentDto
{
    [Required(ErrorMessage = "El ID del miembro es obligatorio.")]
    public long IdMiembro { get; set; }

    [Required(ErrorMessage = "El mes facturado es obligatorio (ej. Mayo 2026).")]
    [MaxLength(50)]
    public string MesFacturado { get; set; } = string.Empty;

    [Range(0.01, 1000000.00, ErrorMessage = "El monto debe ser un valor válido mayor a 0.")]
    public decimal Monto { get; set; }

    [Required(ErrorMessage = "La fecha de vencimiento es obligatoria.")]
    public DateTime FechaVencimiento { get; set; }

    [MaxLength(30)]
    public string Estado { get; set; } = "Pendiente";

    public DateTime? FechaPago { get; set; }
}
