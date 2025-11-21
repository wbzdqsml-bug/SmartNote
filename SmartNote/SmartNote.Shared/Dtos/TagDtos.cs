using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Shared.Dtos
{
    public class TagDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    public class TagCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    public class TagUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    /// <summary>
    /// 为某个笔记设置标签时使用
    /// </summary>
    public class NoteTagUpdateRequest
    {
        /// <summary>
        /// 要绑定到该笔记的标签 Id 列表
        /// </summary>
        public List<int> TagIds { get; set; } = new();
    }
}
