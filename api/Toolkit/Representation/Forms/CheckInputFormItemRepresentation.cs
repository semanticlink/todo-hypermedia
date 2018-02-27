using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "text-input-form-item")]
    public class CheckInputFormItemRepresentation : FormItemRepresentation
    {
        public CheckInputFormItemRepresentation()
        {
            Type = "http://types/text";
        }

    }
}