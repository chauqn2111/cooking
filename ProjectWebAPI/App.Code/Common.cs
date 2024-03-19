/*using Logger;*/

using Logger;
using System.Security.Cryptography;
using System.Text;

namespace ProjectWebMVC.Areas.App.Code
{
    public class Common
    {
        /// <summary>
        /// Mã hóa chuỗi thành chuỗi MD5
        /// Input: sToEncrypt (Chuỗi cần mã hóa)
        /// Output: Chuỗi sau khi mã hóa
        /// </summary>
        public static string EncryptMD5(string sToEncrypt)
        {
            //System.Text.UTF8Encoding ue = new System.Text.UTF8Encoding();
            //byte[] bytes = ue.GetBytes(sToEncrypt);

            //// encrypt bytes
            //System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            //byte[] hashBytes = md5.ComputeHash(bytes);

            //// Convert the encrypted bytes back to a string (base 16)
            //string hashString = "";

            //for (int i = 0; i < hashBytes.Length; i++)
            //{
            //    hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            //}

            //return hashString.PadLeft(32, '0');
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(sToEncrypt);
                byte[] hashBytes = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("x2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Ghi log lỗi và quy trình
        /// Input: sPage(Tên của trang cần Log), string sFunction(Tên của hàm cần Log), string sMessage(Nội dung Log)
        /// </summary>
        public static void WriteLog(string sPage, string sFunction, string sMessage)
        {
            try
            {
                string LogPath = Path.Combine(Environment.CurrentDirectory, "LogInfo");
                //string LogPath = HttpContext.Current.Server.MapPath("~/LogInfo");
                if (!Directory.Exists(LogPath))
                {
                    Directory.CreateDirectory(LogPath);
                }
                CLogger TestLogger = new CLogger();
                TestLogger.Initialize(LogPath, "DotNet.log", 2);
                TestLogger.LogInformation(sPage, sFunction, sMessage);
                TestLogger.Terminate();
            }
            catch (Exception)
            {
            }
        }
    }
}
