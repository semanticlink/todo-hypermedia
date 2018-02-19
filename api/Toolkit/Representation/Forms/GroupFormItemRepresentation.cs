using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "group-form-item")]
    public class GroupFormItemRepresentation : FormItemRepresentation
    {
        public GroupFormItemRepresentation()
        {
            Type = "http://types/group";
        }

        [DataMember(Name = "items", Order = 100)]
        public FormItemRepresentation[] Items { get; set; }
    }
}