namespace FitwomanAPI.DTOs.Plans;

public class PlanResponseDto
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public double Precio { get; set; }
    public bool Estado { get; set; }
    public bool Destacado { get; set; }
    public string? MensajeWhatsapp { get; set; }
    public List<string> ItemsIncluidos { get; set; } = new();
}
