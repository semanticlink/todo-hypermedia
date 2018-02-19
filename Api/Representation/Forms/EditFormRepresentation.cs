using System.Runtime.Serialization;

namespace TodoApi.Representation.Forms
{
    /// <seealso cref = "CreateFormRepresentation" />
    [DataContract(Name = "edit-form")]
    public class EditFormRepresentation : FormRepresentation
    {
    }
}