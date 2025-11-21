using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Shared.Dtos
{
    /// <summary>
    /// 创建笔记 DTO
    /// </summary>
    public class NoteCreateDto
    {
        /// <summary>
        /// 所属工作区
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// 笔记标题
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// 笔记类型（Markdown / Canvas / MindMap / RichText）
        /// </summary>
        public NoteType Type { get; set; } = NoteType.Markdown;

        /// <summary>
        /// 分类（可选，可在创建后修改）
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 初始标签（可选，可在创建后修改）
        /// </summary>
        public List<int>? TagIds { get; set; }
    }

    /// <summary>
    /// 更新笔记 DTO
    /// </summary>
    public class NoteUpdateDto
    {

        /// <summary>
        /// 标题（可选）
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 内容 JSON（由前端直接传完整内容结构）
        /// </summary>
        public string? ContentJson { get; set; }

        /// <summary>
        /// 分类（可选，null 表示不更新）
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 标签更新说明（前端若更新标签，使用 Tag API，而不是这里）
        /// </summary>
        public List<int>? TagIds { get; set; }
    }


    /// <summary>
    /// 笔记视图 DTO（输出到前端）
    /// </summary>
    public class NoteViewDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public NoteType Type { get; set; } = NoteType.Markdown;

        /// <summary>
        /// 完整 JSON 内容
        /// </summary>
        public string ContentJson { get; set; } = "{}";

        /// <summary>
        /// 所属工作区
        /// </summary>
        public int WorkspaceId { get; set; }

        /// <summary>
        /// 分类 ID（可为空）
        /// </summary>
        public int? CategoryId { get; set; }

        /// <summary>
        /// 分类显示名
        /// </summary>
        public string? CategoryName { get; set; }

        /// <summary>
        /// 分类颜色（用于前端显示小圆点）
        /// </summary>
        public string? CategoryColor { get; set; }

        /// <summary>
        /// 标签列表（用于前端 chips）
        /// </summary>
        public List<TagDto> Tags { get; set; } = new();

        /// <summary>
        /// 是否处于回收站
        /// </summary>
        public bool IsDeleted { get; set; }

        public DateTime? DeletedTime { get; set; }

        public DateTime CreateTime { get; set; }

        public DateTime LastUpdateTime { get; set; }
    }
}
