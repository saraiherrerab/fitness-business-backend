using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Notifications;

public class CreateNotificationDto
{
    public bool Tipo { get; set; } = true;

    [Required(ErrorMessage = "El mensaje de la notificación es obligatorio.")]
    [MaxLength(1000)]
    public string Mensaje { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? EnlaceReferencia { get; set; }
}
