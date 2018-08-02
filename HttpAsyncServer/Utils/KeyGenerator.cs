using System.Security.Cryptography;
using System.Text;

namespace HttpAsyncServer
{
    public class KeyGenerator
    {
        public static string GenerateHashId()
        {
            return GetUniqueKey(Consts.HASH_ID_LENGTH);
        }

        public static string GetUniqueKey(int maxSize)
        {
            char[] chars = new char[Consts.KEY_GENERATOR_CHARS.Length];
            chars = Consts.KEY_GENERATOR_CHARS.ToCharArray();
            byte[] data = new byte[1];

            using (RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider())
            {
                crypto.GetNonZeroBytes(data);
                data = new byte[maxSize];
                crypto.GetNonZeroBytes(data);
            }

            StringBuilder result = new StringBuilder(maxSize);

            foreach (byte b in data)
            {
                result.Append(chars[b % (chars.Length)]);
            }

            return result.ToString();
        }
    }
}