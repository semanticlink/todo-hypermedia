using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "text-password-input-form-item")]
    public class PasswordInputFormItemRepresentation : FormItemRepresentation
    {
        public PasswordInputFormItemRepresentation()
        {
            Type = FormType.Password;
        }
    }
}