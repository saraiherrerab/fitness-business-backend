namespace FitwomanAPI.DTOs.Teachers;

public class TeacherResponseDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public DateTime FechaDeNacimiento { get; set; }
    public int CantidadClasesAsignadas { get; set; }
}

public class AssignedClassDto
{
    public long Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
}

public class TeacherDetailDto : TeacherResponseDto
{
    public List<AssignedClassDto> Clases { get; set; } = new();
}
