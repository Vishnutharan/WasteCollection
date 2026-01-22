using System.ComponentModel.DataAnnotations;

namespace CleanLanka.Backend.DTOs
{
    public class LoginDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class RegisterDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = "Citizen";
        public string? Zone { get; set; }
    }

    public class UserDto
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string? Zone { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class CreateWasteRequestDto
    {
        [Required]
        public string RequestType { get; set; } = string.Empty;
        [Required]
        public string Location { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Municipality { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
    }

    public class WasteRequestDto
    {
        public int Id { get; set; }
        public string RequestType { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public string CitizenName { get; set; } = string.Empty;
    }

    public class RequestStatusUpdateDto
    {
        [Required]
        public string Status { get; set; } = string.Empty;
    }

    public class ConfirmRequestDto
    {
        public string Status { get; set; } = "Confirmed";
        public string? CollectorId { get; set; }
        public string? Note { get; set; }
    }

    public class VehicleAssignmentDto
    {
        [Required]
        public string CollectorId { get; set; } = string.Empty;
    }
}
