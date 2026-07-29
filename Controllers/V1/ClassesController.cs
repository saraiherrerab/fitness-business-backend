using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Classes;
using FitwomanAPI.DTOs.Common;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/classes")]
public class ClassesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClassesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista paginada de clases con filtros opcionales (búsqueda, tipo, modalidad, nivel, profesor)
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResultDto<ClassResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetClasses(
        [FromQuery] string? search,
        [FromQuery] string? tipo,
        [FromQuery] string? modalidad,
        [FromQuery] string? nivel,
        [FromQuery] long? idProfesor,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Clases
            .Include(c => c.Profesor)
            .Include(c => c.ClasesHorarios)
            .AsQueryable();

        // Filtro por búsqueda abierta (tipo o descripción)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(c =>
                c.Tipo.ToLower().Contains(searchLower) ||
                (c.Descripcion != null && c.Descripcion.ToLower().Contains(searchLower)));
        }

        // Filtro por tipo exacto (ej. Pole Dance, TRX, Pilates)
        if (!string.IsNullOrWhiteSpace(tipo))
        {
            var tipoLower = tipo.Trim().ToLower();
            query = query.Where(c => c.Tipo.ToLower() == tipoLower);
        }

        // Filtro por modalidad (ej. Presencial, Virtual)
        if (!string.IsNullOrWhiteSpace(modalidad))
        {
            var modLower = modalidad.Trim().ToLower();
            query = query.Where(c => c.Modalidad.ToLower() == modLower);
        }

        // Filtro por nivel (ej. Principiante, Intermedio, Avanzado)
        if (!string.IsNullOrWhiteSpace(nivel))
        {
            var nivelLower = nivel.Trim().ToLower();
            query = query.Where(c => c.Nivel.ToLower() == nivelLower);
        }

        // Filtro por profesor
        if (idProfesor.HasValue)
        {
            query = query.Where(c => c.IdProfesor == idProfesor.Value);
        }

        var totalItems = await query.CountAsync();

        var classesList = await query
            .OrderBy(c => c.Tipo)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new ClassResponseDto
            {
                Id = c.IdClases,
                Tipo = c.Tipo,
                Modalidad = c.Modalidad,
                Duracion = c.Duracion,
                Nivel = c.Nivel,
                Cupos = c.Cupos,
                Descripcion = c.Descripcion,
                IdProfesor = c.IdProfesor,
                NombreProfesor = c.Profesor != null ? $"{c.Profesor.Nombre} {c.Profesor.Apellido}".Trim() : null,
                CantidadHorarios = c.ClasesHorarios.Count
            })
            .ToListAsync();

        var result = new PagedResultDto<ClassResponseDto>
        {
            Items = classesList,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Endpoint público para el Portal Cliente / Landing Web para listar las clases disponibles
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ClassDetailDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicClasses()
    {
        var classes = await _context.Clases
            .Include(c => c.Profesor)
            .Include(c => c.ClasesHorarios)
                .ThenInclude(ch => ch.Horario)
            .OrderBy(c => c.Tipo)
            .Select(c => new ClassDetailDto
            {
                Id = c.IdClases,
                Tipo = c.Tipo,
                Modalidad = c.Modalidad,
                Duracion = c.Duracion,
                Nivel = c.Nivel,
                Cupos = c.Cupos,
                Descripcion = c.Descripcion,
                IdProfesor = c.IdProfesor,
                NombreProfesor = c.Profesor != null ? $"{c.Profesor.Nombre} {c.Profesor.Apellido}".Trim() : null,
                CantidadHorarios = c.ClasesHorarios.Count,
                Profesor = c.Profesor != null ? new TeacherDto
                {
                    Id = c.Profesor.IdProfesores,
                    Nombre = c.Profesor.Nombre,
                    Apellido = c.Profesor.Apellido,
                    FechaDeNacimiento = c.Profesor.FechaDeNacimiento
                } : null,
                Horarios = c.ClasesHorarios
                    .Where(ch => ch.Horario != null)
                    .Select(ch => new ScheduleSlotDto
                    {
                        IdHorario = ch.IdHorarios,
                        DiaSemana = ch.Horario!.DiaSemana,
                        HoraInicio = ch.Horario.HoraInicio.ToString(@"hh\:mm"),
                        HoraFin = ch.Horario.HoraFin.ToString(@"hh\:mm"),
                        Aula = ch.Aula
                    }).ToList()
            })
            .ToListAsync();

        return Ok(classes);
    }

    /// <summary>
    /// Obtiene los tipos de clases y el conteo por tipo (Útil para gráficos de distribución en el Dashboard y filtros)
    /// </summary>
    [HttpGet("types")]
    [AllowAnonymous]
    public async Task<IActionResult> GetClassTypesDistribution()
    {
        var distribution = await _context.Clases
            .GroupBy(c => c.Tipo)
            .Select(g => new
            {
                Tipo = g.Key,
                Count = g.Count()
            })
            .ToListAsync();

        return Ok(distribution);
    }

    /// <summary>
    /// Obtiene el detalle completo de una clase por su ID (incluyendo profesor y horarios)
    /// </summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ClassDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetClassById(long id)
    {
        var clase = await _context.Clases
            .Include(c => c.Profesor)
            .Include(c => c.ClasesHorarios)
                .ThenInclude(ch => ch.Horario)
            .FirstOrDefaultAsync(c => c.IdClases == id);

        if (clase == null)
        {
            return NotFound(new { message = $"No se encontró la clase con ID {id}." });
        }

        var result = new ClassDetailDto
        {
            Id = clase.IdClases,
            Tipo = clase.Tipo,
            Modalidad = clase.Modalidad,
            Duracion = clase.Duracion,
            Nivel = clase.Nivel,
            Cupos = clase.Cupos,
            Descripcion = clase.Descripcion,
            IdProfesor = clase.IdProfesor,
            NombreProfesor = clase.Profesor != null ? $"{clase.Profesor.Nombre} {clase.Profesor.Apellido}".Trim() : null,
            CantidadHorarios = clase.ClasesHorarios.Count,
            Profesor = clase.Profesor != null ? new TeacherDto
            {
                Id = clase.Profesor.IdProfesores,
                Nombre = clase.Profesor.Nombre,
                Apellido = clase.Profesor.Apellido,
                FechaDeNacimiento = clase.Profesor.FechaDeNacimiento
            } : null,
            Horarios = clase.ClasesHorarios
                .Where(ch => ch.Horario != null)
                .Select(ch => new ScheduleSlotDto
                {
                    IdHorario = ch.IdHorarios,
                    DiaSemana = ch.Horario!.DiaSemana,
                    HoraInicio = ch.Horario.HoraInicio.ToString(@"hh\:mm"),
                    HoraFin = ch.Horario.HoraFin.ToString(@"hh\:mm"),
                    Aula = ch.Aula
                }).ToList()
        };

        return Ok(result);
    }

    /// <summary>
    /// Crea una nueva clase en el sistema (Admin)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ClassResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateClass([FromBody] CreateClassDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Verificar que el profesor exista
        var profesorExists = await _context.Profesores.AnyAsync(p => p.IdProfesores == dto.IdProfesor);
        if (!profesorExists)
        {
            return BadRequest(new { message = $"El profesor con ID {dto.IdProfesor} no existe." });
        }

        var newClass = new Clase
        {
            Tipo = dto.Tipo.Trim(),
            Modalidad = dto.Modalidad.Trim(),
            Duracion = dto.Duracion,
            Nivel = dto.Nivel.Trim(),
            Cupos = dto.Cupos,
            Descripcion = dto.Descripcion?.Trim(),
            IdProfesor = dto.IdProfesor
        };

        _context.Clases.Add(newClass);
        await _context.SaveChangesAsync();

        // Asignar horarios opcionales si se proporcionaron
        if (dto.HorarioIds != null && dto.HorarioIds.Any())
        {
            var existingHorarioIds = await _context.Horarios
                .Where(h => dto.HorarioIds.Contains(h.IdHorarios))
                .Select(h => h.IdHorarios)
                .ToListAsync();

            foreach (var hId in existingHorarioIds)
            {
                _context.ClasesHorarios.Add(new ClaseHorario
                {
                    IdClases = newClass.IdClases,
                    IdHorarios = hId
                });
            }

            await _context.SaveChangesAsync();
        }

        var profesor = await _context.Profesores.FindAsync(dto.IdProfesor);

        var response = new ClassResponseDto
        {
            Id = newClass.IdClases,
            Tipo = newClass.Tipo,
            Modalidad = newClass.Modalidad,
            Duracion = newClass.Duracion,
            Nivel = newClass.Nivel,
            Cupos = newClass.Cupos,
            Descripcion = newClass.Descripcion,
            IdProfesor = newClass.IdProfesor,
            NombreProfesor = profesor != null ? $"{profesor.Nombre} {profesor.Apellido}".Trim() : null,
            CantidadHorarios = dto.HorarioIds?.Count ?? 0
        };

        return CreatedAtAction(nameof(GetClassById), new { id = newClass.IdClases }, response);
    }

    /// <summary>
    /// Actualiza los datos de una clase existente (Admin)
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateClass(long id, [FromBody] UpdateClassDto dto)
    {
        var clase = await _context.Clases.FindAsync(id);
        if (clase == null)
        {
            return NotFound(new { message = $"No se encontró la clase con ID {id}." });
        }

        if (dto.IdProfesor.HasValue)
        {
            var profesorExists = await _context.Profesores.AnyAsync(p => p.IdProfesores == dto.IdProfesor.Value);
            if (!profesorExists)
            {
                return BadRequest(new { message = $"El profesor con ID {dto.IdProfesor.Value} no existe." });
            }
            clase.IdProfesor = dto.IdProfesor.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Tipo)) clase.Tipo = dto.Tipo.Trim();
        if (!string.IsNullOrWhiteSpace(dto.Modalidad)) clase.Modalidad = dto.Modalidad.Trim();
        if (dto.Duracion.HasValue) clase.Duracion = dto.Duracion.Value;
        if (!string.IsNullOrWhiteSpace(dto.Nivel)) clase.Nivel = dto.Nivel.Trim();
        if (dto.Cupos.HasValue) clase.Cupos = dto.Cupos.Value;
        if (dto.Descripcion != null) clase.Descripcion = dto.Descripcion.Trim();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Clase actualizada exitosamente." });
    }

    /// <summary>
    /// Elimina una clase por su ID (Admin)
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteClass(long id)
    {
        var clase = await _context.Clases
            .Include(c => c.ClasesHorarios)
            .FirstOrDefaultAsync(c => c.IdClases == id);

        if (clase == null)
        {
            return NotFound(new { message = $"No se encontró la clase con ID {id}." });
        }

        // Eliminar relaciones con horarios primero
        _context.ClasesHorarios.RemoveRange(clase.ClasesHorarios);
        _context.Clases.Remove(clase);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Clase eliminada exitosamente." });
    }

    /// <summary>
    /// Asigna un horario a una clase (Admin)
    /// </summary>
    [HttpPost("{id:long}/schedules")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignSchedule(long id, [FromBody] AssignScheduleDto dto)
    {
        var claseExists = await _context.Clases.AnyAsync(c => c.IdClases == id);
        if (!claseExists)
        {
            return NotFound(new { message = $"No se encontró la clase con ID {id}." });
        }

        var horarioExists = await _context.Horarios.AnyAsync(h => h.IdHorarios == dto.IdHorario);
        if (!horarioExists)
        {
            return NotFound(new { message = $"No se encontró el horario con ID {dto.IdHorario}." });
        }

        var existingAssignment = await _context.ClasesHorarios
            .FirstOrDefaultAsync(ch => ch.IdClases == id && ch.IdHorarios == dto.IdHorario);

        if (existingAssignment != null)
        {
            return BadRequest(new { message = "Este horario ya se encuentra asignado a la clase." });
        }

        var assignment = new ClaseHorario
        {
            IdClases = id,
            IdHorarios = dto.IdHorario,
            Aula = dto.Aula?.Trim()
        };

        _context.ClasesHorarios.Add(assignment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Horario asignado correctamente a la clase." });
    }

    /// <summary>
    /// Remueve la asignación de un horario de una clase (Admin)
    /// </summary>
    [HttpDelete("{id:long}/schedules/{scheduleId:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveSchedule(long id, long scheduleId)
    {
        var assignment = await _context.ClasesHorarios
            .FirstOrDefaultAsync(ch => ch.IdClases == id && ch.IdHorarios == scheduleId);

        if (assignment == null)
        {
            return NotFound(new { message = "No se encontró la asociación entre esta clase y el horario especificado." });
        }

        _context.ClasesHorarios.Remove(assignment);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Horario desasignado correctamente de la clase." });
    }
}
