using Microsoft.EntityFrameworkCore;
using TodoApp.Models;

namespace TodoApp.Data;

public class AppDbContext : DbContext
{
    public DbSet<TodoItem> TodoItems { get; set; } = null!;
    public DbSet<TodoList> TodoLists { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string dbPath = Path.Combine(baseDir, "TodoDatabase.db");
        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoList>()
            .HasMany(l => l.Items)
            .WithOne(i => i.List)
            .HasForeignKey(i => i.TodoListId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TodoItem>()
            .HasMany(i => i.Tags)
            .WithMany(t => t.Items);
            
        base.OnModelCreating(modelBuilder);
    }
}
