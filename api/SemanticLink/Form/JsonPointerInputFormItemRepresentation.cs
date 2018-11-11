using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "text-json-pointer-input-form-item")]
    public class JsonPointerInputFormItemRepresentation : FormItemRepresentation
    {
        public JsonPointerInputFormItemRepresentation()
        {
            Type = FormType.JsonPointer;
        }
    }
}