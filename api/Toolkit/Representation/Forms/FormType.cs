namespace Toolkit.Representation.Forms
{
    /// <summary>
    /// <para>
    ///    Values in representations need to be created and updated via form types. These change depending
    ///     on the client.
    /// </para>
    /// <para>
    ///    Also, not all types are rendered natively on browsers eg <see cref="Group"/>, <see cref="Collection"/>, <see cref="Enum"/>
    /// </para>
    /// <remarks>
    /// <para>
    ///    Browsers tend now to implement more types eg see https://bootstrap-vue.js.org/docs/components/form-input
    ///      types: [ 'text', 'password', 'email', 'number', 'url', 'tel', 'date', `time`, 'range', 'color' ]
    /// </para>
    /// 
    /// <para>
    ///     Caveats with input types:
    /// </para>
    /// <list type="bullet">
    ///   <item><description>Not all browsers support all input types, nor do some types render in the same format across browser types/version.</description></item>
    ///   <item><description>Browsers that do not support a particular type will fall back to a text input type. As an example, Firefox desktop doesn't support date, datetime, or time, while Firefox mobile does.</description></item>
    ///   <item><description>Chrome lost support for datetime in version 26, Opera in version 15, and Safari in iOS 7. Instead of using datetime, since support should be deprecated, use date and time as two separate input types.</description></item>
    ///   <item><description>For date and time style input, where supported, the displayed value in the GUI may be different than what is returned by its value.</description></item>
    ///   <item><description>Regardless of input type, the value is always returned as a string representation.</description></item>
    /// </list>
    ///
    /// </remarks>
    /// </summary>
    public static class FormType
    {
        ///////////////////////////
        // 
        // Input form types 
        //
        public const string Text = "http://types/text";
        public const string Password = "http://types/text/password";
        public const string Email = "http://types/text/email";
        public const string Tel = "http://types/text/tel";

        public const string Uri = "http://types/text/uri";

        public const string Date = "http://types/date";
        public const string DateTime = "http://types/datetime";
        public const string Time = "http://types/time";

        public const string Number = "http://types/number";
        public const string Range = "http://types/range";
        public const string Color = "http://types/color";
        
        /// <summary>
        ///   JSON Pointer defines a string syntax for identifying a specific value
        ///   within a JavaScript Object Notation (JSON) document. see https://tools.ietf.org/html/rfc6901
        /// </summary>
        /// <remarks>
        ///    Used in conjunctin with <see cref="MediaType.JsonPatch"/>
        /// </remarks>
        public const string JsonPointer = "http://types/jsonpointer";

        ///////////////////////////
        // 
        // Selection form types 
        //

        /// <summary>
        ///     Mutually exclusive selection
        /// </summary>
        public const string Select = "http://types/select";

        /// <summary>
        ///     One-or-more selection
        /// </summary>
        public const string Check = "http://types/check";

        ///////////////////////////
        // 
        // Groupings
        //

        public const string Group = "http://types/group";

        /// <summary>
        ///     Some values are part of a dynamic set of values (collection) eg in a select. 
        /// </summary>
        /// <remarks>
        ///    A collection is a reference type compared with the value type of <see cref="Enum"/>
        /// </remarks>
        public const string Collection = "http://types/collection";

        /// <summary>
        /// <para>
        ///     Some values are part of a static set of values (enumeration) eg in a select.
        /// </para>
        /// <example>
        ///
        ///   Used as part of <see cref="Select"/>. In this case, between two options: open, complete
        ///
        /// <code>
        ///   new SelectFormItemRepresentation
        ///        {
        ///            Name = "state",
        ///            Description = "A todo can only toggle between open and complete.",
        ///            Required = false,
        ///            Multiple = false,
        ///            Items = new SelectOptionItemRepresentation[]
        ///            {
        ///                new SelectOptionValueItemRepresentation
        ///                {
        ///                    Type = FormType.Enum,
        ///                    Description = "The todo has been completed",
        ///                    Label = "Completed",
        ///                    Value = TodoState.Complete,
        ///                    Name = "completed",
        ///                },
        ///                new SelectOptionValueItemRepresentation
        ///                {
        ///                    Type = FormType.Enum,
        ///                    Description = "The todo has been opened",
        ///                    Label = "Open",
        ///                    Value = TodoState.Open,
        ///                    Name = "open",
        ///                },
        ///            }
        ///        },
        /// </code>
        /// <para>
        /// We can also represent media types using enums (eg the 'op' in <see cref="MediaType.JsonPatch"/>).
        /// </para>
        /// </example>
        /// </summary>
        /// <remarks>
        ///    An enum is a value type compared with the reference type of <see cref="Collection"/>
        /// </remarks>
        public const string Enum = "http://types/enum";
    }
}