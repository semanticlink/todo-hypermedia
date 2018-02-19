using System.Runtime.Serialization;

namespace TodoApi.Representation.Forms
{
    /// <seealso cref = "EditFormRepresentation" />
    /// <seealso cref = "SearchFormRepresentation" />
    [DataContract(Name = "create-form")]
    public class CreateFormRepresentation : FormRepresentation
    {
    }
}