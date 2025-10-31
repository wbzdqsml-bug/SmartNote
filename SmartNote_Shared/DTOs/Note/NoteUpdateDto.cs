// SmartNote_Shared/DTOs/Note/NoteUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace SmartNote_Shared.DTOs.Note
{
    public class NoteUpdateDto
    {
        [Required]
        public int Id { get; set; } // 必须指定要更新的笔记 ID

        [Required(ErrorMessage = "标题不能为空")]
        [StringLength(200, ErrorMessage = "标题不能超过 200 字符")]
        public string Title { get; set; } = string.Empty;

        public string? ContentMd { get; set; }
    }
}