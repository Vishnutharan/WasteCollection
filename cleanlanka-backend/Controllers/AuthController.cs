using CleanLanka.Backend.Models;
using CleanLanka.Backend.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using CleanLankaUser = CleanLanka.Backend.Models.User;

namespace CleanLanka.Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<CleanLankaUser> _userManager;
        private readonly SignInManager<CleanLankaUser> _signInManager;
        private readonly IConfiguration _configuration;

        public AuthController(UserManager<CleanLankaUser> userManager, SignInManager<CleanLankaUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                if (!string.Equals(user.Status, "Active", StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized(new { Message = "Account is not active. Please contact admin." });
                }

                var userRoles = await _userManager.GetRolesAsync(user);
                var effectiveRole = userRoles.FirstOrDefault() ?? user.Role ?? "Citizen";

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim("Role", effectiveRole),
                    new Claim("Zone", user.Zone ?? "All")
                };

                foreach (var role in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, role));
                }

                if (!userRoles.Any() && !string.IsNullOrWhiteSpace(effectiveRole))
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, effectiveRole));
                }

                var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));

                var token = new JwtSecurityToken(
                    expires: DateTime.Now.AddHours(3),
                    claims: authClaims,
                    signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                    );

                return Ok(new
                {
                    token = new JwtSecurityTokenHandler().WriteToken(token),
                    expiration = token.ValidTo,
                    user = new { user.FullName, user.Email, user.Role, user.Zone }
                });
            }
            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var allowedRoles = new[] { "Citizen", "Collector" };
            var requestedRole = string.IsNullOrWhiteSpace(model.Role) ? "Citizen" : model.Role;
            if (!allowedRoles.Contains(requestedRole, StringComparer.OrdinalIgnoreCase))
                return BadRequest(new { Message = "Invalid role selected." });

            var userExists = await _userManager.FindByEmailAsync(model.Email);
            if (userExists != null)
                return Conflict(new { Status = "Error", Message = "User already exists!" });

            var role = requestedRole;
            var status = role == "Collector" ? "Pending" : "Active";

            CleanLankaUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Email,
                FullName = model.FullName,
                Role = role,
                Zone = model.Zone,
                Status = status
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new
                {
                    Status = "Error",
                    Message = "User creation failed! Please check user details and try again.",
                    Errors = result.Errors.Select(e => e.Description)
                });

            if (role == "Citizen")
            {
                await _userManager.AddToRoleAsync(user, role);
            }

            return Ok(new { Status = "Success", Message = role == "Collector" ? "Collector registered. Awaiting admin approval." : "User created successfully!" });
        }
    }
}
