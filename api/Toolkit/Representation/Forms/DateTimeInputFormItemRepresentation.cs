using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
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