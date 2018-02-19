using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "collection-input-form-item")]
    public class CollectionInputFormItemRepresentation : FormItemRepresentation
    {
        public CollectionInputFormItemRepresentation()
        {
            Type = "http://types/collection";
        }
    }
}