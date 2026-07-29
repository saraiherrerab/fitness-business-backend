using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Contact;

public class ContactMessageDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo electrónico inválido.")]
    [MaxLength(150)]
    public string Correo { get; set; } = string.Empty;

    [MaxLength(30)]
    public string? Telefono { get; set; }

    [MaxLength(150)]
    public string? Asunto { get; set; }

    [Required(ErrorMessage = "El mensaje es obligatorio.")]
    [MaxLength(2000)]
    public string Mensaje { get; set; } = string.Empty;
}
