using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Common;
using FitwomanAPI.DTOs.Members;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/members")]
[Authorize]
public class MembersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MembersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Paginated list of members with search and status filters
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<MemberResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMembers(
        [FromQuery] string? search,
        [FromQuery] string? status,
        [FromQuery] long? planId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Miembros.AsQueryable();

        // Search filter (first name, last name or email)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(m =>
                m.Nombre.ToLower().Contains(searchLower) ||
                m.Apellido.ToLower().Contains(searchLower) ||
                m.Correo.ToLower().Contains(searchLower));
        }

        // Status filter
        if (!string.IsNullOrWhiteSpace(status))
        {
            var statusLower = status.Trim().ToLower();
            query = query.Where(m => m.Estado != null && m.Estado.ToLower() == statusLower);
        }

        // Plan filter
        if (planId.HasValue)
        {
            query = query.Where(m => m.Plan == planId.Value);
        }

        var totalItems = await query.CountAsync();

        var plansDict = await _context.Planes
            .ToDictionaryAsync(p => (long)p.IdPlanes, p => p.Nombre);

        var membersList = await query
            .OrderByDescending(m => m.FechaIngreso)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MemberResponseDto
            {
                Id = m.IdMiembro,
                FirstName = m.Nombre,
                LastName = m.Apellido,
                Email = m.Correo,
                JoinDate = m.FechaIngreso,
                PlanId = m.Plan,
                PlanName = m.Plan.HasValue && plansDict.ContainsKey(m.Plan.Value)
                    ? plansDict[m.Plan.Value]
                    : null,
                Status = m.Estado ?? "Active"
            })
            .ToListAsync();

        var result = new PagedResultDto<MemberResponseDto>
        {
            Items = membersList,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Retrieves full member detail by ID (including weight records and payment history)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MemberDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMemberById(long id)
    {
        var member = await _context.Miembros
            .Include(m => m.RegistrosPesos)
            .Include(m => m.Pagos)
            .FirstOrDefaultAsync(m => m.IdMiembro == id);

        if (member == null)
        {
            return NotFound(new { message = $"Member with ID {id} was not found." });
        }

        string? planName = null;
        if (member.Plan.HasValue)
        {
            var planObj = await _context.Planes.FindAsync((int)member.Plan.Value);
            planName = planObj?.Nombre;
        }

        var detail = new MemberDetailDto
        {
            Id = member.IdMiembro,
            FirstName = member.Nombre,
            LastName = member.Apellido,
            Email = member.Correo,
            JoinDate = member.FechaIngreso,
            PlanId = member.Plan,
            PlanName = planName,
            Status = member.Estado ?? "Active",
            WeightRecords = member.RegistrosPesos
                .OrderByDescending(r => r.FechaRegistro)
                .Select(r => new WeightRecordDto
                {
                    Id = r.IdRegistroPesos,
                    Weight = r.Peso,
                    RecordDate = r.FechaRegistro
                }),
            Payments = member.Pagos
                .OrderByDescending(p => p.FechaVencimiento)
                .Select(p => new PaymentDto
                {
                    Id = p.IdPagos,
                    BilledMonth = p.MesFacturado,
                    Amount = p.Monto,
                    DueDate = p.FechaVencimiento,
                    PaymentDate = p.FechaPago,
                    Status = p.Estado
                })
        };

        return Ok(detail);
    }

    /// <summary>
    /// Register a new gym member
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MemberResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMember([FromBody] CreateMemberDto dto)
    {
        var emailExists = await _context.Miembros
            .AnyAsync(m => m.Correo.ToLower() == dto.Email.ToLower());

        if (emailExists)
        {
            return BadRequest(new { message = "A member with this email already exists." });
        }

        var member = new Miembro
        {
            Nombre = dto.FirstName,
            Apellido = dto.LastName,
            Correo = dto.Email.ToLower(),
            FechaIngreso = DateTime.UtcNow,
            Plan = dto.PlanId,
            Estado = string.IsNullOrWhiteSpace(dto.Status) ? "Active" : dto.Status
        };

        _context.Miembros.Add(member);
        await _context.SaveChangesAsync();

        string? planName = null;
        if (member.Plan.HasValue)
        {
            var planObj = await _context.Planes.FindAsync((int)member.Plan.Value);
            planName = planObj?.Nombre;
        }

        var response = new MemberResponseDto
        {
            Id = member.IdMiembro,
            FirstName = member.Nombre,
            LastName = member.Apellido,
            Email = member.Correo,
            JoinDate = member.FechaIngreso,
            PlanId = member.Plan,
            PlanName = planName,
            Status = member.Estado ?? "Active"
        };

        return CreatedAtAction(nameof(GetMemberById), new { id = member.IdMiembro }, response);
    }

    /// <summary>
    /// Update existing member information
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MemberResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMember(long id, [FromBody] UpdateMemberDto dto)
    {
        var member = await _context.Miembros.FindAsync(id);
        if (member == null)
        {
            return NotFound(new { message = $"Member with ID {id} not found." });
        }

        var emailExists = await _context.Miembros
            .AnyAsync(m => m.Correo.ToLower() == dto.Email.ToLower() && m.IdMiembro != id);

        if (emailExists)
        {
            return BadRequest(new { message = "Email is already in use by another member." });
        }

        member.Nombre = dto.FirstName;
        member.Apellido = dto.LastName;
        member.Correo = dto.Email.ToLower();
        member.Plan = dto.PlanId;
        member.Estado = dto.Status;

        await _context.SaveChangesAsync();

        string? planName = null;
        if (member.Plan.HasValue)
        {
            var planObj = await _context.Planes.FindAsync((int)member.Plan.Value);
            planName = planObj?.Nombre;
        }

        var response = new MemberResponseDto
        {
            Id = member.IdMiembro,
            FirstName = member.Nombre,
            LastName = member.Apellido,
            Email = member.Correo,
            JoinDate = member.FechaIngreso,
            PlanId = member.Plan,
            PlanName = planName,
            Status = member.Estado ?? "Active"
        };

        return Ok(response);
    }

    /// <summary>
    /// Disable a member (Soft delete) or delete permanently
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMember(long id, [FromQuery] bool softDelete = true)
    {
        var member = await _context.Miembros.FindAsync(id);
        if (member == null)
        {
            return NotFound(new { message = $"Member with ID {id} not found." });
        }

        if (softDelete)
        {
            member.Estado = "Inactive";
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Member with ID {id} has been set to Inactive." });
        }
        else
        {
            _context.Miembros.Remove(member);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"Member with ID {id} has been permanently deleted." });
        }
    }

    /// <summary>
    /// Add a weight record for member physical progress tracking
    /// </summary>
    [HttpPost("{id}/weight-records")]
    [ProducesResponseType(typeof(WeightRecordDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddWeightRecord(long id, [FromBody] AddWeightRecordDto dto)
    {
        var memberExists = await _context.Miembros.AnyAsync(m => m.IdMiembro == id);
        if (!memberExists)
        {
            return NotFound(new { message = $"Member with ID {id} not found." });
        }

        var record = new RegistroPeso
        {
            IdMiembro = id,
            Peso = dto.Weight,
            FechaRegistro = dto.RecordDate ?? DateTime.UtcNow
        };

        _context.RegistrosPesos.Add(record);
        await _context.SaveChangesAsync();

        var response = new WeightRecordDto
        {
            Id = record.IdRegistroPesos,
            Weight = record.Peso,
            RecordDate = record.FechaRegistro
        };

        return CreatedAtAction(nameof(GetMemberById), new { id }, response);
    }
}
