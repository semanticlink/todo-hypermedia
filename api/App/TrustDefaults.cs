namespace App
{
    public class TrustDefaults
    {
        /// <summary>
        ///     Known stable name of the trusted service account. Any code that from the root of trust must run
        ///     user this account name
        /// </summary>
        public const string KnownRootIdentifier = "service|root01";

        /// <summary>
        ///     A virtual resource identifier that is the root resource that everything inherits from
        /// </summary>
        public const string KnownHomeResourceId = "000000000000000";
    }
}