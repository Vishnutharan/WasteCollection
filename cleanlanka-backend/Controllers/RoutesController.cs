using CleanLanka.Backend.Data;
using CleanLanka.Backend.DTOs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CleanLankaRoute = CleanLanka.Backend.Models.Route;
using CleanLankaUser = CleanLanka.Backend.Models.User;

namespace CleanLanka.Backend.Controllers
{
    [Authorize]
    [Microsoft.AspNetCore.Mvc.Route("api/[controller]")]
    [ApiController]
    public class RoutesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<CleanLankaUser> _userManager;

        public RoutesController(AppDbContext context, UserManager<CleanLankaUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CleanLankaRoute>>> GetRoutes()
        {
            return await _context.Routes.Include(r => r.Vehicle).ToListAsync();
        }

        [HttpPost]
        [Authorize(Roles = "Admin,MunicipalityOfficer")]
        public async Task<IActionResult> CreateRoute(CleanLankaRoute route)
        {
            _context.Routes.Add(route);
            await _context.SaveChangesAsync();
            return Ok(route);
        }
        
        [HttpPut("{id}")]
         public async Task<IActionResult> UpdateRoute(int id, CleanLankaRoute route)
        {
             if (id != route.Id) return BadRequest();
             _context.Entry(route).State = EntityState.Modified;
             await _context.SaveChangesAsync();
             return NoContent();
        }
    }
}
