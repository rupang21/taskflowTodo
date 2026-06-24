using System.ComponentModel.DataAnnotations;
using ToDoApp.Api.Models;

namespace ToDoApp.Api.DTOs
{
    /// <summary>
    /// DTO for creating a new todo item.
    /// Only exposes fields that a client should be able to set at creation time.
    /// </summary>
    public class CreateTodoDto
    {
        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
        public string Title { get; set; } = string.Empty;

        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string? Description { get; set; }

        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        [MaxLength(100, ErrorMessage = "Category cannot exceed 100 characters.")]
        public string? Category { get; set; }

        public DateTime? DueDate { get; set; }
    }
}
