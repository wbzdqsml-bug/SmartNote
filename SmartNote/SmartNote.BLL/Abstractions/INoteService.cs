using SmartNote.Shared.Dtos;


namespace SmartNote.BLL.Abstractions
{
    public interface INoteService
    {
        Task<IEnumerable<NoteViewDto>> GetUserNotesAsync(int userId);
        Task<int> CreateNoteAsync(int userId, NoteCreateDto dto);
        Task<int> UpdateNoteAsync(int noteId, int userId, NoteUpdateDto dto);
        Task<int> SoftDeleteAsync(IEnumerable<int> noteIds, int userId);
    }
}
