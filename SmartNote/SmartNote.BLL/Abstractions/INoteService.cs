using SmartNote.Shared.Dtos;
using System.IO;

namespace SmartNote.BLL.Abstractions
{
    /// <summary>
    /// 笔记相关业务接口
    /// </summary>
    public interface INoteService
    {
        /// <summary>
        /// 获取当前用户可访问的所有笔记（完整信息：分类 + 标签）
        /// </summary>
        Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId);

        /// <summary>
        /// 按 ID 获取单条笔记详情（含分类 + 标签）
        /// </summary>
        Task<NoteViewDto?> GetNoteByIdAsync(int userId, int noteId);

        /// <summary>
        /// 按分类 / 标签筛选笔记
        /// </summary>
        Task<IEnumerable<NoteViewDto>> FilterNotesAsync(
            int userId,
            int? categoryId,
            IReadOnlyList<int>? tagIds);

        /// <summary>
        /// 创建新笔记（可带初始分类和标签）
        /// </summary>
        Task<int> CreateNoteAsync(int userId, NoteCreateDto dto);

        /// <summary>
        /// 从文件导入笔记
        /// </summary>
        Task<int> ImportNoteAsync(int userId, int workspaceId, string fileName, Stream fileStream);

        /// <summary>
        /// 更新笔记内容 / 标题 / 分类（不负责标签）
        /// </summary>
        Task<int> UpdateNoteAsync(int noteId, int userId, NoteUpdateDto dto);

        /// <summary>
        /// 覆盖式更新某笔记的标签（编辑页用）
        /// </summary>
        Task UpdateNoteTagsAsync(int userId, int noteId, List<int> tagIds);

        /// <summary>
        /// 批量软删除笔记（移动到回收站）
        /// </summary>
        Task<int> SoftDeleteAsync(IEnumerable<int> noteIds, int userId);
    }
}
