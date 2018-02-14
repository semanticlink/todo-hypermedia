using System;

namespace TodoApi.Models
{
    public class Todo
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public bool Completed { get; set; }
        public DateTime Due { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}