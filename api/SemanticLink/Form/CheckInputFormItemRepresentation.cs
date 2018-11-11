using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "text-input-form-item")]
    public class CheckInputFormItemRepresentation : FormItemRepresentation
    {
        public CheckInputFormItemRepresentation()
        {
            Type = FormType.Check;
        }

    }
}