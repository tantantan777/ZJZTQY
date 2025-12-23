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
        public string Username { get; set; } = string.Empty; // 用户名 (通常存邮箱前缀或昵称)

        [Required]
        public string Email { get; set; } = string.Empty;    // 登录邮箱 (唯一标识)

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; // 注册日期

 
        public string RealName { get; set; } = string.Empty;   // 真实姓名 ★重要
        public DateTime? Birthday { get; set; }                // 出生年月日 (可为空)
        public string Gender { get; set; } = "保密";           // 性别
        public string Avatar { get; set; } = string.Empty;     // 头像路径 (存图片URL或本地路径)
        public string PhoneNumber { get; set; } = string.Empty;// 手机号
        public string Address { get; set; } = string.Empty;    // 家庭/居住地址


        public string Company { get; set; } = "筑恒基石";      // 所属公司 (默认值)
        public string Department { get; set; } = string.Empty; // 所属部门
        public string EmployeeNumber { get; set; } = string.Empty; // 工号
        public DateTime? EntryDate { get; set; }               // 入职日期
        public string Status { get; set; } = "在职";           // 在职状态 (在职/离职/休假)

  
        public string Position { get; set; } = string.Empty;


        public string ProjectNames { get; set; } = string.Empty; // 所负责的项目名称集合


        public string Signature { get; set; } = string.Empty;  // 工作宣言/个性签名
        public string Remark { get; set; } = string.Empty;     // 备注
        public bool IsOnline { get; set; } = false;            // 在线状态 (true=在线)

        public string Role { get; set; } = "User";             // 角色 (User/Admin)
        public bool IsActive { get; set; } = true;             // 账号是否启用 (封禁控制)
    }
}