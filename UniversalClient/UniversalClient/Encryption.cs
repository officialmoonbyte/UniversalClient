using System;
using System.Collections;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IndieGoat.Cryptography.UniversalTCPEncryption
{
    public class Encryption
    {

        public RSAParameters ServerPublicKey;
        public RSAParameters ServerPrivateKey;

        public RSAParameters ClientPublicKey;
        public RSAParameters ClientPrivateKey;

        public Encryption(bool IsServer = false)
        {
            RSACryptoServiceProvider EncryptionServiceProvider = new RSACryptoServiceProvider(1024);

            if (IsServer == true)
            {
                ServerPublicKey = EncryptionServiceProvider.ExportParameters(false);
                ServerPrivateKey = EncryptionServiceProvider.ExportParameters(true);
            }
            else
            {
                ClientPublicKey = EncryptionServiceProvider.ExportParameters(false);
                ClientPrivateKey = EncryptionServiceProvider.ExportParameters(true);
            }
        }

        public string Encrypt(string inputString, string key)
        {
            try
            {
                int dwKeySize = 1024;

                // TODO: Add Proper Exception Handlers
                RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
                rsaCryptoServiceProvider.ImportParameters(EncryptKey(key));
                int keySize = dwKeySize / 8;
                byte[] bytes = Encoding.UTF32.GetBytes(inputString);
                // The hash function in use by the .NET RSACryptoServiceProvider here 
                // is SHA1
                // int maxLength = ( keySize ) - 2 - 
                //              ( 2 * SHA1.Create().ComputeHash( rawBytes ).Length );
                int maxLength = keySize - 42;
                int dataLength = bytes.Length;
                int iterations = dataLength / maxLength;
                StringBuilder stringBuilder = new StringBuilder();
                for (int i = 0; i <= iterations; i++)
                {
                    byte[] tempBytes = new byte[
                            (dataLength - maxLength * i > maxLength) ? maxLength :
                                                          dataLength - maxLength * i];
                    Buffer.BlockCopy(bytes, maxLength * i, tempBytes, 0,
                                      tempBytes.Length);
                    byte[] encryptedBytes = rsaCryptoServiceProvider.Encrypt(tempBytes,
                                                                              true);
                    // Be aware the RSACryptoServiceProvider reverses the order of 
                    // encrypted bytes. It does this after encryption and before 
                    // decryption. If you do not require compatibility with Microsoft 
                    // Cryptographic API (CAPI) and/or other vendors. Comment out the 
                    // next line and the corresponding one in the DecryptString function.
                    Array.Reverse(encryptedBytes);
                    // Why convert to base 64?
                    // Because it is the largest power-of-two base printable using only 
                    // ASCII characters
                    stringBuilder.Append(Convert.ToBase64String(encryptedBytes));
                }
                return stringBuilder.ToString();
            }
            catch { return null; }
        }

        public string Decrypt(string inputString, string key)
        {
            try
            {
                int dwKeySize = 1024;

                // TODO: Add Proper Exception Handlers
                RSACryptoServiceProvider rsaCryptoServiceProvider = new RSACryptoServiceProvider(dwKeySize);
                rsaCryptoServiceProvider.ImportParameters(EncryptKey(key));
                int base64BlockSize = ((dwKeySize / 8) % 3 != 0) ?
                  (((dwKeySize / 8) / 3) * 4) + 4 : ((dwKeySize / 8) / 3) * 4;
                int iterations = inputString.Length / base64BlockSize;
                ArrayList arrayList = new ArrayList();
                for (int i = 0; i < iterations; i++)
                {
                    byte[] encryptedBytes = Convert.FromBase64String(
                         inputString.Substring(base64BlockSize * i, base64BlockSize));
                    // Be aware the RSACryptoServiceProvider reverses the order of 
                    // encrypted bytes after encryption and before decryption.
                    // If you do not require compatibility with Microsoft Cryptographic 
                    // API (CAPI) and/or other vendors.
                    // Comment out the next line and the corresponding one in the 
                    // EncryptString function.
                    Array.Reverse(encryptedBytes);
                    arrayList.AddRange(rsaCryptoServiceProvider.Decrypt(
                                        encryptedBytes, true));
                }
                return Encoding.UTF32.GetString(arrayList.ToArray(
                                          Type.GetType("System.Byte")) as byte[]);
            }
            catch { return null; }
        }

        public string GetServerPublicKey() { return DecryptKey(ServerPublicKey); }
        public string GetServerPrivateKey() { return DecryptKey(ServerPrivateKey); }
        public string GetClientPublicKey() { return DecryptKey(ClientPublicKey); }
        public string GetClientPrivateKey() { return DecryptKey(ClientPrivateKey); }

        public void SetServerPublicKey(string key) { ServerPublicKey = EncryptKey(key); }
        public void SetServerPrivateKey(string key) { ServerPrivateKey = EncryptKey(key); }
        public void SetClientPublicKey(string key) { ClientPublicKey = EncryptKey(key); }
        public void SetClientPrivateKey(string key) { ClientPrivateKey = EncryptKey(key); }

        private string DecryptKey(RSAParameters key)
        {
            var sw = new System.IO.StringWriter();
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, key);
            return sw.ToString();
        }

        private RSAParameters EncryptKey(string Key)
        {
            var sr = new System.IO.StringReader(Key);
            var xs = new System.Xml.Serialization.XmlSerializer(typeof(RSAParameters));
            return (RSAParameters)xs.Deserialize(sr);
        }
    }
}
