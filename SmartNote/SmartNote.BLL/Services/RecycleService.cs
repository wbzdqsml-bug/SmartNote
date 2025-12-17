using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Shared.Dtos;


namespace SmartNote.BLL.Services
{
    public class RecycleService : IRecycleService
    {
        private readonly ApplicationDbContext _db;

        public RecycleService(ApplicationDbContext db)
        {
            _db = db;
        }

        private static NoteViewDto MapToDto(Domain.Entities.Note n)
        {
            return new NoteViewDto
            {
                Id = n.Id,
                Title = n.Title,
                Type = n.Type,
                ContentJson = n.ContentJson,
                WorkspaceId = n.WorkspaceId,
                CreateTime = n.CreateTime,
                LastUpdateTime = n.LastUpdateTime,
                IsDeleted = n.IsDeleted,
                DeletedTime = n.DeletedTime,

                CategoryId = n.CategoryId,
                CategoryName = n.Category?.Name,
                CategoryColor = n.Category?.Color,

                Tags = n.NoteTags?.Select(nt => new TagDto
                {
                    Id = nt.Tag.Id,
                    Name = nt.Tag.Name,
                    Color = nt.Tag.Color
                }).ToList() ?? new List<TagDto>()
            };
        }

        public async Task<IEnumerable<NoteViewDto>> GetDeletedNotesAsync(int userId)
        {
            var notes = await _db.Notes
                .IgnoreQueryFilters()
                .Where(n => n.IsDeleted && n.Workspace.OwnerUserId == userId)
                .OrderByDescending(n => n.DeletedTime ?? n.LastUpdateTime)
                .Select(n => new NoteViewDto
                {
                    Id = n.Id,
                    Title = n.Title,
                    LastUpdateTime = n.LastUpdateTime,
                    WorkspaceId = n.WorkspaceId,
                    IsDeleted = n.IsDeleted,
                    DeletedTime = n.DeletedTime
                })
                .ToListAsync();

            return notes;
        }

        public async Task<NoteViewDto?> GetDeletedNoteByIdAsync(int userId, int noteId)
        {
            var note = await _db.Notes
                .IgnoreQueryFilters()
                .Include(n => n.Category)
                .Include(n => n.NoteTags).ThenInclude(nt => nt.Tag)
                .FirstOrDefaultAsync(n => n.Id == noteId && n.IsDeleted && n.Workspace.OwnerUserId == userId);

            return note == null ? null : MapToDto(note);
        }

        public async Task<int> RestoreNotesAsync(IEnumerable<int> noteIds, int userId)
        {
            var notes = await _db.Notes
                .IgnoreQueryFilters()
                .Where(n => noteIds.Contains(n.Id) && n.IsDeleted && n.Workspace.OwnerUserId == userId)
                .ToListAsync();

            foreach (var note in notes)
            {
                note.IsDeleted = false;
                note.DeletedTime = null;
                note.LastUpdateTime = DateTime.UtcNow;
            }

            return await _db.SaveChangesAsync();
        }

        public async Task<int> PermanentlyDeleteAsync(IEnumerable<int> noteIds, int userId)
        {
            var notes = await _db.Notes
                .IgnoreQueryFilters()
                .Where(n => noteIds.Contains(n.Id) && n.IsDeleted && n.Workspace.OwnerUserId == userId)
                .ToListAsync();

            if (!notes.Any()) return 0;

            _db.Notes.RemoveRange(notes);
            return await _db.SaveChangesAsync();
        }
    }
}
