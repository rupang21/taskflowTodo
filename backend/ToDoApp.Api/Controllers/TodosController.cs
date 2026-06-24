using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ToDoApp.Api.Data;
using ToDoApp.Api.DTOs;
using ToDoApp.Api.Models;

namespace ToDoApp.Api.Controllers
{
    /// <summary>
    /// RESTful API controller for managing to-do items.
    /// Provides full CRUD operations with filtering and summary statistics.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TodosController> _logger;

        public TodosController(AppDbContext context, ILogger<TodosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ──────────────────────────────────────
        //  GET /api/todos
        //  List all todos with optional filters
        // ──────────────────────────────────────
        /// <summary>
        /// Retrieves all to-do items. Supports optional filtering by status, priority, and category.
        /// </summary>
        /// <param name="status">Filter by status (0=Pending, 1=InProgress, 2=Completed, 3=Cancelled)</param>
        /// <param name="priority">Filter by priority (0=Low, 1=Medium, 2=High)</param>
        /// <param name="category">Filter by category name (exact match)</param>
        /// <param name="search">Search in title (partial match, case-insensitive)</param>
        /// <returns>List of TodoResponseDto</returns>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<TodoResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<TodoResponseDto>>> GetAll(
            [FromQuery] TodoStatus? status,
            [FromQuery] TodoPriority? priority,
            [FromQuery] string? category,
            [FromQuery] string? search)
        {
            var query = _context.TodoItems.AsQueryable();

            // Apply filters
            if (status.HasValue)
                query = query.Where(t => t.Status == status.Value);

            if (priority.HasValue)
                query = query.Where(t => t.Priority == priority.Value);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(t => t.Category == category);

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(t => t.Title.Contains(search));

            var items = await query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .ThenByDescending(t => t.CreatedAt)
                .ToListAsync();

            _logger.LogInformation("Retrieved {Count} todo items", items.Count);

            return Ok(items.Select(TodoResponseDto.FromEntity));
        }

        // ──────────────────────────────────────
        //  GET /api/todos/{id}
        //  Get a single todo by ID
        // ──────────────────────────────────────
        /// <summary>
        /// Retrieves a single to-do item by its ID.
        /// </summary>
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(TodoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TodoResponseDto>> GetById(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);

            if (item is null)
            {
                _logger.LogWarning("Todo item with ID {Id} not found", id);
                return NotFound(new { message = $"Todo item with ID {id} not found." });
            }

            return Ok(TodoResponseDto.FromEntity(item));
        }

        // ──────────────────────────────────────
        //  POST /api/todos
        //  Create a new todo
        // ──────────────────────────────────────
        /// <summary>
        /// Creates a new to-do item.
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(TodoResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TodoResponseDto>> Create([FromBody] CreateTodoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = new TodoItem
            {
                Title = dto.Title.Trim(),
                Description = dto.Description?.Trim(),
                Priority = dto.Priority,
                Category = dto.Category?.Trim(),
                DueDate = dto.DueDate,
                Status = TodoStatus.Pending,
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.TodoItems.Add(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created todo item with ID {Id}: {Title}", item.Id, item.Title);

            return CreatedAtAction(
                nameof(GetById),
                new { id = item.Id },
                TodoResponseDto.FromEntity(item));
        }

        // ──────────────────────────────────────
        //  PUT /api/todos/{id}
        //  Full update of a todo
        // ──────────────────────────────────────
        /// <summary>
        /// Updates all fields of an existing to-do item.
        /// </summary>
        [HttpPut("{id:int}")]
        [ProducesResponseType(typeof(TodoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<TodoResponseDto>> Update(int id, [FromBody] UpdateTodoDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var item = await _context.TodoItems.FindAsync(id);

            if (item is null)
            {
                _logger.LogWarning("Cannot update: Todo item with ID {Id} not found", id);
                return NotFound(new { message = $"Todo item with ID {id} not found." });
            }

            // Map DTO fields to entity
            item.Title = dto.Title.Trim();
            item.Description = dto.Description?.Trim();
            item.Status = dto.Status;
            item.IsCompleted = dto.Status == TodoStatus.Completed;
            item.Priority = dto.Priority;
            item.Category = dto.Category?.Trim();
            item.DueDate = dto.DueDate;
            item.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated todo item with ID {Id}", id);

            return Ok(TodoResponseDto.FromEntity(item));
        }

        // ──────────────────────────────────────
        //  PATCH /api/todos/{id}/toggle
        //  Toggle completion status
        // ──────────────────────────────────────
        /// <summary>
        /// Toggles the completion status of a to-do item.
        /// Pending/InProgress → Completed, Completed → Pending.
        /// </summary>
        [HttpPatch("{id:int}/toggle")]
        [ProducesResponseType(typeof(TodoResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<TodoResponseDto>> Toggle(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);

            if (item is null)
            {
                _logger.LogWarning("Cannot toggle: Todo item with ID {Id} not found", id);
                return NotFound(new { message = $"Todo item with ID {id} not found." });
            }

            // Toggle logic: if completed → pending, otherwise → completed
            if (item.Status == TodoStatus.Completed)
            {
                item.Status = TodoStatus.Pending;
                item.IsCompleted = false;
            }
            else
            {
                item.Status = TodoStatus.Completed;
                item.IsCompleted = true;
            }

            item.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Toggled todo item {Id} to {Status}", id, item.Status);

            return Ok(TodoResponseDto.FromEntity(item));
        }

        // ──────────────────────────────────────
        //  DELETE /api/todos/{id}
        //  Delete a todo
        // ──────────────────────────────────────
        /// <summary>
        /// Permanently deletes a to-do item.
        /// </summary>
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.TodoItems.FindAsync(id);

            if (item is null)
            {
                _logger.LogWarning("Cannot delete: Todo item with ID {Id} not found", id);
                return NotFound(new { message = $"Todo item with ID {id} not found." });
            }

            _context.TodoItems.Remove(item);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted todo item with ID {Id}", id);

            return NoContent();
        }

        // ──────────────────────────────────────
        //  GET /api/todos/summary
        //  Dashboard statistics
        // ──────────────────────────────────────
        /// <summary>
        /// Returns aggregated summary statistics for all to-do items.
        /// </summary>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(TodoSummaryDto), StatusCodes.Status200OK)]
        public async Task<ActionResult<TodoSummaryDto>> GetSummary()
        {
            var items = await _context.TodoItems.ToListAsync();
            var now = DateTime.UtcNow;

            var summary = new TodoSummaryDto
            {
                TotalTasks = items.Count,
                Pending = items.Count(t => t.Status == TodoStatus.Pending),
                InProgress = items.Count(t => t.Status == TodoStatus.InProgress),
                Completed = items.Count(t => t.Status == TodoStatus.Completed),
                Cancelled = items.Count(t => t.Status == TodoStatus.Cancelled),
                Overdue = items.Count(t => t.DueDate.HasValue
                                          && t.DueDate.Value < now
                                          && t.Status != TodoStatus.Completed
                                          && t.Status != TodoStatus.Cancelled)
            };

            return Ok(summary);
        }
    }
}
