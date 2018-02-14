using System;

namespace TodoApi.Models
{
    public class Tenant
    {
        public long Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdateAt { get; set; }
    }
}