using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_Domain
{
    public class Note
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ContentMd { get; set; } // Markdown 原文
        public string? ContentHtml { get; set; } // 渲染后的 HTML
        public DateTime CreateTime { get; set; }
        public DateTime LastUpdateTime { get; set; }
        // 1-to-Many 关系的外键 (笔记属于哪个工作区)
        public int WorkspaceId { get; set; }
        public virtual Workspace Workspace { get; set; } = null!;
        // (我们将在迭代 5 添加 AI/附件/标签等导航属性)
    }
}
