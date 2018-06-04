namespace Toolkit.Representation.Forms
{
    /// <summary>
    /// <para>
    ///    Values in representations need to be created and updated via form types. These change depending
    ///     on the client.
    /// </para>
    /// <para>
    ///    Also, not all types are rendered natively on browsers eg <see cref="Group"/> <see cref="Collection"/>
    /// </para>
    /// <remarks>
    ///    Browsers tend now to implement more types eg see https://bootstrap-vue.js.org/docs/components/form-input
    ///      types: [ 'text', 'password', 'email', 'number', 'url', 'tel', 'date', `time`, 'range', 'color' ]
    /// 
    ///    Caveats with input types:
    ///         - Not all browsers support all input types, nor do some types render in the same format across browser types/version.
    ///         - Browsers that do not support a particular type will fall back to a text input type. As an example, Firefox desktop doesn't support date, datetime, or time, while Firefox mobile does.
    ///         - Chrome lost support for datetime in version 26, Opera in version 15, and Safari in iOS 7. Instead of using datetime, since support should be deprecated, use date and time as two separate input types.
    ///         - For date and time style input, where supported, the displayed value in the GUI may be different than what is returned by its value.
    ///         - Regardless of input type, the value is always returned as a string representation.
    /// </remarks
    /// </summary>
    public static class FormType
    {
        public const string Text = "http://types/text";
        public const string Password = "http://types/text/password";
        public const string Email = "http://types/text/email";
        
        /// <summary>
        ///     Mutually exlcusive selection
        /// </summary>
        public const string Select = "http://types/select";
        
        /// <summary>
        ///     One-or-more selection
        /// </summary>
        public const string Check = "http://types/check";
        
        public const string Date = "http://types/date";
        public const string DateTime = "http://types/date/time";
        
        public const string Group = "http://types/group";
        public const string Collection = "http://types/collection";
    }
}