using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Plans;

public class CreatePlanDto
{
    [Required(ErrorMessage = "El nombre del plan es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0.0, 100000.0, ErrorMessage = "El precio debe ser un valor válido.")]
    public double Precio { get; set; }

    public bool Estado { get; set; } = true;

    public bool Destacado { get; set; } = false;

    [MaxLength(255)]
    public string? MensajeWhatsapp { get; set; }

    public List<string>? ItemsIncluidos { get; set; }
}
