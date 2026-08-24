using HasbitFlowApi.Data;
using HasbitFlowApi.DTOs.Habits;
using HasbitFlowApi.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace HasbitFlowApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HabitsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;
        public HabitsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateHabitDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var habit = new Habit
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = dto.Title,
                Description = dto.Description,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            _context.Habits.Add(habit);

            await _context.SaveChangesAsync();

            return Ok(habit);
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var habits = await _context.Habits
                .Where(h => h.UserId == userId).ToListAsync();

            return Ok(habits);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.UserId == userId && h.Id == id);

            if (habit == null)
            {
                return NotFound();
            }

            return Ok(habit);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Guid id,UpdateHabitDto dto)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            }

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id == id && h.UserId == userId);

            if(habit == null)
            {
                return NotFound();
            }

            habit.Title = dto.Title;
            habit.Description = dto.Description;
            habit.IsActive = dto.IsActive;

            await _context.SaveChangesAsync();
            
            return Ok(habit);
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if(!Guid.TryParse(userIdValue, out var userId))
            {
                return Unauthorized();
            };

            var habit = await _context.Habits
                .FirstOrDefaultAsync(h => h.Id ==id && h.UserId == userId);

            if (habit == null)
            {
                return NotFound();
            }

            _context.Habits.Remove(habit);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
