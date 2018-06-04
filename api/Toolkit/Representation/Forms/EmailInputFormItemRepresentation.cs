using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
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