namespace FitwomanAPI.DTOs.Classes;

public class ClassResponseDto
{
    public long Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public int Duracion { get; set; }
    public string Nivel { get; set; } = string.Empty;
    public int Cupos { get; set; }
    public string? Descripcion { get; set; }
    public long IdProfesor { get; set; }
    public string? NombreProfesor { get; set; }
    public int CantidadHorarios { get; set; }
}

public class TeacherDto
{
    public long Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public DateTime? FechaDeNacimiento { get; set; }
}

public class ScheduleSlotDto
{
    public long IdHorario { get; set; }
    public string DiaSemana { get; set; } = string.Empty;
    public string HoraInicio { get; set; } = string.Empty;
    public string HoraFin { get; set; } = string.Empty;
    public string? Aula { get; set; }
}

public class ClassDetailDto : ClassResponseDto
{
    public TeacherDto? Profesor { get; set; }
    public List<ScheduleSlotDto> Horarios { get; set; } = new();
}
