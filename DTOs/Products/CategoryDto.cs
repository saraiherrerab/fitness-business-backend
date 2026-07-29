using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Products;

public class CategoryDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int CantidadProductos { get; set; }
}

public class CreateCategoryDto
{
    [Required(ErrorMessage = "El nombre de la categoría es obligatorio.")]
    [MaxLength(100)]
    public string Nombre { get; set; } = string.Empty;
}
