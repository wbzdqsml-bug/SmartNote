using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Shared.DTOs.Note
{
    // 这个 DTO 用于多种场景：创建、更新、返回
    public class NoteDto
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "工作区 ID 不能为空")]
        public int WorkspaceId { get; set; } // 笔记归属的工作区

        [Required(ErrorMessage = "标题不能为空")]
        [StringLength(200, ErrorMessage = "标题不能超过 200 字符")]
        public string Title { get; set; } = string.Empty;

        public string? ContentMd { get; set; }

        // (可选) 用于列表预览的摘要
        public string? Summary { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}