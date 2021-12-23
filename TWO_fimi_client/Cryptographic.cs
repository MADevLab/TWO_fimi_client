using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;

namespace TWO_fimi_client
{      
    class Cryptographic:Logging
    {
        public byte[] GetPasswordHash(string userPassword)
        {            
            byte[] userPasswordKeyRaw = Encoding.ASCII.GetBytes(userPassword.PadRight(16, '\x20'));
            byte[] userPasswordRaw = Encoding.ASCII.GetBytes(userPassword.PadRight(8, '\x20'));

            TripleDES tripleDES = Create3DES(userPasswordKeyRaw);
            ICryptoTransform cryptoTransform = tripleDES.CreateEncryptor();

            byte[] passwordHashBinary = new List<byte>(cryptoTransform.TransformFinalBlock(userPasswordRaw, 0, userPasswordRaw.Length)).GetRange(0,8).ToArray();
            //string passwordHashStr = BitConverter.ToString(passwordHashBinary).Replace("\x2D", String.Empty);

            //Console.WriteLine(String.Format("PasswordHash: {0}", BitConverter.ToString(passwordHashBinary).Replace("\x2D", String.Empty)));

            return passwordHashBinary;//BitConverter.ToString(cryptoTransform.TransformFinalBlock(userPasswordRaw, 0, userPasswordRaw.Length)).Replace("\x2D",String.Empty).Substring(0,16);
        }
        
        public string GetCryptPassword(byte[] passwordHash, string challenge)
        {
            try
            {
                // Create a MemoryStream.
                MemoryStream mStream = new MemoryStream();

                // Create a new DES object.
                DES DESalg = DES.Create();

                // Create a CryptoStream using the MemoryStream and the passed key and initialization vector (IV).
                CryptoStream cStream = new CryptoStream(mStream, DESalg.CreateEncryptor(passwordHash, new byte[8]), CryptoStreamMode.Write);

                // Convert the passed string to a byte array.
                byte[] toEncrypt = new ASCIIEncoding().GetBytes(challenge);

                // Write the byte array to the crypto stream and flush it.
                cStream.Write(toEncrypt, 0, toEncrypt.Length);
                cStream.FlushFinalBlock();

                // Get an array of bytes from the MemoryStream that holds the encrypted data.
                string cryptPassword = BitConverter.ToString(new List<byte>(mStream.ToArray()).GetRange(0, 8).ToArray()).Replace("\x2D", String.Empty);

                // Close the streams.
                cStream.Close();
                mStream.Close();

                // Return the encrypted string.
                Console.WriteLine($"\n{"[{0}]",-7}Calculating the cryptogram of a password...",
                            DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));                
                Console.WriteLine($"{"[{0}]",-7}StatusDescription: [OK]", DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                Console.Write("\n*Crypt password (encrypted DES hex value): ");
                SetColor(cryptPassword, ConsoleColor.Green);

                return cryptPassword;
            }
            catch (CryptographicException e)
            {
                Console.WriteLine("A Cryptographic error occurred: {0}", e.Message);
                return null;
            }
        }

        private TripleDES Create3DES(byte[] key)
        {
            TripleDES tripleDes = new TripleDESCryptoServiceProvider
            {
                Key = key
            };

            tripleDes.IV = new byte[tripleDes.BlockSize / 8];
            return tripleDes;
        }
    }   
}
