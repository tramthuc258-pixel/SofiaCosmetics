using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace SofiaCosmetics
{
    public class Util
    {
        public static string HmacSHA512(string key, string inputData)
        {
            if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(inputData))
                return string.Empty;

            var hash = new StringBuilder();
            byte[] keyBytes = Encoding.UTF8.GetBytes(key);
            byte[] inputBytes = Encoding.UTF8.GetBytes(inputData);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                byte[] hashValue = hmac.ComputeHash(inputBytes);
                foreach (var b in hashValue)
                {
                    hash.Append(b.ToString("x2"));
                }
            }
            return hash.ToString();
        }

        public static string GetIpAddress()
        {
            string ipAddress;
            try
            {
                ipAddress = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (string.IsNullOrEmpty(ipAddress) || (ipAddress.ToLower() == "unknown"))
                    ipAddress = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
            }
            catch (Exception ex)
            {
                ipAddress = "Invalid IP: " + ex.Message;
            }

            return ipAddress;
        }
        public static string CreateHmacSHA256(string rawData, string secretKey)
        {
            var keyBytes = System.Text.Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(rawData);

            using (var hmacsha256 = new System.Security.Cryptography.HMACSHA256(keyBytes))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return BitConverter.ToString(hash).Replace("-", "").ToLower();
            }
        }

    }
}