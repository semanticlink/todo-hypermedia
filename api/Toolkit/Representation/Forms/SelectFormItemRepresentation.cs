using System.Runtime.Serialization;

namespace Toolkit.Representation.Forms
{
    [DataContract(Name = "select-form-item")]
    public class SelectFormItemRepresentation : FormItemRepresentation
    {
        public SelectFormItemRepresentation()
        {
            Type = FormType.Select;
        }

        /// <summary>
        ///     A collection of groups or values.
        /// </summary>
        [DataMember(Name = "items", Order = 100)]
        public SelectOptionItemRepresentation[] Items { get; set; }
    }
}