using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagerAPI.Data;
using TaskManagerAPI.Models;

namespace TaskManagerAPI.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasks()
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
            return Ok(tasks);
        }

        [HttpGet("completed")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetCompletedTasks()
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Where(t => t.IsCompleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("pending")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetPendingTasks()
        {
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Where(t => !t.IsCompleted)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> SearchTasksByTitle([FromQuery] string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return BadRequest("O parâmetro 'title' é obrigatório.");
            }

            var normalizedTitle = title.Trim().ToLowerInvariant();
            var tasks = await _context.Tasks
                .AsNoTracking()
                .Where(t => EF.Functions.Like(t.Title, $"%{normalizedTitle}%"))
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<TaskItem>> GetTaskById(int id)
        {
            var task = await _context.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == id);
            if (task is null)
            {
                return NotFound();
            }

            return Ok(task);
        }

        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask([FromBody] CreateTaskRequest request)
        {
            if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow)
            {
                return BadRequest("DueDate não pode estar no passado.");
            }

            var task = new TaskItem
            {
                Title = request.Title.Trim(),
                Description = request.Description?.Trim(),
                Priority = request.Priority,
                DueDate = request.DueDate,
                CreatedAt = DateTime.UtcNow,
                IsCompleted = false
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTaskById), new { id = task.Id }, task);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateTask(int id, [FromBody] UpdateTaskRequest request)
        {
            if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow && !request.IsCompleted)
            {
                return BadRequest("DueDate não pode estar no passado para tarefas pendentes.");
            }

            var existingTask = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (existingTask is null)
            {
                return NotFound();
            }

            existingTask.Title = request.Title.Trim();
            existingTask.Description = request.Description?.Trim();
            existingTask.Priority = request.Priority;
            existingTask.DueDate = request.DueDate;
            existingTask.IsCompleted = request.IsCompleted;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var task = await _context.Tasks.FirstOrDefaultAsync(t => t.Id == id);
            if (task is null)
            {
                return NotFound();
            }

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}