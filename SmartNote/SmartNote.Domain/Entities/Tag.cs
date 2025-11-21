using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    /// <summary>
    /// 笔记标签（多对多：Note - Tag）
    /// </summary>
    public class Tag
    {
        public int Id { get; set; }

        /// <summary>
        /// 标签归属的用户（哪位用户创建的标签）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 标签名称，如“索引”“算法”“Vue3”
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 标签颜色（可选）
        /// </summary>
        public string? Color { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        // 导航属性
        public User User { get; set; } = null!;
        public ICollection<NoteTag> NoteTags { get; set; } = new List<NoteTag>();
    }
}
