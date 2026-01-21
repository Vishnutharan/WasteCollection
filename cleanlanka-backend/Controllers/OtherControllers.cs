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

        public RequestsController(AppDbContext context, UserManager<CleanLankaUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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
            await _context.SaveChangesAsync();
            return Ok(request);
        }

        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin,MunicipalityOfficer,Collector")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] string status)
        {
            var request = await _context.WasteRequests.FindAsync(id);
            if (request == null) return NotFound();

            request.Status = status;
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
        public VehiclesController(AppDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Vehicle>>> GetVehicles() => await _context.Vehicles.ToListAsync();

        [HttpPost]
        public async Task<IActionResult> CreateVehicle(Vehicle vehicle)
        {
            _context.Vehicles.Add(vehicle);
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
