using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Common;
using FitwomanAPI.DTOs.Notifications;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/notifications")]
[Authorize]
public class NotificationsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public NotificationsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista paginada de notificaciones con filtros por estado de lectura y tipo
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<NotificationResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNotifications(
        [FromQuery] bool? leida,
        [FromQuery] bool? tipo,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 15)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 15;

        var query = _context.Notificaciones.AsQueryable();

        if (leida.HasValue)
        {
            query = query.Where(n => n.Leida == leida.Value);
        }

        if (tipo.HasValue)
        {
            query = query.Where(n => n.Tipo == tipo.Value);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(n => n.FechaCreacion)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(n => new NotificationResponseDto
            {
                Id = n.IdNotificaciones,
                Tipo = n.Tipo,
                Mensaje = n.Mensaje,
                Leida = n.Leida,
                FechaCreacion = n.FechaCreacion,
                EnlaceReferencia = n.EnlaceReferencia
            })
            .ToListAsync();

        var result = new PagedResultDto<NotificationResponseDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el conteo de notificaciones no leídas (para el ícono de la campana en el Header Admin)
    /// </summary>
    [HttpGet("unread-count")]
    [ProducesResponseType(typeof(NotificationUnreadCountDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUnreadCount()
    {
        var count = await _context.Notificaciones
            .Where(n => !n.Leida)
            .CountAsync();

        return Ok(new NotificationUnreadCountDto { UnreadCount = count });
    }

    /// <summary>
    /// Crea una nueva notificación en el sistema (Admin / Interno)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(NotificationResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateNotification([FromBody] CreateNotificationDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var notification = new Notificacion
        {
            Tipo = dto.Tipo,
            Mensaje = dto.Mensaje.Trim(),
            Leida = false,
            FechaCreacion = DateTime.UtcNow,
            EnlaceReferencia = dto.EnlaceReferencia?.Trim()
        };

        _context.Notificaciones.Add(notification);
        await _context.SaveChangesAsync();

        var response = new NotificationResponseDto
        {
            Id = notification.IdNotificaciones,
            Tipo = notification.Tipo,
            Mensaje = notification.Mensaje,
            Leida = notification.Leida,
            FechaCreacion = notification.FechaCreacion,
            EnlaceReferencia = notification.EnlaceReferencia
        };

        return CreatedAtAction(nameof(GetNotifications), new { id = notification.IdNotificaciones }, response);
    }

    /// <summary>
    /// Marca una notificación específica como leída
    /// </summary>
    [HttpPatch("{id:long}/read")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsRead(long id)
    {
        var notification = await _context.Notificaciones.FindAsync(id);
        if (notification == null)
        {
            return NotFound(new { message = $"No se encontró la notificación con ID {id}." });
        }

        notification.Leida = true;
        await _context.SaveChangesAsync();

        return Ok(new { message = "Notificación marcada como leída." });
    }

    /// <summary>
    /// Marca todas las notificaciones pendientes como leídas
    /// </summary>
    [HttpPatch("read-all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> MarkAllAsRead()
    {
        var unread = await _context.Notificaciones
            .Where(n => !n.Leida)
            .ToListAsync();

        foreach (var item in unread)
        {
            item.Leida = true;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = $"Se marcaron {unread.Count} notificaciones como leídas." });
    }

    /// <summary>
    /// Elimina una notificación del sistema
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteNotification(long id)
    {
        var notification = await _context.Notificaciones.FindAsync(id);
        if (notification == null)
        {
            return NotFound(new { message = $"No se encontró la notificación con ID {id}." });
        }

        _context.Notificaciones.Remove(notification);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Notificación eliminada exitosamente." });
    }
}
