using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface INoteService
    {
        Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId);

        Task<NoteViewDto?> GetNoteByIdAsync(int userId, int noteId);

        Task<IEnumerable<NoteViewDto>> FilterNotesAsync(
            int userId,
            int? categoryId,
            IReadOnlyList<int>? tagIds);

        Task<int> CreateNoteAsync(int userId, NoteCreateDto dto);

        Task<int> UpdateNoteAsync(int noteId, int userId, NoteUpdateDto dto);

        Task UpdateNoteTagsAsync(int userId, int noteId, List<int> tagIds);

        Task<int> SoftDeleteAsync(IEnumerable<int> noteIds, int userId);
    }
}
