 using System;
using System.Text;

namespace MTechServices.Models
{
    public class AuthUtility {
        //public static string HashPassword(string username, string password) {
        //    // Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
        //    byte[] originalBytes = Encoding.Default.GetBytes(string.Format("{0}&*%$#{1}", username, password));
        //    byte[] encodedBytes = new MD5CryptoServiceProvider().ComputeHash(originalBytes);

        //    //Convert encoded bytes back to a 'readable' string
        //    return BitConverter.ToString(encodedBytes);
        //}

        public static string HashPassword(string password) {
            string strPasswd = "";
            string passwordHash = password.PadRight(32, ' ');

            int intIndex = 0;
            while (intIndex < passwordHash.Length) {
                if (passwordHash.Substring(intIndex, 1) == " ") {
                    strPasswd = strPasswd + (passwordHash.Substring(intIndex, 1));
                } else {
                    // convert each letter in byte array
                    Byte[] tempBytes = Encoding.ASCII.GetBytes(passwordHash.Substring(intIndex, 1));
                    
                    // add 9 to that byte
                    tempBytes = new[] { Convert.ToByte((Convert.ToInt32(tempBytes[0]) + 9)) };

                    strPasswd = strPasswd + Encoding.ASCII.GetString(tempBytes);
                }

                intIndex = intIndex + 1;
            }

            return strPasswd;
        }
    }
}
