using System;

namespace SmartNote.Domain.Entities
{
    /// <summary>
    /// 笔记操作日志（用于学习统计 / 热力图）
    /// 一条记录代表用户对某个笔记的一次操作，比如创建、更新
    /// </summary>
    public class NoteActivityLog
    {
        public int Id { get; set; }

        /// <summary>
        /// 操作人（当前登录用户）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 被操作的笔记
        /// </summary>
        public int NoteId { get; set; }

        /// <summary>
        /// 操作类型：Create / Update （先用字符串，简单好用）
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// 发生时间（UTC）
        /// </summary>
        public DateTime Time { get; set; }

        // ------------ 导航属性（可选）-----------------
        public User? User { get; set; }
        public Note? Note { get; set; }
    }
}
