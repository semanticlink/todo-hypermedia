namespace Domain
{
    public interface IIdGenerator
    {
        /// <summary>
        ///     Generates a string-based identifier for use as databases keys (eg primary hashkey) 
        /// </summary>
        /// <returns>unique string</returns>
        string New();
    }
}