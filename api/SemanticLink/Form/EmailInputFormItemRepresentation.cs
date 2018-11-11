using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "text-email-input-form-item")]
    public class EmailInputFormItemRepresentation : FormItemRepresentation
    {
        public EmailInputFormItemRepresentation()
        {
            Type = FormType.Email;
        }

    }
}