using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Utils
{
    public static class MyCrypt
    {
        private static string sHashKey = "SGVsbG8gV29ybGQ=";

        public static string GenerateRandomSecretKey(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            string randomKey = new string(Enumerable.Repeat(validChars, length)
              .Select(s => s[random.Next(s.Length)]).ToArray());

            return randomKey;
        }

        public static string GetHash(this string sPassword, params string[] Params)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(sPassword);
            foreach (string curChunk in Params)
            {
                sb.Append(curChunk);
            }
            HMACSHA512 hashProvider = new HMACSHA512(Convert.FromBase64String(sHashKey));
            return Convert.ToBase64String(hashProvider.ComputeHash(Encoding.UTF8.GetBytes(sb.ToString())));
        }
    }
}
