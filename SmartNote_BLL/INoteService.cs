// SmartNote_BLL/INoteService.cs
using SmartNote_Shared.DTOs.Note;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SmartNote_BLL // (确保命名空间与你的 BLL 项目名一致)
{
    public interface INoteService
    {
        /// <summary>
        /// 获取当前登录用户有权访问的所有笔记列表（预览）
        /// (迭代 1 中只获取个人笔记)
        /// </summary>
        /// <param name="userId">当前登录用户的 ID</param>
        /// <returns>笔记预览 DTO 列表</returns>
        Task<List<NotePreviewDto>> GetNotesForUserAsync(int userId);

        /// <summary>
        /// 获取单篇笔记的详细内容（包含权限检查）
        /// </summary>
        /// <param name="noteId">笔记 ID</param>
        /// <param name="userId">当前登录用户的 ID</param>
        /// <returns>笔记详情 DTO</returns>
        Task<NoteDto> GetNoteByIdAsync(int noteId, int userId);

        /// <summary>
        /// 在指定工作区创建一篇新笔记（包含权限检查）
        /// </summary>
        /// <param name="createDto">创建笔记的 DTO</param>
        /// <param name="userId">当前登录用户的 ID</param>
        /// <returns>创建成功的新笔记 DTO</returns>
        Task<NoteDto> CreateNoteAsync(NoteCreateDto createDto, int userId);

        /// <summary>
        /// 更新一篇笔记（包含权限检查）
        /// </summary>
        /// <param name="updateDto">更新笔记的 DTO</param>
        /// <param name="userId">当前登录用户的 ID</param>
        /// <returns>更新后的笔记 DTO</returns>
        Task<NoteDto> UpdateNoteAsync(NoteUpdateDto updateDto, int userId);

        /// <summary>
        /// 删除一篇笔记（包含权限检查）
        /// </summary>
        /// <param name="noteId">笔记 ID</param>
        /// <param name="userId">当前登录用户的 ID</param>
        Task DeleteNoteAsync(int noteId, int userId);
    }
}