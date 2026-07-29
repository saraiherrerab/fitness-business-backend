using System.Text.Json;
using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Plans;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/plans")]
public class PlansController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PlansController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtiene la lista completa de planes de membresía (Admin)
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(IEnumerable<PlanResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlans([FromQuery] bool? status)
    {
        var query = _context.Planes.AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(p => p.Estado == status.Value);
        }

        var plans = await query
            .OrderByDescending(p => p.Destacado)
            .ThenBy(p => p.Precio)
            .ToListAsync();

        var result = plans.Select(MapToDto).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Endpoint público de planes de membresía activos para el Front Cliente / Landing
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<PlanResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicPlans()
    {
        var plans = await _context.Planes
            .Where(p => p.Estado)
            .OrderByDescending(p => p.Destacado)
            .ThenBy(p => p.Precio)
            .ToListAsync();

        var result = plans.Select(MapToDto).ToList();
        return Ok(result);
    }

    /// <summary>
    /// Obtiene un plan de membresía por su ID
    /// </summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(PlanResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlanById(int id)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan == null)
        {
            return NotFound(new { message = $"No se encontró el plan con ID {id}." });
        }

        return Ok(MapToDto(plan));
    }

    /// <summary>
    /// Crea un nuevo plan de membresía (Admin)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(PlanResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePlan([FromBody] CreatePlanDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var jsonItems = dto.ItemsIncluidos != null && dto.ItemsIncluidos.Any()
            ? JsonSerializer.Serialize(dto.ItemsIncluidos)
            : null;

        var plan = new Plan
        {
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio,
            Estado = dto.Estado,
            Destacado = dto.Destacado,
            MensajeWhatsapp = dto.MensajeWhatsapp?.Trim(),
            ItemsIncluidos = jsonItems
        };

        _context.Planes.Add(plan);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlanById), new { id = plan.IdPlanes }, MapToDto(plan));
    }

    /// <summary>
    /// Actualiza los datos de un plan existente (Admin)
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePlan(int id, [FromBody] UpdatePlanDto dto)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan == null)
        {
            return NotFound(new { message = $"No se encontró el plan con ID {id}." });
        }

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) plan.Nombre = dto.Nombre.Trim();
        if (dto.Precio.HasValue) plan.Precio = dto.Precio.Value;
        if (dto.Estado.HasValue) plan.Estado = dto.Estado.Value;
        if (dto.Destacado.HasValue) plan.Destacado = dto.Destacado.Value;
        if (dto.MensajeWhatsapp != null) plan.MensajeWhatsapp = dto.MensajeWhatsapp.Trim();

        if (dto.ItemsIncluidos != null)
        {
            plan.ItemsIncluidos = dto.ItemsIncluidos.Any()
                ? JsonSerializer.Serialize(dto.ItemsIncluidos)
                : null;
        }

        await _context.SaveChangesAsync();

        return Ok(new { message = "Plan actualizado exitosamente.", plan = MapToDto(plan) });
    }

    /// <summary>
    /// Alterna el estado activo/inactivo de un plan (Admin)
    /// </summary>
    [HttpPatch("{id:int}/status")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleStatus(int id, [FromQuery] bool active)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan == null)
        {
            return NotFound(new { message = $"No se encontró el plan con ID {id}." });
        }

        plan.Estado = active;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Estado del plan actualizado a {(active ? "Activo" : "Inactivo")}." });
    }

    /// <summary>
    /// Alterna si el plan es destacado en la landing web (Admin)
    /// </summary>
    [HttpPatch("{id:int}/featured")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleFeatured(int id, [FromQuery] bool featured)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan == null)
        {
            return NotFound(new { message = $"No se encontró el plan con ID {id}." });
        }

        plan.Destacado = featured;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"El plan ahora {(featured ? "es destacado" : "ya no es destacado")}." });
    }

    /// <summary>
    /// Elimina un plan de membresía (Admin). Falla si hay miembros asociados.
    /// </summary>
    [HttpDelete("{id:int}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePlan(int id)
    {
        var plan = await _context.Planes.FindAsync(id);
        if (plan == null)
        {
            return NotFound(new { message = $"No se encontró el plan con ID {id}." });
        }

        // Verificar si hay miembros usando este plan
        var hasMembers = await _context.Miembros.AnyAsync(m => m.Plan == (long)id);
        if (hasMembers)
        {
            return BadRequest(new { message = "No se puede eliminar el plan porque hay miembros suscritos actualmente a él." });
        }

        _context.Planes.Remove(plan);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Plan eliminado exitosamente." });
    }

    // Helper privado de mapeo
    private static PlanResponseDto MapToDto(Plan plan)
    {
        List<string> items = new();
        if (!string.IsNullOrWhiteSpace(plan.ItemsIncluidos))
        {
            try
            {
                items = JsonSerializer.Deserialize<List<string>>(plan.ItemsIncluidos) ?? new();
            }
            catch
            {
                items = new List<string> { plan.ItemsIncluidos };
            }
        }

        return new PlanResponseDto
        {
            Id = plan.IdPlanes,
            Nombre = plan.Nombre,
            Precio = plan.Precio,
            Estado = plan.Estado,
            Destacado = plan.Destacado,
            MensajeWhatsapp = plan.MensajeWhatsapp,
            ItemsIncluidos = items
        };
    }
}
