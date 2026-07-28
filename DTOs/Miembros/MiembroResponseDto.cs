namespace FitwomanAPI.DTOs.Miembros;

public class MiembroResponseDto
{
    public long IdMiembro { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public long? PlanId { get; set; }
    public string? NombrePlan { get; set; }
    public string Estado { get; set; } = "Activo";
}

public class RegistroPesoDto
{
    public long IdRegistroPesos { get; set; }
    public decimal Peso { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class PagoDto
{
    public long IdPagos { get; set; }
    public string MesFacturado { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public DateTime? FechaPago { get; set; }
    public string Estado { get; set; } = string.Empty;
}

public class MiembroDetailDto : MiembroResponseDto
{
    public IEnumerable<RegistroPesoDto> RegistrosPesos { get; set; } = new List<RegistroPesoDto>();
    public IEnumerable<PagoDto> Pagos { get; set; } = new List<PagoDto>();
}

public class AddRegistroPesoDto
{
    public decimal Peso { get; set; }
    public DateTime? FechaRegistro { get; set; }
}
