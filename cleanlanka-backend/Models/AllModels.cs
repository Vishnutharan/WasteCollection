using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CleanLanka.Backend.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string Role { get; set; } = "Citizen"; // Admin, MunicipalityOfficer, Collector, Citizen
        public string? Zone { get; set; }
        public string Status { get; set; } = "Active";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class WasteRequest
    {
        public int Id { get; set; }
        public string RequestType { get; set; } = string.Empty; // Plastic, Organic, e-Waste
        public string Location { get; set; } = string.Empty;
        public string District { get; set; } = string.Empty;
        public string Municipality { get; set; } = string.Empty;
        public DateTime RequestedDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string CitizenId { get; set; } = string.Empty;
        public User? Citizen { get; set; }
        public string? AssignedTo { get; set; } // Driver/Collector ID
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class Vehicle
    {
        public int Id { get; set; }
        public string Type { get; set; } = string.Empty; // Truck, Van
        public string LicensePlate { get; set; } = string.Empty;
        public double Capacity { get; set; } // in tons or units
        public string Status { get; set; } = "Available"; // Available, OnRoute, Maintenance
        public string? DriverId { get; set; }
        public DateTime? LastService { get; set; }
    }

    public class Route
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public Vehicle? Vehicle { get; set; }
        public string? DriverId { get; set; }
        public string Zone { get; set; } = string.Empty;
        public string Stops { get; set; } = string.Empty; // JSON or comma-separated
        public DateTime StartTime { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, InProgress, Completed
        public int CompletionPercentage { get; set; }
    }

    public class Notification
    {
        public int Id { get; set; }
        public string Type { get; set; } = "Info"; // Success, Warning, Info, Error
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; }
        public string UserId { get; set; } = string.Empty;
    }

    public class Activity
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string Action { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
    }
}
