using System.Runtime.Serialization;

namespace TodoApi.Representation.Forms
{
    [DataContract(Name = "date-input-form-item")]
    public class DateTimeInputFormItemRepresentation : FormItemRepresentation
    {
        public DateTimeInputFormItemRepresentation()
        {
            Type = "http://types/date-time";
        }

    
    }
}