using CleanLanka.Backend.Data;
using CleanLanka.Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CleanLankaUser = CleanLanka.Backend.Models.User;
using CleanLanka.Backend.Models;

namespace CleanLanka.Backend.Controllers
{
    [Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class RequestsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<CleanLankaUser> _userManager;

        private static readonly HashSet<string> AllowedStatuses = new(new[] { "Pending", "Confirmed", "Assigned", "Rejected", "Completed" });

        public RequestsController(AppDbContext context, UserManager<CleanLankaUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        private async Task CreateNotification(string userId, string title, string message, string type = "Info")
        {
            var notification = new Notification
            {
                Title = title,
                Message = message,
                Type = type,
                UserId = userId,
                Timestamp = DateTime.UtcNow,
                IsRead = false
            };
            await _context.Notifications.AddAsync(notification);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<WasteRequest>>> GetRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            if (User.IsInRole("Admin") || User.IsInRole("MunicipalityOfficer"))
            {
                return await _context.WasteRequests.Include(r => r.Citizen).OrderByDescending(r => r.RequestedDate).ToListAsync();
            }
            else if (User.IsInRole("Collector"))
            {
                // Collectors see requests in their zone or assigned to them
                return await _context.WasteRequests.Include(r => r.Citizen)
                    .Where(r => r.AssignedTo == user.Id || (r.AssignedTo == null && r.District == user.Zone))
                    .OrderByDescending(r => r.RequestedDate)
                    .ToListAsync();
            }
            else
            {
                // Citizens see only their own requests
                return await _context.WasteRequests.Include(r => r.Citizen)
                    .Where(r => r.CitizenId == user.Id)
                    .OrderByDescending(r => r.RequestedDate)
                    .ToListAsync();
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateRequest([FromBody] CreateWasteRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var request = new WasteRequest
            {
                RequestType = dto.RequestType,
                Location = dto.Location,
                District = dto.District,
                Municipality = dto.Municipality,
                Notes = dto.Notes,
                RequestedDate = DateTime.Now, // Or from DTO
                CitizenId = user.Id,
                Status = "Pending"
            };

            _context.WasteRequests.Add(request);
            await _context.Activities.AddAsync(new Activity { Action = "New Request", UserId = user.Id, Details = $"Request type {request.RequestType} created." });
            await CreateNotification(user.Id, "Request submitted", $"Your request for {request.RequestType} at {request.Location} was submitted.", "Success");
            await _context.SaveChangesAsync();
            return Ok(request);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,MunicipalityOfficer,Collector")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] RequestStatusUpdateDto dto)
        {
            if (!AllowedStatuses.Contains(dto.Status))
                return BadRequest(new { Message = "Invalid status." });

            var request = await _context.WasteRequests.FindAsync(id);
            if (request == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var isCollector = await _userManager.IsInRoleAsync(user, "Collector");
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            var isMunicipality = await _userManager.IsInRoleAsync(user, "MunicipalityOfficer");

            if (isCollector)
            {
                if (dto.Status != "Completed")
                    return Forbid();

                if (request.AssignedTo != user.Id)
                    return Forbid();

                if (request.Status != "Confirmed" && request.Status != "Assigned")
                    return BadRequest(new { Message = "Only confirmed/assigned requests can be completed." });
            }
            else if (isAdmin || isMunicipality)
            {
                if (dto.Status != "Rejected" && dto.Status != "Completed")
                    return BadRequest(new { Message = "Admins can only reject or complete via this endpoint. Use confirm/assign to approve." });
            }
            else
            {
                return Forbid();
            }

            request.Status = dto.Status;

            if (dto.Status == "Completed")
            {
                await CreateNotification(request.CitizenId, "Request completed", $"Your request #{request.Id} has been completed.", "Success");
            }
            else if (dto.Status == "Rejected")
            {
                await CreateNotification(request.CitizenId, "Request rejected", $"Your request #{request.Id} was rejected.", "Error");
            }

            await _context.SaveChangesAsync();
            return Ok(request);
        }

        [HttpPut("{id}/confirm")]
        [Authorize(Roles = "Admin,MunicipalityOfficer")]
        public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmRequestDto dto)
        {
            var request = await _context.WasteRequests.FindAsync(id);
            if (request == null) return NotFound();

            if (request.Status != "Pending")
                return BadRequest(new { Message = "Only pending requests can be confirmed or assigned." });

            var statusToSet = string.IsNullOrWhiteSpace(dto.Status) ? "Confirmed" : dto.Status;
            if (!AllowedStatuses.Contains(statusToSet))
                return BadRequest(new { Message = "Invalid status." });

            // Only allow confirm or assign from this endpoint
            if (statusToSet != "Confirmed" && statusToSet != "Assigned")
                return BadRequest(new { Message = "Use confirm to set Confirmed/Assigned only." });

            request.Status = statusToSet;

            if (!string.IsNullOrWhiteSpace(dto.CollectorId))
            {
                var collector = await _userManager.FindByIdAsync(dto.CollectorId);
                if (collector == null || !(await _userManager.IsInRoleAsync(collector, "Collector")))
                {
                    return BadRequest(new { Message = "Collector not found or invalid role." });
                }

                if (!string.Equals(collector.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Message = "Collector is not active." });
                }

                request.AssignedTo = dto.CollectorId;
                await CreateNotification(dto.CollectorId, "New collection assigned", $"Collect at {request.Location} ({request.District}) for request #{request.Id}.", "Warning");
            }

            await CreateNotification(request.CitizenId, "Request confirmed", $"Your request #{request.Id} is {statusToSet}.", "Success");
            await _context.SaveChangesAsync();
            return Ok(request);
        }
    }

    [Authorize(Roles = "Admin,MunicipalityOfficer")]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<CleanLankaUser> _userManager;
        public VehiclesController(AppDbContext context, UserManager<CleanLankaUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles() => await _context.Vehicles.ToListAsync();

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            if (!string.IsNullOrWhiteSpace(vehicle.DriverId))
            {
                var collector = await _userManager.FindByIdAsync(vehicle.DriverId);
                if (collector == null || !(await _userManager.IsInRoleAsync(collector, "Collector")) || !string.Equals(collector.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { Message = "Assigned collector is invalid or inactive." });
                }
            }

            _context.Vehicles.Add(vehicle);
            await _context.SaveChangesAsync();
            return Ok(vehicle);
        }

        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignCollector(int id, [FromBody] VehicleAssignmentDto dto)
        {
            var vehicle = await _context.Vehicles.FindAsync(id);
            if (vehicle == null) return NotFound();

            var collector = await _userManager.FindByIdAsync(dto.CollectorId);
            if (collector == null || !(await _userManager.IsInRoleAsync(collector, "Collector")) || !string.Equals(collector.Status, "Active", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(new { Message = "Collector not found or inactive." });
            }

            vehicle.DriverId = dto.CollectorId;
            await _context.SaveChangesAsync();
            return Ok(vehicle);
        }
    }

    [Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class NotificationsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<CleanLankaUser> _userManager;

        public NotificationsController(AppDbContext context, UserManager<CleanLankaUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Notification>>> GetNotifications()
        {
             var user = await _userManager.GetUserAsync(User);
             if (user == null) return Unauthorized();
             return await _context.Notifications.Where(n => n.UserId == user.Id).OrderByDescending(n => n.Timestamp).ToListAsync();
        }
    }
}
