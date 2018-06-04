using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    // TODO: a length and/or a regex might be other information you may want to provide to help the client fill in the form.
    [DataContract(Name = "text-input-form-item")]
    public class TextInputFormItemRepresentation : FormItemRepresentation
    {
        public TextInputFormItemRepresentation()
        {
            Type = FormType.Text;
        }

    }
}