using System;
using System.ComponentModel.DataAnnotations;

namespace ToDoApp.Api.Models
{
    public class User
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;
        
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        public string GoogleSubjectId { get; set; } = string.Empty;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
