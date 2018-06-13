using System;

namespace Domain.Models
{
    public class TodoCreateData
    {
        public string Name { get; set; }
        public string State{ get; set; }
        public DateTime Due { get; set; }
    }
}