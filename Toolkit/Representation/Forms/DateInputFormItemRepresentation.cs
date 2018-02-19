using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "date-input-form-item")]
    public class DateInputFormItemRepresentation : FormItemRepresentation
    {
        public DateInputFormItemRepresentation()
        {
            Type = "http://types/date";
        }
    }
}