using System.Runtime.Serialization;

namespace Toolkit.Representation.LinkedRepresentation
{
    // cref="http://en.wikipedia.org/wiki/Atom_(standard)#Example_of_an_Atom_1.0_feed"
    [DataContract(Name = "feed")]
    public class FeedRepresentation : LinkedRepresentation
    {
        [DataMember(Name = "items", Order = 20)]
        public FeedItemRepresentation[] Items { get; set; }
    }
}