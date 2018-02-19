using System.Runtime.Serialization;

namespace TodoApi.Representation.Forms
{
    [DataContract(Name = "text-input-form-item")]
    public class CheckInputFormItemRepresentation : FormItemRepresentation
    {
        public CheckInputFormItemRepresentation()
        {
            Type = "http://types/check";
        }

    }
}