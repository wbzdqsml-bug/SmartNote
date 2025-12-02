// 文件位置建议：SmartNote.BLL/Abstractions/IAnalysisService.cs
using SmartNote.Shared.Dtos.AnalysisDTO;

namespace SmartNote.BLL.Abstractions
{
    public interface IAnalysisService
    {
        /// <summary>
        /// 按分类统计笔记数量
        /// </summary>
        Task<IEnumerable<CategoryStatDto>> GetCategoryStatsAsync(int userId);

        /// <summary>
        /// 按标签统计笔记数量
        /// </summary>
        Task<IEnumerable<TagStatDto>> GetTagStatsAsync(int userId);

        /// <summary>
        /// 每日创建 / 更新趋势
        /// </summary>
        Task<IEnumerable<DailyTrendDto>> GetDailyTrendAsync(int userId);

        /// <summary>
        /// 学习活动热力图（每天总操作次数）
        /// </summary>
        Task<IEnumerable<DailyHeatmapDto>> GetHeatmapAsync(int userId);

        /// <summary>
        /// 工作区笔记数量与占比
        /// </summary>
        Task<IEnumerable<WorkspaceStatDto>> GetWorkspaceStatsAsync(int userId);
    }
}
