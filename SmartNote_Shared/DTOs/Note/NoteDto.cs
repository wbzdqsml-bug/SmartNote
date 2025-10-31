// SmartNote_Shared/DTOs/Note/NoteDto.cs
using System;

namespace SmartNote_Shared.DTOs.Note // (请确保命名空间正确)
{
    // 用于获取单篇笔记的完整内容
    public class NoteDto
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? ContentMd { get; set; } // Markdown 原文

        // VVVVVV 补全缺失的属性 VVVVVV
        public string? ContentHtml { get; set; } // 渲染后的 HTML
        public DateTime CreateTime { get; set; }
        // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        public DateTime LastUpdateTime { get; set; }
    }
}