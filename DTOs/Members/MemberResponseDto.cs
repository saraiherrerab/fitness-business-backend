namespace FitwomanAPI.DTOs.Members;

public class MemberResponseDto
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime JoinDate { get; set; }
    public long? PlanId { get; set; }
    public string? PlanName { get; set; }
    public string Status { get; set; } = "Active";
}

public class WeightRecordDto
{
    public long Id { get; set; }
    public decimal Weight { get; set; }
    public DateTime RecordDate { get; set; }
}

public class PaymentDto
{
    public long Id { get; set; }
    public string BilledMonth { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string Status { get; set; } = string.Empty;
}

public class MemberDetailDto : MemberResponseDto
{
    public IEnumerable<WeightRecordDto> WeightRecords { get; set; } = new List<WeightRecordDto>();
    public IEnumerable<PaymentDto> Payments { get; set; } = new List<PaymentDto>();
}

public class AddWeightRecordDto
{
    public decimal Weight { get; set; }
    public DateTime? RecordDate { get; set; }
}
