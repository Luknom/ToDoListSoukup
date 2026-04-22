using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TodoApp.Models;

public enum TaskPriority
{
    Low,
    Medium,
    High
}

public class TodoItem
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsCompleted { get; set; } = false;

    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDate { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Foreign Key for TodoList
    public int TodoListId { get; set; }

    [ForeignKey("TodoListId")]
    public virtual TodoList List { get; set; } = null!;

    // Navigation property for Tags: Many-to-Many
    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
