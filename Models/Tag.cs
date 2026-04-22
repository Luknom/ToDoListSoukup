using System.ComponentModel.DataAnnotations;

namespace TodoApp.Models;

public class Tag
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = string.Empty;

    // Navigation property: Many-to-Many
    public virtual ICollection<TodoItem> Items { get; set; } = new List<TodoItem>();

    public override string ToString() => Name;
}
