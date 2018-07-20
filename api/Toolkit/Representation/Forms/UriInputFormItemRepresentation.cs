using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
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