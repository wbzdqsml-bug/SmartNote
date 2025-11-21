using SmartNote.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.BLL.Abstractions
{
    public interface ITagService
    {
        Task<IReadOnlyList<TagDto>> GetUserTagsAsync(int userId);
        Task<int> CreateTagAsync(int userId, TagCreateRequest req);
        Task UpdateTagAsync(int userId, int tagId, TagUpdateRequest req);
        Task DeleteTagAsync(int userId, int tagId);

        /// <summary>
        /// 获取某个笔记当前的标签列表
        /// </summary>
        Task<IReadOnlyList<TagDto>> GetNoteTagsAsync(int userId, int noteId);

        /// <summary>
        /// 为某个笔记设置标签（覆盖式设置）
        /// </summary>
        Task UpdateNoteTagsAsync(int userId, int noteId, IReadOnlyList<int> tagIds);
    }
}
