using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Contact;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/contact")]
public class ContactController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ContactController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la información oficial de contacto del gimnasio (teléfono, dirección, maps, etc.)
    /// </summary>
    [HttpGet("info")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ContactInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetContactInfo()
    {
        var info = await _context.Contactos.FirstOrDefaultAsync();

        if (info == null)
        {
            // Retorna estructura por defecto si no ha sido configurada aún
            return Ok(new ContactInfoDto
            {
                Id = 0,
                Telefono = "+56 9 1234 5678",
                Correo = "contacto@fitwoman.cl",
                Direccion = "Av. Principal 123",
                Ciudad = "Santiago",
                Pais = "Chile",
                UrlGoogleMaps = "https://maps.google.com"
            });
        }

        var dto = new ContactInfoDto
        {
            Id = info.IdContacto,
            Telefono = info.Telefono,
            Correo = info.Correo,
            Direccion = info.Direccion,
            Ciudad = info.Ciudad,
            Pais = info.Pais,
            UrlGoogleMaps = info.UrlGoogleMaps
        };

        return Ok(dto);
    }

    /// <summary>
    /// Actualiza la información oficial de contacto del gimnasio (Admin)
    /// </summary>
    [HttpPut("info")]
    [Authorize]
    [ProducesResponseType(typeof(ContactInfoDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateContactInfo([FromBody] UpdateContactInfoDto dto)
    {
        var info = await _context.Contactos.FirstOrDefaultAsync();

        if (info == null)
        {
            info = new Contacto();
            _context.Contactos.Add(info);
        }

        if (dto.Telefono != null) info.Telefono = dto.Telefono.Trim();
        if (dto.Correo != null) info.Correo = dto.Correo.Trim();
        if (dto.Direccion != null) info.Direccion = dto.Direccion.Trim();
        if (dto.Ciudad != null) info.Ciudad = dto.Ciudad.Trim();
        if (dto.Pais != null) info.Pais = dto.Pais.Trim();
        if (dto.UrlGoogleMaps != null) info.UrlGoogleMaps = dto.UrlGoogleMaps.Trim();

        await _context.SaveChangesAsync();

        var result = new ContactInfoDto
        {
            Id = info.IdContacto,
            Telefono = info.Telefono,
            Correo = info.Correo,
            Direccion = info.Direccion,
            Ciudad = info.Ciudad,
            Pais = info.Pais,
            UrlGoogleMaps = info.UrlGoogleMaps
        };

        return Ok(result);
    }

    /// <summary>
    /// Recibe un mensaje enviado desde el formulario de contacto público de la página web
    /// </summary>
    [HttpPost("send")]
    [AllowAnonymous]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SendContactMessage([FromBody] ContactMessageDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Crear una notificación del sistema para alertar a los administradores
        var notificacion = new Notificacion
        {
            Tipo = true,
            Mensaje = $"Nuevo mensaje de {dto.Nombre} ({dto.Correo}): \"{dto.Mensaje}\"",
            Leida = false,
            FechaCreacion = DateTime.UtcNow,
            EnlaceReferencia = "/admin/contact"
        };

        _context.Notificaciones.Add(notificacion);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            message = "Tu mensaje ha sido recibido con éxito. Nos pondremos en contacto contigo a la brevedad."
        });
    }
}
