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
                    IsDeleted = n.IsDeleted
                })
                .ToListAsync();

            return notes;
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
