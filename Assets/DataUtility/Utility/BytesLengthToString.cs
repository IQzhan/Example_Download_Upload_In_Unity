namespace E.Data
{
    public partial class Utility
    {
        private static readonly string[] units = {
                "B",
                "KB",
                "MB",
                "GB",
                "TB",
                "EB",
                "ZB" };

        private static readonly ulong[] lengths = {
                1,
                1024,
                1048576,
                1073741824,
                1099511627776,
                1125899906842624,
                1152921504606846976 };

        /// <summary>
        /// convert bytes length to string. 
        /// such as 1572864 -> "1.5mb"
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string BytesLengthToString(long length, string pattern = null)
        {
            if (length < 0) throw new System.ArgumentException("cannot be less than 0.", "length");
            return BytesLengthToString((ulong)length, pattern);
        }

        /// <summary>
        /// convert bytes length to string. 
        /// such as 1572864 -> "1.5mb"
        /// </summary>
        /// <param name="length"></param>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string BytesLengthToString(ulong length, string pattern = null)
        {
            int mark = 0;
            for (int i = 0; i < lengths.Length; i++)
            { if (length > lengths[i]) { mark = i; } else break; }
            if (pattern == null) pattern = "<n> <u>";
            return pattern
            .Replace("<n>", ((double)length / lengths[mark]).ToString())
            .Replace("<U>", units[mark])
            .Replace("<u>", units[mark].ToLower());
        }
    }
}
