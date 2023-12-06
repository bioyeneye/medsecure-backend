using System.Security.Cryptography;

namespace MedSecureSystem.Infrastructure.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Security.Cryptography;

    public static class PasswordHelper
    {
        private const int DefaultPasswordLength = 12;
        private static readonly char[] LowerCase = "abcdefghijklmnopqrstuvwxyz".ToCharArray();
        private static readonly char[] UpperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
        private static readonly char[] Digits = "1234567890".ToCharArray();
        private static readonly char[] SpecialCharacters = "!@#$%^&*()".ToCharArray();
        private static readonly char[][] CharacterSets = { LowerCase, UpperCase, Digits, SpecialCharacters };
        private static char[] punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();

        public static string GeneratePassword(int length = DefaultPasswordLength, int numberOfNonAlphanumericCharacters = 3)
        {


            string password;
            int index;
            byte[] buf;
            char[] cBuf;
            int count;

            do
            {
                buf = new byte[length];
                cBuf = new char[length];
                count = 0;

                (new RNGCryptoServiceProvider()).GetBytes(buf);

                for (int iter = 0; iter < length; iter++)
                {
                    int i = (int)(buf[iter] % 87);
                    if (i < 10)
                        cBuf[iter] = (char)('0' + i);
                    else if (i < 36)
                        cBuf[iter] = (char)('A' + i - 10);
                    else if (i < 62)
                        cBuf[iter] = (char)('a' + i - 36);
                    else
                    {
                        cBuf[iter] = punctuations[i - 62];
                        count++;
                    }
                }

                if (count < numberOfNonAlphanumericCharacters)
                {
                    int j, k;
                    Random rand = new Random();

                    for (j = 0; j < numberOfNonAlphanumericCharacters - count; j++)
                    {
                        do
                        {
                            k = rand.Next(0, length);
                        }
                        while (!Char.IsLetterOrDigit(cBuf[k]));

                        cBuf[k] = punctuations[rand.Next(0, punctuations.Length)];
                    }
                }

                password = new string(cBuf);
            }
            while (IsDangerousString(password, out index));

            return password;
        }

        private static bool IsAtoZ(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }


#if OBSOLETE
        private static char[] startingChars = new char[] { '<', '&', '/', '*', 'o', 'O', 's', 'S' , 'e', 'E' };
#endif // OBSOLETE
        private static char[] startingChars = new char[] { '<', '&' };

        internal static bool IsDangerousString(string s, out int matchIndex)
        {
            //bool inComment = false;
            matchIndex = 0;

            for (int i = 0; ;)
            {

                // Look for the start of one of our patterns
                int n = s.IndexOfAny(startingChars, i);

                // If not found, the string is safe
                if (n < 0) return false;

                // If it's the last char, it's safe
                if (n == s.Length - 1) return false;

                matchIndex = n;

                switch (s[n])
                {
                    case '<':
                        // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
                        if (IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?') return true;
                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. &#83;)
                        if (s[n + 1] == '#') return true;
                        break;
#if OBSOLETE
                case '/':
                    // Look for a starting C style comment (i.e. "/*")
                    if (s[n+1] == '*') {
                        // Remember that we're inside a comment
                        inComment = true;
                        n++;
                    }
                    break;
                case '*':
                    // If we're not inside a comment, we don't care about finding "*/".
                    if (!inComment) break;

                    // Look for the end of a C style comment (i.e. "*/").  If we found one,
                    // we found a full comment, which we don't allow (VSWhidbey 228396).
                    if (s[n+1] == '/') return true;
                    break;
                case 'o':
                case 'O':
                    if (IsDangerousOnString(s, n))
                        return true;
                    break;
                case 's':
                case 'S':
                    if (IsDangerousScriptString(s, n))
                        return true;
                    break;
                case 'e':
                case 'E':
                    if (IsDangerousExpressionString(s, n))
                        return true;
                    break;
#endif // OBSOLETE
                }

                // Continue searching
                i = n + 1;
            }

        }


        public static string GenerateTemporaryPassword(int length = DefaultPasswordLength)
        {
            if (length < CharacterSets.Length)
            {
                throw new ArgumentException($"Password length must be at least {CharacterSets.Length} to accommodate all character types.");
            }

            var passwordChars = new char[length];
            using var rng = RandomNumberGenerator.Create();
            var byteBuffer = new byte[4];

            // Ensuring at least one character from each set is included
            for (int i = 0; i < CharacterSets.Length; i++)
            {
                rng.GetBytes(byteBuffer);
                int pos = BitConverter.ToInt32(byteBuffer, 0) & int.MaxValue;
                passwordChars[i] = CharacterSets[i][pos % CharacterSets[i].Length];
            }

            // Fill the remaining characters
            for (int i = CharacterSets.Length; i < length; i++)
            {
                rng.GetBytes(byteBuffer);
                int pos = BitConverter.ToInt32(byteBuffer, 0) & int.MaxValue;
                int setIndex = pos % CharacterSets.Length;
                passwordChars[i] = CharacterSets[setIndex][pos % CharacterSets[setIndex].Length];
            }

            // Shuffle the characters
            Shuffle(passwordChars, rng);

            return new string(passwordChars);
        }

        private static void Shuffle(char[] array, RandomNumberGenerator rng)
        {
            var byteBuffer = new byte[4];
            for (int i = array.Length - 1; i > 0; i--)
            {
                rng.GetBytes(byteBuffer);
                int j = (BitConverter.ToInt32(byteBuffer, 0) & int.MaxValue) % (i + 1);

                var temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }

    }

}