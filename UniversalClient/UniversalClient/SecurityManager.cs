using System.Security.Cryptography;
using System.Text;

namespace Moonbyte.Net.Crypto
{
    public class securityManager
    {

        #region Encrypt

        public string CalculateSHA256Hash(string str)
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] hashValue;
            UTF8Encoding objUtf8 = new UTF8Encoding();
            hashValue = sha256.ComputeHash(objUtf8.GetBytes(str));

            return Encoding.UTF8.GetString(hashValue);
        }

        #endregion Encrypt

    }
}
