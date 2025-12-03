// SmartNote.BLL/Services/AnalysisService.cs
using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Shared.Dtos.AnalysisDTO;

namespace SmartNote.BLL.Services
{
    public class AnalysisService : IAnalysisService
    {
        private readonly ApplicationDbContext _db;

        public AnalysisService(ApplicationDbContext db)
        {
            _db = db;
        }

        // 当前用户可访问的笔记（用于分类统计）
        private IQueryable<Domain.Entities.Note> GetUserNotes(int userId)
        {
            return _db.Notes
                .AsNoTracking() // 统计只读，使用 AsNoTracking 提升性能
                .Where(n =>
                    n.Workspace.OwnerUserId == userId ||
                    n.Workspace.Members.Any(m => m.UserId == userId)
                )
                .Where(n => !n.IsDeleted);
        }

        // 1. 分类统计
        public async Task<IEnumerable<CategoryStatDto>> GetCategoryStatsAsync(int userId)
        {
            return await GetUserNotes(userId)
                .Where(n => n.CategoryId != null)
                .GroupBy(n => n.Category!)
                .Select(g => new CategoryStatDto
                {
                    CategoryId = g.Key.Id,
                    Name = g.Key.Name,
                    Color = g.Key.Color,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        // 2. 标签统计
        public async Task<IEnumerable<TagStatDto>> GetTagStatsAsync(int userId)
        {
            // 只统计当前用户可访问工作区中的标签使用情况
            var workspaceIds = await _db.WorkspaceMembers
                .Where(m => m.UserId == userId)
                .Select(m => m.WorkspaceId)
                .Union(
                    _db.Workspaces
                        .Where(w => w.OwnerUserId == userId)
                        .Select(w => w.Id)
                )
                .Distinct()
                .ToListAsync();

            return await _db.NoteTags
                .AsNoTracking()
                .Where(nt => nt.Tag.UserId == userId &&
                             nt.Note.WorkspaceId != 0 &&
                             workspaceIds.Contains(nt.Note.WorkspaceId) &&
                             !nt.Note.IsDeleted)
                .GroupBy(nt => nt.Tag)
                .Select(g => new TagStatDto
                {
                    TagId = g.Key.Id,
                    Name = g.Key.Name,
                    Color = g.Key.Color,
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Count)
                .ToListAsync();
        }

        // 3. 学习趋势（按日志统计：Create / Update）
        public async Task<IEnumerable<DailyTrendDto>> GetDailyTrendAsync(int userId)
        {
            var logs = await _db.NoteActivityLogs
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .ToListAsync();

            // ⚠️ 修复：匹配字符串必须与 NoteActivityType 枚举 ToString() 保持一致 ("Created", "Updated")
            return logs
                .GroupBy(l => l.Time.Date)
                .Select(g => new DailyTrendDto
                {
                    Date = g.Key,
                    Created = g.Count(l => l.Action == "Created"),
                    Updated = g.Count(l => l.Action == "Updated" || l.Action == "TagUpdated") // 可选：把标签更新也算作活跃
                })
                .OrderBy(x => x.Date)
                .ToList();
        }

        // 4. 学习活动热力图（按日志次数）
        public async Task<IEnumerable<DailyHeatmapDto>> GetHeatmapAsync(int userId)
        {
            // ✅ 先在数据库里按日期 group by，拿到 DateTime + Count
            var raw = await _db.NoteActivityLogs
                .AsNoTracking()
                .Where(l => l.UserId == userId)
                .GroupBy(l => l.Time.Date)
                .Select(g => new
                {
                    Date = g.Key,         // DateTime
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();

            // ✅ 再在内存中做 ToString("yyyy-MM-dd")
            return raw
                .Select(x => new DailyHeatmapDto
                {
                    Date = x.Date.ToString("yyyy-MM-dd"),
                    Count = x.Count
                })
                .ToList();
        }

        // 5. 工作区占比（按当前笔记数量）
        public async Task<IEnumerable<WorkspaceStatDto>> GetWorkspaceStatsAsync(int userId)
        {
            var workspaces = await _db.Workspaces
                .AsNoTracking()
                .Where(w =>
                    w.OwnerUserId == userId ||
                    w.Members.Any(m => m.UserId == userId)
                )
                .Select(w => new WorkspaceStatDto
                {
                    WorkspaceId = w.Id,
                    Name = w.Name,
                    Count = w.Notes.Count(n => !n.IsDeleted)
                })
                .ToListAsync();

            var total = workspaces.Sum(w => w.Count);
            foreach (var w in workspaces)
            {
                w.Percentage = total == 0
                    ? 0
                    : Math.Round((double)w.Count / total * 100, 1);
            }

            return workspaces
                .OrderByDescending(w => w.Count)
                .ToList();
        }
    }
}