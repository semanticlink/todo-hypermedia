using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "date-input-form-item")]
    public class DateInputFormItemRepresentation : FormItemRepresentation
    {
        public DateInputFormItemRepresentation()
        {
            Type = FormType.Date;
        }
    }
}