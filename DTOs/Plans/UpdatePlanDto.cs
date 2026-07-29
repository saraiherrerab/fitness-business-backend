using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Plans;

public class UpdatePlanDto
{
    [MaxLength(100)]
    public string? Nombre { get; set; }

    [Range(0.0, 100000.0, ErrorMessage = "El precio debe ser un valor válido.")]
    public double? Precio { get; set; }

    public bool? Estado { get; set; }

    public bool? Destacado { get; set; }

    [MaxLength(255)]
    public string? MensajeWhatsapp { get; set; }

    public List<string>? ItemsIncluidos { get; set; }
}
