using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;
using TodoApi.Models;

namespace TodoApi.Models
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<TodoItem> TodoItems { get; set; } = null!;

        public DbSet<TodoApi.Models.Hero>? Hero { get; set; } = null!;

        public DbSet<TodoApi.Models.Employee>? Employee { get; set; } = null!;
    }
}