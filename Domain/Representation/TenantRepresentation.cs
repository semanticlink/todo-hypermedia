using System;
using Toolkit.Representation.LinkedRepresentation;

namespace Domain.Representation
{
    public class TenantRepresentation : LinkedRepresentation
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}