namespace TALib
{
    public static partial class Core
    {
        /// <summary>
        /// Represents the return codes for functions, indicating the outcome of an operation.
        /// </summary>
        public enum RetCode : ushort
        {
            Success,
            BadParam,
            OutOfRangeParam
        }
    }

}
