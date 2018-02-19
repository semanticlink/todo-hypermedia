using System.Runtime.Serialization;

namespace TodoApi.Representation.Forms
{
    /// <seealso cref = "EditFormRepresentation" />
    /// <seealso cref = "CreateFormRepresentation" />
    [DataContract(Name = "search-form")]
    public class SearchFormRepresentation : FormRepresentation
    {
    }
}