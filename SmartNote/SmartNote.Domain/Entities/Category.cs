using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities
{
    /// <summary>
    /// 笔记分类（每个用户可以维护自己的分类列表）
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        /// <summary>
        /// 分类归属的用户（哪位用户创建的分类）
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// 分类名称，如“数据库”“英语词汇”
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 分类颜色（可选，前端用来渲染标签色块）
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// 排序值，越小越靠前
        /// </summary>
        public int SortOrder { get; set; }

        public DateTime CreateTime { get; set; } = DateTime.UtcNow;

        // 导航属性
        public User User { get; set; } = null!;
        public ICollection<Note> Notes { get; set; } = new List<Note>();
    }
}
