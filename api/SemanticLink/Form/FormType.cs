namespace SemanticLink.Form
{
    /// <summary>
    /// <para>
    ///    Values in representations need to be created and updated via form types. These change depending
    ///     on the client. Each type relates to the input element represents a typed data field, usually with
    ///     a form control to allow the user to edit the data.
    /// </para>
    /// <para>
    ///    Also, not all types are rendered natively on browsers eg <see cref="Group"/>, <see cref="Collection"/>, <see cref="Enum"/>
    /// </para>
    /// <remarks>
    /// <para>
    ///    Browsers tend now to implement more types eg see https://bootstrap-vue.js.org/docs/components/form-input
    ///      types: [ 'text', 'password', 'email', 'number', 'url', 'tel', 'date', `time`, 'range', 'color' ]
    /// </para>
    /// <para>
    ///     see html spec https://www.w3.org/TR/html5/single-page.html#the-input-element
    /// </para>
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
        // ================
        //
        //  see https://www.w3.org/TR/html5/single-page.html#the-input-element
        //
        
        /// <summary>
        ///     Text with no line breaks
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#text-typetext-state-and-search-state-typesearch
        /// </remarks>
        public const string Text = "http://types/text";
        /// <summary>
        ///     Text with no line breaks (sensitive information)
        /// </summary>
        /// <remarks>
        ///    see html spec 
        /// </remarks>
        public const string Password = "http://types/text/password";
        /// <summary>
        ///     An e-mail address or list of e-mail addresses
        /// </summary>
        /// <remarks>
        ///    see html spec 
        /// </remarks>
        public const string Email = "http://types/text/email";
        /// <summary>
        ///     Text with no line breaks
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-telephone
        /// </remarks>
        public const string Tel = "http://types/text/tel";

        /// <summary>
        ///     An absolute URL
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-url
        /// </remarks>
        public const string Uri = "http://types/text/uri";

        /// <summary>
        ///     A date and time (year, month, day) with timezone offset
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-date 
        /// </remarks>
        public const string Date = "http://types/date";
        /// <summary>
        ///     A date and time (year, month, day, hour, minute, second, fraction of a second) with timezone offset
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-localdatetime
        /// </remarks>
        public const string DateTime = "http://types/datetime";
        /// <summary>
        ///     A time (hour, minute, seconds, fractional seconds) with time zone
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-time
        /// </remarks>
        public const string Time = "http://types/time";

        /// <summary>
        ///     A numerical value
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-number
        /// </remarks>
        public const string Number = "http://types/number";
        /// <summary>
        ///     A numerical value, with the extra semantic that the exact value is not important
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-range 
        /// </remarks>
        public const string Range = "http://types/range";
        /// <summary>
        ///     An sRGB color with 8-bit red, green, and blue components
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-number 
        /// </remarks>
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
        ///     One-or-more selection froma predefined list
        /// </summary>
        /// <remarks>
        ///    see https://www.w3.org/TR/html5/single-page.html#element-statedef-input-checkbox
        /// </remarks>
        public const string Check = "http://types/check";

        ///////////////////////////
        // 
        // Groupings
        //

        /// <summary>
        ///     
        /// </summary>
        /// <remarks>
        ///    Similar to an html fieldset
        /// </remarks>
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