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
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<CleanLankaUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _context;

        public UsersController(UserManager<CleanLankaUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext context)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users.Select(u => new UserDto
            {
                Id = u.Id,
                Email = u.Email,
                FullName = u.FullName,
                Role = u.Role,
                Zone = u.Zone,
                Status = u.Status
            }).ToListAsync();

            return Ok(users);
        }

        [HttpGet("collectors")]
        [Authorize(Roles = "Admin,MunicipalityOfficer")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetCollectors()
        {
            var collectors = await _userManager.GetUsersInRoleAsync("Collector");
            var result = collectors
                .Where(u => string.Equals(u.Status, "Active", StringComparison.OrdinalIgnoreCase))
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    FullName = u.FullName,
                    Role = u.Role,
                    Zone = u.Zone,
                    Status = u.Status
                });
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterDto model)
        {
             var user = new CleanLankaUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                Role = model.Role,
                Zone = model.Zone,
                Status = string.Equals(model.Role, "Collector", StringComparison.OrdinalIgnoreCase) ? "Active" : "Active"
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, model.Role);
                return Ok(new { Message = "User created successfully" });
            }
            return BadRequest(result.Errors);
        }

        [HttpPut("{id}/approve-collector")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ApproveCollector(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (!string.Equals(user.Role, "Collector", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Message = "User is not a collector." });

            user.Status = "Active";
            user.Role = "Collector";
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors);

            // Ensure correct role membership
            if (await _userManager.IsInRoleAsync(user, "Citizen"))
                await _userManager.RemoveFromRoleAsync(user, "Citizen");

            if (!await _userManager.IsInRoleAsync(user, "Collector"))
                await _userManager.AddToRoleAsync(user, "Collector");

            return Ok(new { Message = "Collector approved." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            await _userManager.DeleteAsync(user);
            return Ok(new { Message = "User deleted" });
        }
    }
}
