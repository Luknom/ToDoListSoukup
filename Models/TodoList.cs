using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class TodoList
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public virtual ICollection<TodoItem> Items { get; set; } = new List<TodoItem>();

    public override string ToString() => Name;
}
