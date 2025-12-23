using System;
using System.IO;

namespace ZJZTQY.Helpers
{
    public static class SessionHelper
    {

        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "login.session");

        public static void SaveUser(string email)
        {
            try
            {
                File.WriteAllText(FilePath, email);
            }
            catch { /* 忽略写入错误 */ }
        }


        public static string? GetUser()
        {
            try
            {
                if (File.Exists(FilePath))
                {
                    return File.ReadAllText(FilePath).Trim();
                }
            }
            catch { /* 忽略读取错误 */ }
            return null;
        }


        public static void Clear()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}