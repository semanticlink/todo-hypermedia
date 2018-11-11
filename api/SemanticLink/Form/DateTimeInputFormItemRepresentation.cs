using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    [DataContract(Name = "date-input-form-item")]
    public class DateTimeInputFormItemRepresentation : FormItemRepresentation
    {
        public DateTimeInputFormItemRepresentation()
        {
            Type = FormType.DateTime;
        }

    
    }
}