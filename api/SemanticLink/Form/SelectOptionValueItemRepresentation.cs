using System.Runtime.Serialization;

namespace SemanticLink.Form
{
    /// <summary>
    ///     A value for a selection (menu)
    /// </summary>
    [DataContract(Name = "select-option-form-item")]
    public class SelectOptionValueItemRepresentation : SelectOptionItemRepresentation
    {
        [DataMember(Name = "value", Order = 30, EmitDefaultValue = false)]
        public string Value { get; set; }

        [DataMember(Name = "label", Order = 30, EmitDefaultValue = false)]
        public string Label { get; set; }
    }
}