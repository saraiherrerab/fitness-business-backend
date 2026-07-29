namespace FitwomanAPI.DTOs.Payments;

public class PaymentResponseDto
{
    public long Id { get; set; }
    public string MesFacturado { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string Estado { get; set; } = string.Empty;
    public long IdMiembro { get; set; }
    public string? NombreMiembro { get; set; }
    public string? CorreoMiembro { get; set; }
}

public class PaymentSummaryDto
{
    public decimal TotalRecaudado { get; set; }
    public decimal TotalPendiente { get; set; }
    public int CantidadPagados { get; set; }
    public int CantidadPendientes { get; set; }
    public int CantidadVencidos { get; set; }
}
