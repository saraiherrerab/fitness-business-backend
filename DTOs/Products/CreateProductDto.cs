using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Products;

public class CreateProductDto
{
    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [MaxLength(150)]
    public string Nombre { get; set; } = string.Empty;

    [Range(0.01, 100000.00, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [MaxLength(100)]
    public string? Tallas { get; set; }

    [Range(0, 2, ErrorMessage = "El estado debe ser 1 (disponible), 0 (agotado) o 2 (descontinuado).")]
    public int Estado { get; set; } = 1;

    public bool Visibilidad { get; set; } = true;

    [MaxLength(500)]
    public string? Imagen { get; set; }

    [Required(ErrorMessage = "La categoría del producto es obligatoria.")]
    public long IdCategoria { get; set; }
}
