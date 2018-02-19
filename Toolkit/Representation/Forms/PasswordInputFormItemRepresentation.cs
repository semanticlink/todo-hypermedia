using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "text-password-input-form-item")]
    public abstract class PasswordInputFormItemRepresentation : FormItemRepresentation
    {
        public PasswordInputFormItemRepresentation()
        {
            Type = "http://types/text/password";
        }
    }
}