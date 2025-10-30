// SmartNote.Domain/UserProfile.cs
using System;

namespace SmartNote_Domain // (请确保命名空间与你的项目名一致)
{
    public class UserProfile
    {
        public int Id { get; set; }
        public string Nickname { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; } // 头像图片路径

        // VVVVVV 在这里添加 Bio 属性 VVVVVV
        public string? Bio { get; set; } // 个人简介
                                         // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        public DateTime LastUpdateTime { get; set; }

        // 1-to-1 关系的外键
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
    }
}