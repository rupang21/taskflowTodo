using Microsoft.EntityFrameworkCore;
using ToDoApp.Api.Models;

namespace ToDoApp.Api.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems => Set<TodoItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ───── TodoItem Table Configuration ─────
            modelBuilder.Entity<TodoItem>(entity =>
            {
                // Table name
                entity.ToTable("TodoItems");

                // Primary Key
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Id)
                      .ValueGeneratedOnAdd();

                // Title — required, max 200 chars
                entity.Property(e => e.Title)
                      .IsRequired()
                      .HasMaxLength(200)
                      .HasColumnType("TEXT");

                // Description — optional, max 1000 chars
                entity.Property(e => e.Description)
                      .HasMaxLength(1000)
                      .HasColumnType("TEXT");

                // Status — stored as integer, default Pending(0)
                entity.Property(e => e.Status)
                      .HasDefaultValue(TodoStatus.Pending)
                      .HasConversion<int>();

                // IsCompleted — default false
                entity.Property(e => e.IsCompleted)
                      .HasDefaultValue(false);

                // Priority — stored as integer, default Medium(1)
                entity.Property(e => e.Priority)
                      .HasDefaultValue(TodoPriority.Medium)
                      .HasConversion<int>();

                // Category — optional, max 100 chars
                entity.Property(e => e.Category)
                      .HasMaxLength(100)
                      .HasColumnType("TEXT");

                // DueDate — optional
                entity.Property(e => e.DueDate)
                      .HasColumnType("TEXT"); // SQLite stores DateTime as TEXT

                // CreatedAt — default to current UTC time
                entity.Property(e => e.CreatedAt)
                      .HasColumnType("TEXT")
                      .HasDefaultValueSql("datetime('now')");

                // UpdatedAt — default to current UTC time
                entity.Property(e => e.UpdatedAt)
                      .HasColumnType("TEXT")
                      .HasDefaultValueSql("datetime('now')");

                // ───── Indexes ─────
                // Composite index for common query: filter by status + priority
                entity.HasIndex(e => new { e.Status, e.Priority })
                      .HasDatabaseName("IX_TodoItems_Status_Priority");

                // Index for filtering/sorting by due date
                entity.HasIndex(e => e.DueDate)
                      .HasDatabaseName("IX_TodoItems_DueDate");

                // Index for category filtering
                entity.HasIndex(e => e.Category)
                      .HasDatabaseName("IX_TodoItems_Category");

                // Index for creation time ordering
                entity.HasIndex(e => e.CreatedAt)
                      .HasDatabaseName("IX_TodoItems_CreatedAt");

                // Legacy index for IsCompleted filtering
                entity.HasIndex(e => e.IsCompleted)
                      .HasDatabaseName("IX_TodoItems_IsCompleted");

                // ───── Seed Data ─────
                entity.HasData(
                    new TodoItem
                    {
                        Id = 1,
                        Title = "Complete initial project setup",
                        Description = "Set up backend .NET 9 + frontend React/Vite project structure",
                        Status = TodoStatus.Completed,
                        IsCompleted = true,
                        Priority = TodoPriority.High,
                        Category = "Development",
                        CreatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc),
                        UpdatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new TodoItem
                    {
                        Id = 2,
                        Title = "Implement CRUD API controller",
                        Description = "Build Create, Read, Update, Delete, and Toggle endpoints in TodosController",
                        Status = TodoStatus.InProgress,
                        IsCompleted = false,
                        Priority = TodoPriority.High,
                        Category = "Development",
                        CreatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc),
                        UpdatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new TodoItem
                    {
                        Id = 3,
                        Title = "Design frontend UI",
                        Description = "Build todo list and input form UI using React components",
                        Status = TodoStatus.Pending,
                        IsCompleted = false,
                        Priority = TodoPriority.Medium,
                        Category = "Design",
                        DueDate = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc),
                        CreatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc),
                        UpdatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc)
                    },
                    new TodoItem
                    {
                        Id = 4,
                        Title = "Write unit tests",
                        Description = "Create xUnit tests for API endpoints and business logic",
                        Status = TodoStatus.Pending,
                        IsCompleted = false,
                        Priority = TodoPriority.Low,
                        Category = "Testing",
                        DueDate = new DateTime(2026, 7, 15, 0, 0, 0, DateTimeKind.Utc),
                        CreatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc),
                        UpdatedAt = new DateTime(2026, 6, 21, 0, 0, 0, DateTimeKind.Utc)
                    }
                );
            });
        }
    }
}

