using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Common;
using FitwomanAPI.DTOs.Miembros;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // Requiere token JWT para cualquier acción
public class MiembrosController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public MiembrosController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Listado paginado de miembros con filtros de búsqueda y estado
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultDto<MiembroResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMiembros(
        [FromQuery] string? search,
        [FromQuery] string? estado,
        [FromQuery] long? planId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Miembros.AsQueryable();

        // Filtro por búsqueda (nombre, apellido o correo)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(m =>
                m.Nombre.ToLower().Contains(searchLower) ||
                m.Apellido.ToLower().Contains(searchLower) ||
                m.Correo.ToLower().Contains(searchLower));
        }

        // Filtro por estado
        if (!string.IsNullOrWhiteSpace(estado))
        {
            var estadoLower = estado.Trim().ToLower();
            query = query.Where(m => m.Estado != null && m.Estado.ToLower() == estadoLower);
        }

        // Filtro por plan
        if (planId.HasValue)
        {
            query = query.Where(m => m.Plan == planId.Value);
        }

        var totalItems = await query.CountAsync();

        // Obtener todos los planes para hacer el Join en memoria o Include si fuera FK
        var planesDict = await _context.Planes
            .ToDictionaryAsync(p => (long)p.IdPlanes, p => p.Nombre);

        var miembrosList = await query
            .OrderByDescending(m => m.FechaIngreso)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(m => new MiembroResponseDto
            {
                IdMiembro = m.IdMiembro,
                Nombre = m.Nombre,
                Apellido = m.Apellido,
                Correo = m.Correo,
                FechaIngreso = m.FechaIngreso,
                PlanId = m.Plan,
                NombrePlan = m.Plan.HasValue && planesDict.ContainsKey(m.Plan.Value)
                    ? planesDict[m.Plan.Value]
                    : null,
                Estado = m.Estado ?? "Activo"
            })
            .ToListAsync();

        var result = new PagedResultDto<MiembroResponseDto>
        {
            Items = miembrosList,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Obtiene el detalle completo de un miembro por ID (con historial de peso y pagos)
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MiembroDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMiembroById(long id)
    {
        var miembro = await _context.Miembros
            .Include(m => m.RegistrosPesos)
            .Include(m => m.Pagos)
            .FirstOrDefaultAsync(m => m.IdMiembro == id);

        if (miembro == null)
        {
            return NotFound(new { message = $"Miembro con ID {id} no fue encontrado." });
        }

        string? nombrePlan = null;
        if (miembro.Plan.HasValue)
        {
            var planObj = await _context.Planes.FindAsync((int)miembro.Plan.Value);
            nombrePlan = planObj?.Nombre;
        }

        var detail = new MiembroDetailDto
        {
            IdMiembro = miembro.IdMiembro,
            Nombre = miembro.Nombre,
            Apellido = miembro.Apellido,
            Correo = miembro.Correo,
            FechaIngreso = miembro.FechaIngreso,
            PlanId = miembro.Plan,
            NombrePlan = nombrePlan,
            Estado = miembro.Estado ?? "Activo",
            RegistrosPesos = miembro.RegistrosPesos
                .OrderByDescending(r => r.FechaRegistro)
                .Select(r => new RegistroPesoDto
                {
                    IdRegistroPesos = r.IdRegistroPesos,
                    Peso = r.Peso,
                    FechaRegistro = r.FechaRegistro
                }),
            Pagos = miembro.Pagos
                .OrderByDescending(p => p.FechaVencimiento)
                .Select(p => new PagoDto
                {
                    IdPagos = p.IdPagos,
                    MesFacturado = p.MesFacturado,
                    Monto = p.Monto,
                    FechaVencimiento = p.FechaVencimiento,
                    FechaPago = p.FechaPago,
                    Estado = p.Estado
                })
        };

        return Ok(detail);
    }

    /// <summary>
    /// Registrar un nuevo miembro en el gimnasio
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(MiembroResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateMiembro([FromBody] CreateMiembroDto dto)
    {
        var existeCorreo = await _context.Miembros
            .AnyAsync(m => m.Correo.ToLower() == dto.Correo.ToLower());

        if (existeCorreo)
        {
            return BadRequest(new { message = "Ya existe un miembro registrado con este correo." });
        }

        var miembro = new Miembro
        {
            Nombre = dto.Nombre,
            Apellido = dto.Apellido,
            Correo = dto.Correo.ToLower(),
            FechaIngreso = DateTime.UtcNow,
            Plan = dto.Plan,
            Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Activo" : dto.Estado
        };

        _context.Miembros.Add(miembro);
        await _context.SaveChangesAsync();

        string? nombrePlan = null;
        if (miembro.Plan.HasValue)
        {
            var planObj = await _context.Planes.FindAsync((int)miembro.Plan.Value);
            nombrePlan = planObj?.Nombre;
        }

        var response = new MiembroResponseDto
        {
            IdMiembro = miembro.IdMiembro,
            Nombre = miembro.Nombre,
            Apellido = miembro.Apellido,
            Correo = miembro.Correo,
            FechaIngreso = miembro.FechaIngreso,
            PlanId = miembro.Plan,
            NombrePlan = nombrePlan,
            Estado = miembro.Estado ?? "Activo"
        };

        return CreatedAtAction(nameof(GetMiembroById), new { id = miembro.IdMiembro }, response);
    }

    /// <summary>
    /// Actualiza la información de un miembro existente
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(MiembroResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMiembro(long id, [FromBody] UpdateMiembroDto dto)
    {
        var miembro = await _context.Miembros.FindAsync(id);
        if (miembro == null)
        {
            return NotFound(new { message = $"Miembro con ID {id} no encontrado." });
        }

        var existeCorreo = await _context.Miembros
            .AnyAsync(m => m.Correo.ToLower() == dto.Correo.ToLower() && m.IdMiembro != id);

        if (existeCorreo)
        {
            return BadRequest(new { message = "El correo ya está en uso por otro miembro." });
        }

        miembro.Nombre = dto.Nombre;
        miembro.Apellido = dto.Apellido;
        miembro.Correo = dto.Correo.ToLower();
        miembro.Plan = dto.Plan;
        miembro.Estado = dto.Estado;

        await _context.SaveChangesAsync();

        string? nombrePlan = null;
        if (miembro.Plan.HasValue)
        {
            var planObj = await _context.Planes.FindAsync((int)miembro.Plan.Value);
            nombrePlan = planObj?.Nombre;
        }

        var response = new MiembroResponseDto
        {
            IdMiembro = miembro.IdMiembro,
            Nombre = miembro.Nombre,
            Apellido = miembro.Apellido,
            Correo = miembro.Correo,
            FechaIngreso = miembro.FechaIngreso,
            PlanId = miembro.Plan,
            NombrePlan = nombrePlan,
            Estado = miembro.Estado ?? "Activo"
        };

        return Ok(response);
    }

    /// <summary>
    /// Cambia el estado de un miembro a 'Inactivo' (Soft Delete) o eliminación física
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMiembro(long id, [FromQuery] bool softDelete = true)
    {
        var miembro = await _context.Miembros.FindAsync(id);
        if (miembro == null)
        {
            return NotFound(new { message = $"Miembro con ID {id} no encontrado." });
        }

        if (softDelete)
        {
            miembro.Estado = "Inactivo";
            await _context.SaveChangesAsync();
            return Ok(new { message = $"El miembro con ID {id} ha sido deshabilitado (Estado = Inactivo)." });
        }
        else
        {
            _context.Miembros.Remove(miembro);
            await _context.SaveChangesAsync();
            return Ok(new { message = $"El miembro con ID {id} ha sido eliminado permanentemente." });
        }
    }

    /// <summary>
    /// Registra un nuevo peso para el avance físico del miembro (Sección Nutrición/Progreso)
    /// </summary>
    [HttpPost("{id}/pesos")]
    [ProducesResponseType(typeof(RegistroPesoDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddRegistroPeso(long id, [FromBody] AddRegistroPesoDto dto)
    {
        var existeMiembro = await _context.Miembros.AnyAsync(m => m.IdMiembro == id);
        if (!existeMiembro)
        {
            return NotFound(new { message = $"Miembro con ID {id} no encontrado." });
        }

        var registro = new RegistroPeso
        {
            IdMiembro = id,
            Peso = dto.Peso,
            FechaRegistro = dto.FechaRegistro ?? DateTime.UtcNow
        };

        _context.RegistrosPesos.Add(registro);
        await _context.SaveChangesAsync();

        var response = new RegistroPesoDto
        {
            IdRegistroPesos = registro.IdRegistroPesos,
            Peso = registro.Peso,
            FechaRegistro = registro.FechaRegistro
        };

        return CreatedAtAction(nameof(GetMiembroById), new { id }, response);
    }
}
