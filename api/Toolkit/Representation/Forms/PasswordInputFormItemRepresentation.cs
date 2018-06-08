using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
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