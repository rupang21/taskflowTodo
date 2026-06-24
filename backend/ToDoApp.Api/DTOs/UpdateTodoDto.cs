using System.ComponentModel.DataAnnotations;
using ToDoApp.Api.Models;

namespace ToDoApp.Api.DTOs
{
    /// <summary>
    /// DTO for updating an existing todo item.
    /// Includes status and completion fields that are not available at creation.
    /// </summary>
    public class UpdateTodoDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public TodoStatus Status { get; set; } = TodoStatus.Pending;

        public bool IsCompleted { get; set; } = false;

        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string? Category { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
