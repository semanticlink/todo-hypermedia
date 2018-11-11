using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "text-uri-input-form-item")]
    public class UriInputFormItemRepresentation : FormItemRepresentation
    {
        public UriInputFormItemRepresentation()
        {
            Type = FormType.Uri;
        }
    }
}