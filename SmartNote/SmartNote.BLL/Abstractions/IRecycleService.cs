using SmartNote.Shared.Dtos;
using SmartNote.Shared.Dtos.SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IRecycleService
    {
        /// <summary>
        /// 获取当前用户回收站中的笔记
        /// </summary>
        Task<IEnumerable<NoteViewDto>> GetDeletedNotesAsync(int userId);

        /// <summary>
        /// 恢复已删除笔记
        /// </summary>
        Task<int> RestoreNotesAsync(IEnumerable<int> noteIds, int userId);

        /// <summary>
        /// 永久删除笔记（物理删除）
        /// </summary>
        Task<int> PermanentlyDeleteAsync(IEnumerable<int> noteIds, int userId);
    }
}
