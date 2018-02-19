using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Db
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> TodoItems { get; set; }
        public DbSet<Tenant> Tenants { get; set; }

    }
}
