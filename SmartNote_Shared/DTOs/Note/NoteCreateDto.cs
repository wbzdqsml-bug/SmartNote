// SmartNote_Shared/DTOs/Note/NoteCreateDto.cs
using System.ComponentModel.DataAnnotations;

namespace SmartNote_Shared.DTOs.Note
{
    public class NoteCreateDto
    {
        [Required(ErrorMessage = "工作区 ID 不能为空")]
        public int WorkspaceId { get; set; } // 必须指定在哪创建

        [Required(ErrorMessage = "标题不能为空")]
        [StringLength(200, ErrorMessage = "标题不能超过 200 字符")]
        public string Title { get; set; } = string.Empty;

        public string? ContentMd { get; set; } // 内容可以为空
    }
}