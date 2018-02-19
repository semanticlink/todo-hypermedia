using System;

namespace TodoApi.Models
{
    public class TodoCreateData
    {
        public string Name { get; set; }
        public bool Completed { get; set; }
        public DateTime Due { get; set; }
    }
}