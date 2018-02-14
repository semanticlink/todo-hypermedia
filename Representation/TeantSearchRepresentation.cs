﻿using System.Runtime.Serialization;

namespace TodoApi.Representation
{
    [DataContract(Name = "tenant-search")]
    public class TeantSearchRepresentation
    {
        [DataMember(Name = "search", Order = 20)]
        public string Search { get; set; }
    }
}