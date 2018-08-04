using System;
using System.Collections.Generic;

namespace Domain.Models
{
    public class TodoCreateData
    {
        public string Tenant { get; set; }
        public string Name { get; set; }
        public string State{ get; set; }
        public List<string> Tags{ get; set; }
        public DateTime Due { get; set; }
    }
}