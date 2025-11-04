using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Domain.Entities.Enums
{
    public enum NoteType
    {
        Text = 0,      // 普通文本笔记
        Markdown = 1,  // Markdown 富文本笔记
        Canvas = 2,    // 画布 / 白板类型
        Mixed = 3      // 图文混合笔记（Markdown + Canvas）
    }
}
