using Asp.Versioning;
using FitwomanAPI.Data;
using FitwomanAPI.DTOs.Common;
using FitwomanAPI.DTOs.Products;
using FitwomanAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FitwomanAPI.Controllers.V1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/products")]
public class ProductsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ProductsController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lista paginada de productos para el panel de administración con múltiples filtros
    /// </summary>
    [HttpGet]
    [Authorize]
    [ProducesResponseType(typeof(PagedResultDto<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetProducts(
        [FromQuery] string? search,
        [FromQuery] long? idCategoria,
        [FromQuery] bool? visibilidad,
        [FromQuery] int? estado,
        [FromQuery] decimal? minPrecio,
        [FromQuery] decimal? maxPrecio,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 10;

        var query = _context.Productos
            .Include(p => p.Categoria)
            .AsQueryable();

        // Filtro por búsqueda (nombre o tallas)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchLower = search.Trim().ToLower();
            query = query.Where(p =>
                p.Nombre.ToLower().Contains(searchLower) ||
                (p.Tallas != null && p.Tallas.ToLower().Contains(searchLower)));
        }

        // Filtro por categoría
        if (idCategoria.HasValue)
        {
            query = query.Where(p => p.IdCategoria == idCategoria.Value);
        }

        // Filtro por visibilidad
        if (visibilidad.HasValue)
        {
            query = query.Where(p => p.Visibilidad == visibilidad.Value);
        }

        // Filtro por estado
        if (estado.HasValue)
        {
            query = query.Where(p => p.Estado == estado.Value);
        }

        // Filtro por rango de precios
        if (minPrecio.HasValue)
        {
            query = query.Where(p => p.Precio >= minPrecio.Value);
        }
        if (maxPrecio.HasValue)
        {
            query = query.Where(p => p.Precio <= maxPrecio.Value);
        }

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderByDescending(p => p.FechaRegistro)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductResponseDto
            {
                Id = p.IdProducto,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Tallas = p.Tallas,
                Estado = p.Estado,
                Visibilidad = p.Visibilidad,
                FechaRegistro = p.FechaRegistro,
                Imagen = p.Imagen,
                IdCategoria = p.IdCategoria,
                NombreCategoria = p.Categoria != null ? p.Categoria.Nombre : null
            })
            .ToListAsync();

        var result = new PagedResultDto<ProductResponseDto>
        {
            Items = items,
            TotalItems = totalItems,
            PageNumber = page,
            PageSize = pageSize
        };

        return Ok(result);
    }

    /// <summary>
    /// Catálogo público de productos visibles para la tienda en el Front Cliente
    /// </summary>
    [HttpGet("public")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPublicCatalog([FromQuery] long? categoryId)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Where(p => p.Visibilidad);

        if (categoryId.HasValue)
        {
            query = query.Where(p => p.IdCategoria == categoryId.Value);
        }

        var catalog = await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductResponseDto
            {
                Id = p.IdProducto,
                Nombre = p.Nombre,
                Precio = p.Precio,
                Tallas = p.Tallas,
                Estado = p.Estado,
                Visibilidad = p.Visibilidad,
                FechaRegistro = p.FechaRegistro,
                Imagen = p.Imagen,
                IdCategoria = p.IdCategoria,
                NombreCategoria = p.Categoria != null ? p.Categoria.Nombre : null
            })
            .ToListAsync();

        return Ok(catalog);
    }

    /// <summary>
    /// Obtiene el detalle de un producto por su ID
    /// </summary>
    [HttpGet("{id:long}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProductById(long id)
    {
        var product = await _context.Productos
            .Include(p => p.Categoria)
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        if (product == null)
        {
            return NotFound(new { message = $"No se encontró el producto con ID {id}." });
        }

        var dto = new ProductResponseDto
        {
            Id = product.IdProducto,
            Nombre = product.Nombre,
            Precio = product.Precio,
            Tallas = product.Tallas,
            Estado = product.Estado,
            Visibilidad = product.Visibilidad,
            FechaRegistro = product.FechaRegistro,
            Imagen = product.Imagen,
            IdCategoria = product.IdCategoria,
            NombreCategoria = product.Categoria?.Nombre
        };

        return Ok(dto);
    }

    /// <summary>
    /// Registra un nuevo producto en el catálogo (Admin)
    /// </summary>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProduct([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var categoriaExists = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria);
        if (!categoriaExists)
        {
            return BadRequest(new { message = $"La categoría con ID {dto.IdCategoria} no existe." });
        }

        var product = new Producto
        {
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio,
            Tallas = dto.Tallas?.Trim(),
            Estado = dto.Estado,
            Visibilidad = dto.Visibilidad,
            FechaRegistro = DateTime.UtcNow,
            Imagen = dto.Imagen?.Trim(),
            IdCategoria = dto.IdCategoria
        };

        _context.Productos.Add(product);
        await _context.SaveChangesAsync();

        var categoria = await _context.Categorias.FindAsync(dto.IdCategoria);

        var response = new ProductResponseDto
        {
            Id = product.IdProducto,
            Nombre = product.Nombre,
            Precio = product.Precio,
            Tallas = product.Tallas,
            Estado = product.Estado,
            Visibilidad = product.Visibilidad,
            FechaRegistro = product.FechaRegistro,
            Imagen = product.Imagen,
            IdCategoria = product.IdCategoria,
            NombreCategoria = categoria?.Nombre
        };

        return CreatedAtAction(nameof(GetProductById), new { id = product.IdProducto }, response);
    }

    /// <summary>
    /// Actualiza la información de un producto existente (Admin)
    /// </summary>
    [HttpPut("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProduct(long id, [FromBody] UpdateProductDto dto)
    {
        var product = await _context.Productos.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"No se encontró el producto con ID {id}." });
        }

        if (dto.IdCategoria.HasValue)
        {
            var categoriaExists = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria.Value);
            if (!categoriaExists)
            {
                return BadRequest(new { message = $"La categoría con ID {dto.IdCategoria.Value} no existe." });
            }
            product.IdCategoria = dto.IdCategoria.Value;
        }

        if (!string.IsNullOrWhiteSpace(dto.Nombre)) product.Nombre = dto.Nombre.Trim();
        if (dto.Precio.HasValue) product.Precio = dto.Precio.Value;
        if (dto.Tallas != null) product.Tallas = dto.Tallas.Trim();
        if (dto.Estado.HasValue) product.Estado = dto.Estado.Value;
        if (dto.Visibilidad.HasValue) product.Visibilidad = dto.Visibilidad.Value;
        if (dto.Imagen != null) product.Imagen = dto.Imagen.Trim();

        await _context.SaveChangesAsync();

        return Ok(new { message = "Producto actualizado exitosamente." });
    }

    /// <summary>
    /// Cambia la visibilidad pública de un producto (Admin)
    /// </summary>
    [HttpPatch("{id:long}/visibility")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleVisibility(long id, [FromQuery] bool visibility)
    {
        var product = await _context.Productos.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"No se encontró el producto con ID {id}." });
        }

        product.Visibilidad = visibility;
        await _context.SaveChangesAsync();

        return Ok(new { message = $"Visibilidad del producto actualizada a {visibility}." });
    }

    /// <summary>
    /// Elimina un producto por su ID (Admin)
    /// </summary>
    [HttpDelete("{id:long}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(long id)
    {
        var product = await _context.Productos.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { message = $"No se encontró el producto con ID {id}." });
        }

        _context.Productos.Remove(product);
        await _context.SaveChangesAsync();

        return Ok(new { message = "Producto eliminado exitosamente del catálogo." });
    }
}
