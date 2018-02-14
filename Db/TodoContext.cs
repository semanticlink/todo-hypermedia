using Microsoft.EntityFrameworkCore;
using TodoApi.Models;

namespace TodoApi.Db
{
    public class TodoContext : DbContext
    {
        public TodoContext(DbContextOptions<TodoContext> options)
            : base(options)
        {
        }

        public DbSet<Todo> TodoItems { get; set; }

    }
}
