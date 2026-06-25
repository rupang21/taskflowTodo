using System;
using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Api.Models
{
    /// <summary>
    /// Represents the priority level of a to-do item.
    /// Stored as INTEGER in SQLite (0=Low, 1=Medium, 2=High).
    /// </summary>
    public enum TodoPriority
    {
        Low = 0,
        Medium = 1,
        High = 2
    }

    /// <summary>
    /// Represents the status of a to-do item.
    /// Stored as INTEGER in SQLite (0=Pending, 1=InProgress, 2=Completed, 3=Cancelled).
    /// </summary>
    public enum TodoStatus
    {
        Pending = 0,
        InProgress = 1,
        Completed = 2,
        Cancelled = 3
    }

    /// <summary>
    /// Core domain entity for a to-do task.
    /// Maps to the "TodoItems" table in the SQLite database.
    /// </summary>
    public class TodoItem
    {
        /// <summary>Primary key. Auto-incremented INTEGER.</summary>
        public int Id { get; set; }

        /// <summary>Task title. Required, max 200 characters.</summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>Optional detailed description. Max 1000 characters.</summary>
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>Current status of the task. Defaults to Pending.</summary>
        public TodoStatus Status { get; set; } = TodoStatus.Pending;

        /// <summary>Legacy convenience flag. True when Status == Completed.</summary>
        public bool IsCompleted { get; set; } = false;

        /// <summary>Priority level (Low/Medium/High). Defaults to Medium.</summary>
        public TodoPriority Priority { get; set; } = TodoPriority.Medium;

        /// <summary>Optional category/tag for grouping tasks.</summary>
        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>Optional due date for the task.</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>Timestamp when the item was created (UTC).</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Timestamp when the item was last updated (UTC).</summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Foreign key to the User who owns this task.</summary>
        [Required]
        public string UserId { get; set; } = string.Empty;

        /// <summary>Navigation property to the User.</summary>
        public User? User { get; set; }
    }
}
