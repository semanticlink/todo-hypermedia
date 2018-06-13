using Domain.Models;

namespace Domain.Representation.Enum
{
    /// <summary>
    ///     The options available for a <see cref="TodoRepresentation"/> and <seealso cref="Todo"/> that can be
    ///      displaye in forms.
    /// </summary>
    /// <remarks>
    ///    <para>
    ///         This is an example of a reference data type. Put differently, if you want a dynamic type then you'll be
    ///         looking at creating collection.
    ///     </para>
    ///     <para>
    ///         Obviously, once data goes into persistence, you'll need to not change these. But, that is what you
    ///         have to do with language construct enums too :-)
    ///     </para>
    /// </remarks>
    public static class TodoState
    {
        /// <summary>
        ///     Starting state of a todo that can be edited
        /// </summary>
        public const string Open = "http://example.com/todo/state/open";

        /// <summary>
        ///     Finished state of a todo.
        /// </summary>
        public const string Complete = "http://example.com/todo/state/complete";
    }
}