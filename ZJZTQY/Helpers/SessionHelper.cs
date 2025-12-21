using System;
using System.IO;

namespace ZJZTQY.Helpers
{
    public static class SessionHelper
    {
        // 凭证文件保存在程序运行目录
        private static readonly string FilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "login.session");

        // 保存邮箱
        public static void SaveUser(string email)
        {
            try
            {
                File.WriteAllText(FilePath, email);
            }
            catch { /* 忽略写入错误 */ }
        }

        // 读取邮箱
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

        // 清除凭证（比如退出登录时用）
        public static void Clear()
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
    }
}