//创建迁移记录Add-Migration AddUserProfileFields
//更新数据库Update-Database

using System;
using System.ComponentModel.DataAnnotations;

namespace ZJZTQY.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // --- 以下字段初始化为空，等待用户后续补充 ---
        public string RealName { get; set; } = string.Empty;
        public DateTime? Birthday { get; set; }
        public string Gender { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public string Company { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string EmployeeNumber { get; set; } = string.Empty;
        public DateTime? EntryDate { get; set; }
        public string Status { get; set; } = string.Empty; 
        public string Position { get; set; } = string.Empty;
        public string ProjectNames { get; set; } = string.Empty;
        public string Signature { get; set; } = string.Empty;
        public string Remark { get; set; } = string.Empty;

        public bool IsOnline { get; set; } = false; 
        public string Role { get; set; } = string.Empty; 
        public bool IsActive { get; set; } = true; 
    }
}