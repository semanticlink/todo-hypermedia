using System.Collections.Generic;

namespace Domain.Models
{
    public class InheritForm
    {
        /// <summary>
        ///     The specific right type for the resource. Note: there can be multiple of these per resourceId
        /// </summary>
        public RightType Type { get; set; }

        /// <summary>
        ///     The id (ie database) of resource (eg tenant, user, todo, tag, comment
        /// </summary>
        public string ResourceId { get; set; }

        /// <summary>
        ///     The  inherit type on the resource to apply to this resource type
        /// </summary>
        /// <example>
        ///    InheritTypes = new []
        ///     {
        ///         RightType.Todo
        ///         RightType.Comment
        ///     }
        /// </example>
        public IEnumerable<RightType> InheritedTypes { get; set; }

        /// <summary>
        ///     The inherit rights to copy from the resource to the new resource
        /// </summary>
        public IEnumerable<RightType> CopyInheritTypes { get; set; }
    }
}