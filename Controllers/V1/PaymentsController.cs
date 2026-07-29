using System.Security.Claims;
using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Common;
using FitwomanAPI.DTOs.Payments;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/payments")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista paginada de cobros y pagos registrada en el sistema con filtros avanzados (Admin)
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<PaymentResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPayments(
        [FromQuery] string? search,
        [FromQuery] string? estado,
        [FromQuery] long? idMiembro,
        [FromQuery] string? mesFacturado,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Pagos
            .Include(p => p.Miembro)
            .AsQueryable();

        // Filtro por búsqueda (nombre/apellido o correo del miembro)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p =>
                p.Miembro != null && (
                    p.Miembro.Nombre.ToLower().Contains(searchLower) ||
                    p.Miembro.Apellido.ToLower().Contains(searchLower) ||
                    p.Miembro.Correo.ToLower().Contains(searchLower)
                ));
        }

        // Filtro por estado del pago (Pagado, Pendiente, Vencido, Cancelado)
        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoLower = estado.Trim().ToLower();
            query = query.Where(p => p.Estado.ToLower() == estadoLower);
        }

        // Filtro por miembro especifico
        if (idMiembro.HasValue)
        {
            query = query.Where(p => p.IdMiembro == idMiembro.Value);
        }

        // Filtro por mes facturado
        if (!string.IsNullOrWhiteSpace(mesFacturado))
        {
            var mesLower = mesFacturado.Trim().ToLower();
            query = query.Where(p => p.MesFacturado.ToLower().Contains(mesLower));
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.FechaVencimiento)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new PaymentResponseDto
            {
                Id = p.IdPagos,
                MesFacturado = p.MesFacturado,
                Monto = p.Monto,
                FechaVencimiento = p.FechaVencimiento,
                FechaPago = p.FechaPago,
                Estado = p.Estado,
                IdMiembro = p.IdMiembro,
                NombreMiembro = p.Miembro != null ? $"{p.Miembro.Nombre} {p.Miembro.Apellido}".Trim() : null,
                CorreoMiembro = p.Miembro != null ? p.Miembro.Correo : null
            })
            .ToListAsync();

        var result = new PagedResultDto<PaymentResponseDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Histórico de pagos pertenecientes al miembro autenticado (Portal Cliente)
    /// </summary>
    [HttpGet("my-payments")]
    [ProducesResponseType(typeof(IEnumerable<PaymentResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyPayments()
    {
        var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(emailClaim))
        {
            return Unauthorized(new { message = "No se pudo determinar la identidad del usuario." });
        }

        var miembro = await _context.Miembros.FirstOrDefaultAsync(m => m.Correo.ToLower() == emailClaim.ToLower());
        if (miembro == null)
        {
            return Ok(new List<PaymentResponseDto>());
        }

        var payments = await _context.Pagos
            .Where(p => p.IdMiembro == miembro.IdMiembro)
            .OrderByDescending(p => p.FechaVencimiento)
            .Select(p => new PaymentResponseDto
            {
                Id = p.IdPagos,
                MesFacturado = p.MesFacturado,
                Monto = p.Monto,
                FechaVencimiento = p.FechaVencimiento,
                FechaPago = p.FechaPago,
                Estado = p.Estado,
                IdMiembro = p.IdMiembro,
                NombreMiembro = $"{miembro.Nombre} {miembro.Apellido}".Trim(),
                CorreoMiembro = miembro.Correo
            })
            .ToListAsync();

        return Ok(payments);
    }

    /// <summary>
    /// Resumen financiero consolidado (Total recaudado, total pendiente y conteo de estados)
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(PaymentSummaryDto), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPaymentSummary()
    {
        var pagos = await _context.Pagos.ToListAsync();

        var totalRecaudado = pagos
            .Where(p => p.Estado.Equals("Pagado", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Monto);

        var totalPendiente = pagos
            .Where(p => p.Estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase) || p.Estado.Equals("Vencido", StringComparison.OrdinalIgnoreCase))
            .Sum(p => p.Monto);

        var pagadosCount = pagos.Count(p => p.Estado.Equals("Pagado", StringComparison.OrdinalIgnoreCase));
        var pendientesCount = pagos.Count(p => p.Estado.Equals("Pendiente", StringComparison.OrdinalIgnoreCase));
        var vencidosCount = pagos.Count(p => p.Estado.Equals("Vencido", StringComparison.OrdinalIgnoreCase));

        return Ok(new PaymentSummaryDto
        {
            TotalRecaudado = totalRecaudado,
            TotalPendiente = totalPendiente,
            CantidadPagados = pagadosCount,
            CantidadPendientes = pendientesCount,
            CantidadVencidos = vencidosCount
        });
    }

    /// <summary>
    /// Obtiene el detalle de un pago por su ID
    /// </summary>
    [HttpGet("{id:long}")]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPaymentById(long id)
    {
        var payment = await _context.Pagos
            .Include(p => p.Miembro)
            .FirstOrDefaultAsync(p => p.IdPagos == id);

        if (payment == null)
        {
            return NotFound(new { message = $"No se encontró el registro de pago con ID {id}." });
        }

        var dto = new PaymentResponseDto
        {
            Id = payment.IdPagos,
            MesFacturado = payment.MesFacturado,
            Monto = payment.Monto,
            FechaVencimiento = payment.FechaVencimiento,
            FechaPago = payment.FechaPago,
            Estado = payment.Estado,
            IdMiembro = payment.IdMiembro,
            NombreMiembro = payment.Miembro != null ? $"{payment.Miembro.Nombre} {payment.Miembro.Apellido}".Trim() : null,
            CorreoMiembro = payment.Miembro?.Correo
        };

        return Ok(dto);
    }

    /// <summary>
    /// Emite un nuevo recibo/cobro a un miembro (Admin)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var miembro = await _context.Miembros.FindAsync(dto.IdMiembro);
        if (miembro == null)
        {
            return BadRequest(new { message = $"El miembro con ID {dto.IdMiembro} no existe." });
        }

        var payment = new Pago
        {
            IdMiembro = dto.IdMiembro,
            MesFacturado = dto.MesFacturado.Trim(),
            Monto = dto.Monto,
            FechaVencimiento = DateTime.SpecifyKind(dto.FechaVencimiento, DateTimeKind.Utc),
            FechaPago = dto.FechaPago.HasValue ? DateTime.SpecifyKind(dto.FechaPago.Value, DateTimeKind.Utc) : null,
            Estado = dto.Estado.Trim()
        };

        _context.Pagos.Add(payment);
        await _context.SaveChangesAsync();

        var response = new PaymentResponseDto
        {
            Id = payment.IdPagos,
            MesFacturado = payment.MesFacturado,
            Monto = payment.Monto,
            FechaVencimiento = payment.FechaVencimiento,
            FechaPago = payment.FechaPago,
            Estado = payment.Estado,
            IdMiembro = payment.IdMiembro,
            NombreMiembro = $"{miembro.Nombre} {miembro.Apellido}".Trim(),
            CorreoMiembro = miembro.Correo
        };

        return CreatedAtAction(nameof(GetPaymentById), new { id = payment.IdPagos }, response);
    }

    /// <summary>
    /// Actualiza los datos de un recibo o cuota de pago (Admin)
    /// </summary>
    [HttpPut("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePayment(long id, [FromBody] UpdatePaymentDto dto)
    {
        var payment = await _context.Pagos.FindAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = $"No se encontró el pago con ID {id}." });
        }

        if (!string.IsNullOrWhiteSpace(dto.MesFacturado)) payment.MesFacturado = dto.MesFacturado.Trim();
        if (dto.Monto.HasValue) payment.Monto = dto.Monto.Value;
        if (dto.FechaVencimiento.HasValue) payment.FechaVencimiento = DateTime.SpecifyKind(dto.FechaVencimiento.Value, DateTimeKind.Utc);
        if (dto.FechaPago.HasValue) payment.FechaPago = DateTime.SpecifyKind(dto.FechaPago.Value, DateTimeKind.Utc);
        if (!string.IsNullOrWhiteSpace(dto.Estado)) payment.Estado = dto.Estado.Trim();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Registro de pago actualizado exitosamente." });
    }

    /// <summary>
    /// Marca un pago como abonado/pagado registrando la fecha actual (Admin)
    /// </summary>
    [HttpPatch("{id:long}/pay")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkAsPaid(long id)
    {
        var payment = await _context.Pagos.FindAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = $"No se encontró el pago con ID {id}." });
        }

        payment.Estado = "Pagado";
        payment.FechaPago = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return Ok(new { message = "El pago ha sido registrado como Pagado exitosamente.", fechaPago = payment.FechaPago });
    }

    /// <summary>
    /// Elimina un registro de pago (Admin)
    /// </summary>
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePayment(long id)
    {
        var payment = await _context.Pagos.FindAsync(id);
        if (payment == null)
        {
            return NotFound(new { message = $"No se encontró el pago con ID {id}." });
        }

        _context.Pagos.Remove(payment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Registro de pago eliminado exitosamente." });
    }
}
