// SmartNote_Shared/DTOs/Note/NotePreviewDto.cs
using System;

namespace SmartNote_Shared.DTOs.Note
{
    // 用于在列表（主页）显示笔记预览，不包含完整内容
    public class NotePreviewDto
    {
        public int Id { get; set; }
        public int WorkspaceId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; } // 摘要 (来自 AI 分析或截断)
        public DateTime LastUpdateTime { get; set; }
    }
}