using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "collection-input-form-item")]
    public class CollectionInputFormItemRepresentation : FormItemRepresentation
    {
        public CollectionInputFormItemRepresentation()
        {
            Type = FormType.Collection;
        }
    }
}