#region Using

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

#endregion

namespace MagicMaids.Security
{
    // *********************************************************************************************
    //  DefaultCrypto Class
    //
    /// <summary>
    ///     Instance for <see cref="DefaultCrypto"/> class
    ///     Use this class to extend base Crypto class and to change default iterations to not be based on year/PKCS
    ///     FROM OWASP: https://www.owasp.org/index.php/Password_Storage_Cheat_Sheet
    ///     
    ///     This class contains multiple generic HASH / VERIFY HASH functions that can be made used to create other 
    ///     HASH implementations if needed later
    ///     </summary>
    //
    // *********************************************************************************************
    public class DefaultCrypto
    {
        #region Constants
        public const char PasswordHashingIterationCountSeparator = '.';
        const int StartYear = 2000;
        const int StartCount = 1000;
        #endregion

        #region Methods, Public

        // *****************************************************************************************
        //  GenerateNumericCode Method 
        //
        /// <summary>
        ///    Gernerates long numeric code that can be used to generate random salt
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="integer digits">
        ///     </param>
        /// <returns>
        ///    string 
        ///     </returns>
        //
        // *****************************************************************************************
        public string GenerateNumericCode(int digits)
        {
            // 18 is good size for a long
            if (digits > 18) digits = 18;
            if (digits <= 0) digits = 6;

            var bytes = Crypto.GenerateSaltInternal(sizeof(long));
            var val = BitConverter.ToInt64(bytes, 0);
            var mod = (int)Math.Pow(10, digits);
            val %= mod;
            val = Math.Abs(val);

            return val.ToString("D" + digits);
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
        public string GenerateSalt()
        {
            return Crypto.GenerateSalt();
        }

        // *****************************************************************************************
        //  HashPassword Method 
        //
        /// <summary>
        ///    Generates hashed password string
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="string">
        ///        Input value to be hashed
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        public string HashPassword(string password)
        {
            return HashPassword(password, IterationCount());
        }

        // *****************************************************************************************
        //  HashPassword Method 
        //
        /// <summary>
        ///    Generates hashed password string
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="string">
        ///        Input value to be hashed
        ///     </param>
        /// <param name="iterations">
        ///     iteration count for PBKDF2
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        public string HashPassword(string password, int iterations)
        {
            var count = iterations;
            if (count <= 0)
            {
                //count = GetIterationsFromYear(DateTime.Now.Year);
                count = IterationCount();
            }
            var result = Crypto.HashPassword(password, count);
            return EncodeIterations(count) + PasswordHashingIterationCountSeparator + result;
        }


        // *****************************************************************************************
        //  VerifyHashedPassword Method 
        //
        /// <summary>
        ///    Verifies hashed password with plain text password
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="hashedPassword">
        ///     </param>
        /// <param name="password">
        ///     </param>
        /// <returns>
        ///    bool
        ///     </returns>
        //
        // *****************************************************************************************
        public bool VerifyHashedPassword(string hashedPassword, string password)
        {
            if (hashedPassword.Contains(PasswordHashingIterationCountSeparator))
            {
                var parts = hashedPassword.Split(PasswordHashingIterationCountSeparator);
                if (parts.Length != 2) return false;

                int count = DecodeIterations(parts[0]);
                if (count <= 0) return false;

                hashedPassword = parts[1];

                return Crypto.VerifyHashedPassword(hashedPassword, password, count);
            }
            else
            {
                return Crypto.VerifyHashedPassword(hashedPassword, password);
            }
        }

        // *****************************************************************************************
        //  Hash Method 
        //
        /// <summary>
        ///    Generates hashed string
        ///     </summary>
        /// <remarks>
        ///    Generic hashing function that can be used for alternative hashing implementations
        ///     </remarks>
        /// <param name="string">
        ///     Input value to be hashed
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        public string Hash(string value)
        {
            return Crypto.Hash(value);
        }

        // *****************************************************************************************
        // VerifyHash Method 
        //
        /// <summary>
        ///    Verifies hashed string
        ///     </summary>
        /// <remarks>
        ///    Generic verification function that can be used for alternative hashing implementations
        ///     </remarks>
        /// <param name="value">
        ///     </param>
        /// <param name="hash">
        ///     </param>
        /// <returns>
        ///    Boolean
        ///     </returns>
        //
        // *****************************************************************************************
        public bool VerifyHash(string value, string hash)
        {
            var hashedValue = Hash(value);
            return SlowEquals(hashedValue, hash);
        }

        // *****************************************************************************************
        //  Hash Method 
        //
        /// <summary>
        ///    Generates hashed string
        ///     </summary>
        /// <remarks>
        ///    Generic hash function that can be used for alternative hashing implementations
        ///     </remarks>
        /// <param name="string">
        ///        Input value to be hashed
        ///     </param>
        /// <param name="key">
        ///     </param>
        /// <returns>
        ///    Hash string 
        ///     </returns>
        //
        // *****************************************************************************************
        public string Hash(string value, string key)
        {
            if (String.IsNullOrWhiteSpace(value)) throw new ArgumentNullException("value");
            if (String.IsNullOrWhiteSpace(key)) throw new ArgumentNullException("key");

            var valueBytes = System.Text.Encoding.UTF8.GetBytes(value);
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(key);

            var alg = new System.Security.Cryptography.HMACSHA512(keyBytes);
            var hash = alg.ComputeHash(valueBytes);

            var result = Crypto.BinaryToHex(hash);
            return result;
        }

        // *****************************************************************************************
        // VerifyHash Method 
        //
        /// <summary>
        ///    Verifies hashed string
        ///     </summary>
        /// <remarks>
        ///    Generic verification function that can be used for alternative hashing implementations
        ///     </remarks>
        /// <param name="value">
        ///     </param>
        /// <param name="key">
        ///     </param>
        /// <param name="hash">
        ///     </param>
        /// <returns>
        ///    Boolean
        ///     </returns>
        //
        // *****************************************************************************************
        public bool VerifyHash(string value, string key, string hash)
        {
            var hashedValue = Hash(value, key);
            return SlowEquals(hashedValue, hash);
        }

        // *****************************************************************************************
        //  EncodeIterations Method 
        //
        /// <summary>
        ///    Returns interation count as series of unicode characters
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="iteration count">
        ///     </param>
        /// <returns>
        ///     string
        ///     </returns>
        //
        // <notes>
        //      </notes>
        // *****************************************************************************************
        public string EncodeIterations(int count)
        {
            return count.ToString("X");
        }

        // *****************************************************************************************
        //  GetIterationsFromYear Method 
        //
        /// <summary>
        ///    Returns decrypted interation count
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="int current year">
        ///     </param>
        /// <returns>
        ///     interval counter
        ///     </returns>
        //
        // <notes>
        //      </notes>
        // *****************************************************************************************
        public int GetIterationsFromYear(int year)
        {
            if (year > StartYear)
            {
                var diff = (year - StartYear) / 2;
                var mul = (int)Math.Pow(2, diff);
                int count = StartCount * mul;
                // if we go negative, then we wrapped (expected in year ~2044). 
                // Int32.Max is best we can do at this point
                if (count < 0) count = Int32.MaxValue;
                return count;
            }
            return StartCount;
        }
        #endregion

        #region Methods, Private
        // *****************************************************************************************
        //  SlowEquals Method 
        //
        /// <summary>
        ///    Compares two stringsfor equality. 
        ///     </summary>
        /// <remarks>
        ///     The method is specifically written so that the loop is not optimized.
        ///     </remarks>
        /// <param name="string">
        ///     </param>
        /// <param name="string">
        ///     </param>
        /// <returns>
        ///     bool
        ///     </returns>
        //
        // <notes>
        //      1.  Compares two strings for equality. The method is specifically written so that the loop is not optimized.
        //      </notes>
        // *****************************************************************************************
        [MethodImpl(MethodImplOptions.NoOptimization)]
        private static bool SlowEqualsInternal(string a, string b)
        {
            if (Object.ReferenceEquals(a, b))
            {
                return true;
            }

            if (a == null || b == null || a.Length != b.Length)
            {
                return false;
            }

            bool same = true;
            for (var i = 0; i < a.Length; i++)
            {
                same &= (a[i] == b[i]);
            }
            return same;
        }



        // *****************************************************************************************
        //  SlowEquals Method 
        //
        /// <summary>
        ///    Compares two stringsfor equality. 
        ///     </summary>
        /// <remarks>
        ///     The method is specifically written so that the loop is not optimized.
        ///     </remarks>
        /// <param name="string">
        ///     </param>
        /// <param name="string">
        ///     </param>
        /// <returns>
        ///     bool
        ///     </returns>
        //
        // <notes>
        //      1.  Compares two strings for equality. The method is specifically written so that the loop is not optimized.
        //      </notes>
        // *****************************************************************************************
        private bool SlowEquals(string a, string b)
        {
            return SlowEqualsInternal(a, b);
        }

        // *****************************************************************************************
        //  IterationCount Method 
        //
        /// <summary>
        ///    returns the iteration counter for key stretching in the hashing algorithm
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="integer digits">
        ///     </param>
        /// <returns>
        ///    string 
        ///     </returns>
        //
        // *****************************************************************************************
        private Int32 IterationCount()
        {
            return 50000;
        }

        // *****************************************************************************************
        //  DecodeIterations Method 
        //
        /// <summary>
        ///    Returns decrypted interation count
        ///     </summary>
        /// <remarks>
        ///     </remarks>
        /// <param name="prefix string">
        ///     </param>
        /// <returns>
        ///     string
        ///     </returns>
        //
        // <notes>
        //      </notes>
        // *****************************************************************************************
        private int DecodeIterations(string prefix)
        {
            int val;
            if (Int32.TryParse(prefix, System.Globalization.NumberStyles.HexNumber, null, out val))
            {
                return val;
            }
            return -1;
        }


        #endregion

    }
}