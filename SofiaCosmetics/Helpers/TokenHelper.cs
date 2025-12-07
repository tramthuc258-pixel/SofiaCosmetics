using System;
using System.Linq;
using System.Text;
using System.Web;
using System.Security.Cryptography;
using Newtonsoft.Json;

namespace SofiaCosmetics.Helpers
{
    public class TokenHelper
    {
        private static string secretKey = "MY_SECRET_KEY_2025_ABCXYZ";

        public static string CreateResetToken(string email)
        {
            var data = new
            {
                Email = email,
                Expire = DateTime.Now.AddMinutes(30)
            };

            string json = JsonConvert.SerializeObject(data);
            byte[] bytes = Encoding.UTF8.GetBytes(json);

            using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
            {
                byte[] signature = hmac.ComputeHash(bytes);
                string token = Convert.ToBase64String(bytes) + "." + Convert.ToBase64String(signature);
                return HttpUtility.UrlEncode(token);
            }
        }

        public static string ValidateToken(string token)
        {
            try
            {
                token = HttpUtility.UrlDecode(token);
                var parts = token.Split('.');
                if (parts.Length != 2) return null;

                byte[] dataBytes = Convert.FromBase64String(parts[0]);
                byte[] signBytes = Convert.FromBase64String(parts[1]);

                using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secretKey)))
                {
                    byte[] expectedSign = hmac.ComputeHash(dataBytes);

                    if (!expectedSign.SequenceEqual(signBytes))
                        return null;
                }

                string json = Encoding.UTF8.GetString(dataBytes);
                dynamic obj = JsonConvert.DeserializeObject(json);

                // Kiểm tra hết hạn
                DateTime expire = obj.Expire;
                if (expire < DateTime.Now) return null;

                return (string)obj.Email;
            }
            catch
            {
                return null;
            }
        }
    }
}
