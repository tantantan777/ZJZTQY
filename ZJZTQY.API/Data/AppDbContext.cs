using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using ZJZTQY.API.Models;

namespace ZJZTQY.API.Data
{
    public class AppDbContext : DbContext
    {
        // 构造函数：接收外部注入的数据库配置选项
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // 保持原本的逻辑：邮箱必须唯一
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        }
    }
}