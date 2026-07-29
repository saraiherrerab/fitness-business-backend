using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Common;
using FitwomanAPI.DTOs.Teachers;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/teachers")]
public class TeachersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public TeachersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista paginada de profesores/instructores del gimnasio con filtro de búsqueda por nombre o apellido
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResultDto<TeacherResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTeachers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Profesores
            .Include(p => p.Clases)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(searchLower) ||
                p.Apellido.ToLower().Contains(searchLower));
        }

        var totalItems = await query.CountAsync();

        var teachersList = await query
            .OrderBy(p => p.Nombre)
            .ThenBy(p => p.Apellido)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new TeacherResponseDto
            {
                Id = p.IdProfesores,
                Nombre = p.Nombre,
                Apellido = p.Apellido,
                FechaDeNacimiento = p.FechaDeNacimiento,
                CantidadClasesAsignadas = p.Clases.Count
            })
            .ToListAsync();

        var result = new PagedResultDto<TeacherResponseDto>
        {
            Items = teachersList,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Lista completa de profesores (sin paginación) para llenar selectores y desplegables en los portales
    /// </summary>
    [HttpGet("all")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<TeacherResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTeachers()
    {
        var teachers = await _context.Profesores
            .Include(p => p.Clases)
            .OrderBy(p => p.Nombre)
            .Select(p => new TeacherResponseDto
            {
                Id = p.IdProfesores,
                Nombre = p.Nombre,
                Apellido = p.Apellido,
                FechaDeNacimiento = p.FechaDeNacimiento,
                CantidadClasesAsignadas = p.Clases.Count
            })
            .ToListAsync();

        return Ok(teachers);
    }

    /// <summary>
    /// Obtiene el detalle completo de un profesor por su ID (incluyendo sus clases asignadas)
    /// </summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TeacherDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTeacherById(long id)
    {
        var teacher = await _context.Profesores
            .Include(p => p.Clases)
            .FirstOrDefaultAsync(p => p.IdProfesores == id);

        if (teacher == null)
        {
            return NotFound(new { message = $"No se encontró el profesor con ID {id}." });
        }

        var result = new TeacherDetailDto
        {
            Id = teacher.IdProfesores,
            Nombre = teacher.Nombre,
            Apellido = teacher.Apellido,
            FechaDeNacimiento = teacher.FechaDeNacimiento,
            CantidadClasesAsignadas = teacher.Clases.Count,
            Clases = teacher.Clases.Select(c => new AssignedClassDto
            {
                Id = c.IdClases,
                Tipo = c.Tipo,
                Modalidad = c.Modalidad,
                Nivel = c.Nivel
            }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Registra un nuevo profesor/instructor en el sistema (Admin)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(TeacherResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTeacher([FromBody] CreateTeacherDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var teacher = new Profesor
        {
            Nombre = dto.Nombre.Trim(),
            Apellido = dto.Apellido.Trim(),
            FechaDeNacimiento = DateTime.SpecifyKind(dto.FechaDeNacimiento, DateTimeKind.Utc)
        };

        _context.Profesores.Add(teacher);
        await _context.SaveChangesAsync();

        var response = new TeacherResponseDto
        {
            Id = teacher.IdProfesores,
            Nombre = teacher.Nombre,
            Apellido = teacher.Apellido,
            FechaDeNacimiento = teacher.FechaDeNacimiento,
            CantidadClasesAsignadas = 0
        };

        return CreatedAtAction(nameof(GetTeacherById), new { id = teacher.IdProfesores }, response);
    }

    /// <summary>
    /// Actualiza la información de un profesor existente (Admin)
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTeacher(long id, [FromBody] UpdateTeacherDto dto)
    {
        var teacher = await _context.Profesores.FindAsync(id);
        if (teacher == null)
        {
            return NotFound(new { message = $"No se encontró el profesor con ID {id}." });
        }

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) teacher.Nombre = dto.Nombre.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Apellido)) teacher.Apellido = dto.Apellido.Trim();
        if (dto.FechaDeNacimiento.HasValue) teacher.FechaDeNacimiento = DateTime.SpecifyKind(dto.FechaDeNacimiento.Value, DateTimeKind.Utc);

        await _context.SaveChangesAsync();

        return Ok(new { message = "Información del profesor actualizada exitosamente." });
    }

    /// <summary>
    /// Elimina un profesor por su ID (Admin). Falla si tiene clases asignadas.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTeacher(long id)
    {
        var teacher = await _context.Profesores
            .Include(p => p.Clases)
            .FirstOrDefaultAsync(p => p.IdProfesores == id);

        if (teacher == null)
        {
            return NotFound(new { message = $"No se encontró el profesor con ID {id}." });
        }

        if (teacher.Clases.Any())
        {
            return BadRequest(new { message = $"No se puede eliminar al profesor porque tiene {teacher.Clases.Count} clase(s) asignada(s)." });
        }

        _context.Profesores.Remove(teacher);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Profesor eliminado exitosamente." });
    }
}
