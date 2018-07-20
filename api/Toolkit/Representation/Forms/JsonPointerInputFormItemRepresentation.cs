using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
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