using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace AutoTorrentInspection.Util
{
    public class AESEncryptDecrypt
    {
        private const string Extension   = ".tctc";
        private const int IterationCount = 10000;
        private const int SaltLength     = 32;
        private const int KeySize        = 256;
        private const int BlockSize      = 128;
        private const int BufferSize     = 1024 * 1024;

        // Call this function to remove the key from memory after use for security
        // Usage: GCHandle gch=GCHandle.Alloc(arr,GCHandleType.Pinned);
        //        ZeroMemory(gch.AddrOfPinnedObject(),arr.Length);
        //        gch.Free();
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, EntryPoint = "RtlZeroMemory")]
        private static extern bool ZeroMemory(IntPtr destination, int length);

        public static void ZeroMemory(byte[] arr)
        {
            var gch = GCHandle.Alloc(arr, GCHandleType.Pinned);
            ZeroMemory(gch.AddrOfPinnedObject(), arr.Length);
            gch.Free();
        }

        private static byte[] GenerateRandomSalt()
        {
            var data = new byte[SaltLength];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }
            return data;
        }

        public static byte[] GenerateRandomKey()
        {
            var data = new byte[KeySize/8];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(data);
            }
            return data;
        }

        public static async Task AES_Encrypt(string inputFile, byte[] passwordBytes)
        {
            //http://stackoverflow.com/questions/27645527/aes-encryption-on-large-files

            var salt = GenerateRandomSalt();

            //Set Rijndael symmetric encryption algorithm
            //Cipher modes: http://security.stackexchange.com/questions/52665/which-is-the-best-cipher-mode-and-padding-mode-for-aes-encryption
            var AES = new RijndaelManaged
            {
                KeySize   = KeySize,
                BlockSize = BlockSize,
                Padding   = PaddingMode.PKCS7,
                Mode      = CipherMode.CFB
            };

            //http://stackoverflow.com/questions/2659214/why-do-i-need-to-use-the-rfc2898derivebytes-class-in-net-instead-of-directly
            //"What it does is repeatedly hash the user password along with the salt." High iteration counts.
            var key = new Rfc2898DeriveBytes(passwordBytes, salt, IterationCount);
            AES.Key = key.GetBytes(AES.KeySize   / 8);
            AES.IV  = key.GetBytes(AES.BlockSize / 8);

            var buffer = new byte[BufferSize];

            try
            {
                using (var fsCrypt = new FileStream(inputFile + Extension, FileMode.Create))
                {
                    //write salt to the begining of the output file, so in this case can be random every time
                    fsCrypt.Write(salt, 0, salt.Length);
                    using (var cs = new CryptoStream(fsCrypt, AES.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        using (var fsIn = File.OpenRead(inputFile))
                        {
                            int read;
                            while ((read = await fsIn.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                cs.Write(buffer, 0, read);
                            }
                            cs.FlushFinalBlock();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        public static async Task AES_Decrypt(string inputFile, byte[] passwordBytes)
        {
            if (!inputFile.ToLower().EndsWith(Extension)) return;
            //todo:
            // - create error message on wrong password
            // - on cancel: close and delete file
            // - on wrong password: close and delete file!
            // - create a better filen name
            // - could be check md5 hash on the files but it make this slow

            var buffer = new byte[BufferSize];
            try
            {
                using (var fsCrypt = File.OpenRead(inputFile))
                {
                    var salt = new byte[SaltLength];
                    fsCrypt.Read(salt, 0, salt.Length);

                    var AES = new RijndaelManaged
                    {
                        KeySize   = KeySize,
                        BlockSize = BlockSize,
                        Padding   = PaddingMode.PKCS7,
                        Mode      = CipherMode.CFB
                    };
                    var key = new Rfc2898DeriveBytes(passwordBytes, salt, IterationCount);
                    AES.Key = key.GetBytes(AES.KeySize   / 8);
                    AES.IV  = key.GetBytes(AES.BlockSize / 8);

                    using (var cs = new CryptoStream(fsCrypt, AES.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        inputFile = inputFile.Substring(0, inputFile.Length - Extension.Length);
                        using (var fsOut = new FileStream(inputFile, FileMode.Create))
                        {
                            int read;
                            while ((read = await cs.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                fsOut.Write(buffer, 0, read);
                            }
                        }
                    }
                }
            }
            catch (CryptographicException exCryptographicException)
            {
                Debug.WriteLine("CryptographicException error: " + exCryptographicException.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }
    }
}