/*
 * Copyright (c) Brock Allen.  All rights reserved.
 * see license.txt
 */

// Original Version Copyright:
// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Original License: http://www.apache.org/licenses/LICENSE-2.0

#region Using

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;

#endregion

namespace MagicMaids.Security
{
    // *********************************************************************************************
    //  Crypto Class
    //
    /// <summary>
    ///     Instance for <see cref="Crypto"/> class
    ///     Best practices for crypto implementation:  https://www.codeproject.com/Articles/704865/Salted-Password-Hashing-Doing-it-Right#faq
    ///     </summary>
    //
    // *********************************************************************************************
    public static class Crypto
    {
        #region Fields
        private const int PBKDF2IterCount = 1000;       // default for Rfc2898DeriveBytes is 1000
        private const int PBKDF2SubkeyLength = 256 / 8; // 256 bits
        private const int SaltSize = 256 / 8;           // 256 bits
        #endregion

        #region Methods, Internal
        // *****************************************************************************************
        //  GenerateSaltInternal Method 
        //
        /// <summary>
        ///     Generates internal SALT for hashing
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="byteLength">
        ///     </param>
        /// <returns>
        ///     byte array with salt
        ///     </returns>
        //
        // *****************************************************************************************
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "It really is a byte length")]
        internal static byte[] GenerateSaltInternal(int byteLength = SaltSize)
        {
            byte[] buf = new byte[byteLength];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(buf);
            }
            return buf;
        }

        // *****************************************************************************************
        //  BinaryToHex Method 
        //
        /// <summary>
        ///     Generates internal SALT for hashing
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="byte[]">
        ///     </param>
        /// <returns>
        ///     HEX string
        ///     </returns>
        //
        // *****************************************************************************************
        internal static string BinaryToHex(byte[] data)
        {
            char[] hex = new char[data.Length * 2];

            for (int iter = 0; iter < data.Length; iter++)
            {
                byte hexChar = ((byte)(data[iter] >> 4));
                hex[iter * 2] = (char)(hexChar > 9 ? hexChar + 0x37 : hexChar + 0x30);
                hexChar = ((byte)(data[iter] & 0xF));
                hex[(iter * 2) + 1] = (char)(hexChar > 9 ? hexChar + 0x37 : hexChar + 0x30);
            }
            return new string(hex);
        }

        // *****************************************************************************************
        //  ByteArraysEqual Method 
        //
        /// <summary>
        ///    Compares two byte arrays for equality. 
        ///     </summary>
        /// <remarks>
        ///     The method is specifically written so that the loop is not optimized.
        ///     </remarks>
        /// <param name="byte[]">
        ///     </param>
        /// <param name="byte[]">
        ///     </param>
        /// <returns>
        ///     bool
        ///     </returns>
        //
        // <notes>
        //      1.  Compares two byte arrays for equality. The method is specifically written so that the loop is not optimized.
        //      </notes>
        // *****************************************************************************************
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool ByteArraysEqual(byte[] a, byte[] b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool areSame = true;
            for (int i = 0; i < a.Length; i++)
            {
                areSame &= (a[i] == b[i]);
            }
            return areSame;
        }

        // *****************************************************************************************
        //  GenerateSalt Method 
        //
        /// <summary>
        ///    Gernerates 64 bit encoded array of SALT
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="byteLength">
        ///     </param>
        /// <returns>
        ///    string 
        ///     </returns>
        //
        // *****************************************************************************************
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "It really is a byte length")]
        public static string GenerateSalt(int byteLength = SaltSize)
        {
            return Convert.ToBase64String(GenerateSaltInternal(byteLength));
        }

		[SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "byte", Justification = "It really is a byte length")]
		public static string Encrypt(string plainText, string passPhrase)
		{
			// Salt and IV is randomly generated each time, but is preprended to encrypted cipher text
			// so that the same Salt and IV values can be used when decrypting.  
			var saltStringBytes = GenerateSaltInternal(32);
			var ivStringBytes = GenerateSaltInternal(32);
			var plainTextBytes = Encoding.UTF8.GetBytes(plainText);
			using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, PBKDF2IterCount))
			{
				var keyBytes = password.GetBytes(PBKDF2SubkeyLength);
				using (var symmetricKey = new RijndaelManaged())
				{
					symmetricKey.BlockSize = 256;
					symmetricKey.Mode = CipherMode.CBC;
					symmetricKey.Padding = PaddingMode.PKCS7;
					using (var encryptor = symmetricKey.CreateEncryptor(keyBytes, ivStringBytes))
					{
						using (var memoryStream = new MemoryStream())
						{
							using (var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write))
							{
								cryptoStream.Write(plainTextBytes, 0, plainTextBytes.Length);
								cryptoStream.FlushFinalBlock();
								// Create the final bytes as a concatenation of the random salt bytes, the random iv bytes and the cipher bytes.
								var cipherTextBytes = saltStringBytes;
								cipherTextBytes = cipherTextBytes.Concat(ivStringBytes).ToArray();
								cipherTextBytes = cipherTextBytes.Concat(memoryStream.ToArray()).ToArray();
								memoryStream.Close();
								cryptoStream.Close();
								return Convert.ToBase64String(cipherTextBytes);
							}
						}
					}
				}
			}
		}

		public static string Decrypt(string cipherText, string passPhrase)
		{
			// Get the complete stream of bytes that represent:
			// [32 bytes of Salt] + [32 bytes of IV] + [n bytes of CipherText]
			var cipherTextBytesWithSaltAndIv = Convert.FromBase64String(cipherText);
			// Get the saltbytes by extracting the first 32 bytes from the supplied cipherText bytes.
			var saltStringBytes = cipherTextBytesWithSaltAndIv.Take(PBKDF2SubkeyLength).ToArray();
			// Get the IV bytes by extracting the next 32 bytes from the supplied cipherText bytes.
			var ivStringBytes = cipherTextBytesWithSaltAndIv.Skip(PBKDF2SubkeyLength).Take(PBKDF2SubkeyLength).ToArray();
			// Get the actual cipher text bytes by removing the first 64 bytes from the cipherText string.
			var cipherTextBytes = cipherTextBytesWithSaltAndIv.Skip((PBKDF2SubkeyLength) * 2).Take(cipherTextBytesWithSaltAndIv.Length - ((PBKDF2SubkeyLength) * 2)).ToArray();

			using (var password = new Rfc2898DeriveBytes(passPhrase, saltStringBytes, PBKDF2IterCount))
			{
				var keyBytes = password.GetBytes(PBKDF2SubkeyLength);
				using (var symmetricKey = new RijndaelManaged())
				{
					symmetricKey.BlockSize = 256;
					symmetricKey.Mode = CipherMode.CBC;
					symmetricKey.Padding = PaddingMode.PKCS7;
					using (var decryptor = symmetricKey.CreateDecryptor(keyBytes, ivStringBytes))
					{
						using (var memoryStream = new MemoryStream(cipherTextBytes))
						{
							using (var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read))
							{
								var plainTextBytes = new byte[cipherTextBytes.Length];
								var decryptedByteCount = cryptoStream.Read(plainTextBytes, 0, plainTextBytes.Length);
								memoryStream.Close();
								cryptoStream.Close();
								return Encoding.UTF8.GetString(plainTextBytes, 0, decryptedByteCount);
							}
						}
					}
				}
			}
		}

        // *****************************************************************************************
        //  Hash Method 
        //
        /// <summary>
        ///    Generates hashed string
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="input">
        ///     </param>
        /// <param name="algorithm">
        ///     Hashing algorithm defaults to SHA256
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        internal static string Hash(string input, string algorithm = "sha256")
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            return Hash(Encoding.UTF8.GetBytes(input), algorithm);
        }

        // *****************************************************************************************
        //  Hash Method 
        //
        /// <summary>
        ///    Generates hashed string
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="byte[]">
        ///     </param>
        /// <param name="algorithm">
        ///     Hashing algorithm defaults to SHA256
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        internal static string Hash(byte[] input, string algorithm = "sha256")
        {
            if (input == null)
            {
                throw new ArgumentNullException("input");
            }

            using (HashAlgorithm alg = HashAlgorithm.Create(algorithm))
            {
                if (alg != null)
                {
                    byte[] hashData = alg.ComputeHash(input);
                    return BinaryToHex(hashData);
                }
                else
                {
                    throw new InvalidOperationException();//String.Format(CultureInfo.InvariantCulture, HelpersResources.Crypto_NotSupportedHashAlg, algorithm));
                }
            }
        }


        // *****************************************************************************************
        //  SHA256 HASH Method 
        //
        /// <summary>
        ///    Generates hashed string SHA256
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="input">
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "SHA", Justification = "Consistent with the Framework, which uses SHA")]
        internal static string SHA256(string input)
        {
            return Hash(input, "sha256");
        }

        // *****************************************************************************************
        // HashPassword Method 
        //
        /// <summary>
        ///    Generates hashed password
        ///     </summary>
        /// <remarks>
        ///     PBKDF2 with HMAC-SHA256, 128-bit salt, 256-bit subkey, 1000 iterations.
        ///     Format: { 0x00, salt, subkey }
        ///     </remarks>
        /// <param name="password">
        ///     </param>
        /// <param name="iterationCount">
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        internal static string HashPassword(string password, int iterationCount = PBKDF2IterCount)
        {
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            // Produce a version 0 (see comment above) password hash.
            byte[] salt;
            byte[] subkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, SaltSize, iterationCount))
            {
                salt = deriveBytes.Salt;
                subkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }

            byte[] outputBytes = new byte[1 + SaltSize + PBKDF2SubkeyLength];
            Buffer.BlockCopy(salt, 0, outputBytes, 1, SaltSize);
            Buffer.BlockCopy(subkey, 0, outputBytes, 1 + SaltSize, PBKDF2SubkeyLength);
            return Convert.ToBase64String(outputBytes);
        }

        // *****************************************************************************************
        // VerifyHashedPassword Method 
        //
        /// <summary>
        ///    Verifies hashed password
        ///     </summary>
        /// <remarks>
        ///     hashedPassword must be of the format of HashWithPassword (salt + Hash(salt+input)
        ///     </remarks>
        /// <param name="hashedPassword">
        ///     </param>
        /// <param name="password">
        ///     </param>
        /// <param name="iterationCount">
        ///     </param>
        /// <returns>
        ///    Boolean
        ///     </returns>
        //
        // *****************************************************************************************
        internal static bool VerifyHashedPassword(string hashedPassword, string password, int iterationCount = PBKDF2IterCount)
        {
            if (hashedPassword == null)
            {
                throw new ArgumentNullException("hashedPassword");
            }
            if (password == null)
            {
                throw new ArgumentNullException("password");
            }

            byte[] hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

            // Verify a version 0 (see comment above) password hash.

            if (hashedPasswordBytes.Length != (1 + SaltSize + PBKDF2SubkeyLength) || hashedPasswordBytes[0] != 0x00)
            {
                // Wrong length or version header.
                return false;
            }

            byte[] salt = new byte[SaltSize];
            Buffer.BlockCopy(hashedPasswordBytes, 1, salt, 0, SaltSize);
            byte[] storedSubkey = new byte[PBKDF2SubkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, 1 + SaltSize, storedSubkey, 0, PBKDF2SubkeyLength);

            byte[] generatedSubkey;
            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, iterationCount))
            {
                generatedSubkey = deriveBytes.GetBytes(PBKDF2SubkeyLength);
            }
            return ByteArraysEqual(storedSubkey, generatedSubkey);
        }
        #endregion 

    }
}