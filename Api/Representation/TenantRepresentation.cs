using System;

namespace TodoApi.Representation
{
    public class TenantRepresentation : LinkedRepresentation.LinkedRepresentation
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}