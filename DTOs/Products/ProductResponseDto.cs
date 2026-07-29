namespace FitwomanAPI.DTOs.Products;

public class ProductResponseDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string? Tallas { get; set; }
    public int Estado { get; set; }
    public bool Visibilidad { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string? Imagen { get; set; }
    public long IdCategoria { get; set; }
    public string? NombreCategoria { get; set; }
}
