using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Products;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista todas las categorías con el conteo de productos asociados
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<CategoryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _context.Categorias
            .Include(c => c.Productos)
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoryDto
            {
                Id = c.IdCategoria,
                Nombre = c.Nombre,
                CantidadProductos = c.Productos.Count
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Obtiene una categoría específica por su ID
    /// </summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategoryById(long id)
    {
        var category = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.IdCategoria == id);

        if (category == null)
        {
            return NotFound(new { message = $"No se encontró la categoría con ID {id}." });
        }

        var dto = new CategoryDto
        {
            Id = category.IdCategoria,
            Nombre = category.Nombre,
            CantidadProductos = category.Productos.Count
        };

        return Ok(dto);
    }

    /// <summary>
    /// Crea una nueva categoría de productos (Admin)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(CategoryDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var exists = await _context.Categorias
            .AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.Trim().ToLower());

        if (exists)
        {
            return BadRequest(new { message = $"Ya existe una categoría con el nombre '{dto.Nombre}'." });
        }

        var category = new Categoria
        {
            Nombre = dto.Nombre.Trim()
        };

        _context.Categorias.Add(category);
        await _context.SaveChangesAsync();

        var response = new CategoryDto
        {
            Id = category.IdCategoria,
            Nombre = category.Nombre,
            CantidadProductos = 0
        };

        return CreatedAtAction(nameof(GetCategoryById), new { id = category.IdCategoria }, response);
    }

    /// <summary>
    /// Actualiza el nombre de una categoría existente (Admin)
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateCategory(long id, [FromBody] CreateCategoryDto dto)
    {
        var category = await _context.Categorias.FindAsync(id);
        if (category == null)
        {
            return NotFound(new { message = $"No se encontró la categoría con ID {id}." });
        }

        var exists = await _context.Categorias
            .AnyAsync(c => c.IdCategoria != id && c.Nombre.ToLower() == dto.Nombre.Trim().ToLower());

        if (exists)
        {
            return BadRequest(new { message = $"Ya existe otra categoría con el nombre '{dto.Nombre}'." });
        }

        category.Nombre = dto.Nombre.Trim();
        await _context.SaveChangesAsync();

        return Ok(new { message = "Categoría actualizada exitosamente." });
    }

    /// <summary>
    /// Elimina una categoría (Admin). Falla si tiene productos asociados.
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCategory(long id)
    {
        var category = await _context.Categorias
            .Include(c => c.Productos)
            .FirstOrDefaultAsync(c => c.IdCategoria == id);

        if (category == null)
        {
            return NotFound(new { message = $"No se encontró la categoría con ID {id}." });
        }

        if (category.Productos.Any())
        {
            return BadRequest(new { message = $"No se puede eliminar la categoría porque tiene {category.Productos.Count} productos asociados." });
        }

        _context.Categorias.Remove(category);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Categoría eliminada exitosamente." });
    }
}
