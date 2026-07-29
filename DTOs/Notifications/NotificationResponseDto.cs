namespace FitwomanAPI.DTOs.Notifications;

public class NotificationResponseDto
{
    public long Id { get; set; }
    public bool Tipo { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public bool Leida { get; set; }
    public DateTime FechaCreacion { get; set; }
    public string? EnlaceReferencia { get; set; }
}

public class NotificationUnreadCountDto
{
    public int UnreadCount { get; set; }
}
