using System.ComponentModel.DataAnnotations;

namespace FitwomanAPI.DTOs.Members;

public class UpdateMemberDto
{
    [Required(ErrorMessage = "First name is required.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    public long? PlanId { get; set; }

    [Required(ErrorMessage = "Status is required.")]
    public string Status { get; set; } = "Active";
}
