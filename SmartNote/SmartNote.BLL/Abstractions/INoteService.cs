using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface INoteService
    {
        /// <summary>
        /// 获取用户可看到的所有笔记（完整数据：分类 + 标签）
        /// </summary>
        Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId);

        /// <summary>
        /// 创建笔记（支持分类 + 初始标签）
        /// </summary>
        Task<int> CreateNoteAsync(int userId, NoteCreateDto dto);

        /// <summary>
        /// 更新笔记（标题、内容、分类）
        /// </summary>
        Task<int> UpdateNoteAsync(int noteId, int userId, NoteUpdateDto dto);

        /// <summary>
        /// 软删除笔记（移动到回收站）
        /// </summary>
        Task<int> SoftDeleteAsync(IEnumerable<int> noteIds, int userId);

        /// <summary>
        /// 根据分类和标签筛选笔记
        /// categoryId = null 表示不筛选分类
        /// tagIds = null 表示不筛选标签
        /// </summary>
        Task<IEnumerable<NoteViewDto>> FilterNotesAsync(
            int userId,
            int? categoryId,
            IReadOnlyList<int>? tagIds);

        /// <summary>
        /// 获取笔记（完整信息，包括分类和标签）
        /// </summary>
        Task<NoteViewDto?> GetNoteByIdAsync(int userId, int noteId);

        /// <summary>
        /// 更新笔记的标签（NoteTag 多对多关系）
        /// </summary>
        Task UpdateNoteTagsAsync(int userId, int noteId, List<int> tagIds);
    }
}
