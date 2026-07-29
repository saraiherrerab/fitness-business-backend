using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Payments;

public class UpdatePaymentDto
{
    [MaxLength(50)]
    public string? MesFacturado { get; set; }

    [Range(0.01, 1000000.00, ErrorMessage = "El monto debe ser un valor válido mayor a 0.")]
    public decimal? Monto { get; set; }

    public DateTime? FechaVencimiento { get; set; }

    public DateTime? FechaPago { get; set; }

    [MaxLength(30)]
    public string? Estado { get; set; }
}
