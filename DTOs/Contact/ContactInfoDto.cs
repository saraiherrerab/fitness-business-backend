namespace FitwomanAPI.DTOs.Contact;

public class ContactInfoDto
{
    public long Id { get; set; }
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public string? UrlGoogleMaps { get; set; }
}

public class UpdateContactInfoDto
{
    public string? Telefono { get; set; }
    public string? Correo { get; set; }
    public string? Direccion { get; set; }
    public string? Ciudad { get; set; }
    public string? Pais { get; set; }
    public string? UrlGoogleMaps { get; set; }
}
