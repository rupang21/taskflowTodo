using ToDoApp.Api.Models;

namespace ToDoApp.Api.DTOs
{
    /// <summary>
    /// DTO for API responses. Includes all entity fields plus
    /// human-readable labels for enum values.
    /// </summary>
    public class TodoResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TodoStatus Status { get; set; }
        public string StatusLabel { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public TodoPriority Priority { get; set; }
        public string PriorityLabel { get; set; } = string.Empty;
        public string? Category { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        /// <summary>
        /// Maps a domain entity to a response DTO.
        /// </summary>
        public static TodoResponseDto FromEntity(TodoItem item)
        {
            return new TodoResponseDto
            {
                Id = item.Id,
                Title = item.Title,
                Description = item.Description,
                Status = item.Status,
                StatusLabel = item.Status.ToString(),
                IsCompleted = item.IsCompleted,
                Priority = item.Priority,
                PriorityLabel = item.Priority.ToString(),
                Category = item.Category,
                DueDate = item.DueDate,
                CreatedAt = item.CreatedAt,
                UpdatedAt = item.UpdatedAt
            };
        }
    }

    /// <summary>
    /// DTO for the dashboard summary endpoint.
    /// </summary>
    public class TodoSummaryDto
    {
        public int TotalTasks { get; set; }
        public int Pending { get; set; }
        public int InProgress { get; set; }
        public int Completed { get; set; }
        public int Cancelled { get; set; }
        public int Overdue { get; set; }
    }
}
