// 文件位置建议：SmartNote.Shared/Dtos/AnalysisDTO/AnalysisDtos.cs
namespace SmartNote.Shared.Dtos.AnalysisDTO
{
    /// <summary>
    /// 分类统计 DTO
    /// </summary>
    public class CategoryStatDto
    {
        public int CategoryId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }

        /// <summary>
        /// 该分类下的笔记数量
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 标签统计 DTO
    /// </summary>
    public class TagStatDto
    {
        public int TagId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Color { get; set; }

        /// <summary>
        /// 使用该标签的笔记数量
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 每日创建/更新趋势 DTO
    /// </summary>
    public class DailyTrendDto
    {
        /// <summary>
        /// 日期（后端用 DateTime，前端自己格式化即可）
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// 当天创建的笔记数
        /// </summary>
        public int Created { get; set; }

        /// <summary>
        /// 当天更新的笔记数
        /// </summary>
        public int Updated { get; set; }
    }

    /// <summary>
    /// 热力图 DTO（日期 + 操作次数）
    /// </summary>
    public class DailyHeatmapDto
    {
        /// <summary>
        /// 日期字符串，格式：yyyy-MM-dd
        /// </summary>
        public string Date { get; set; } = string.Empty;

        /// <summary>
        /// 当天的学习/笔记活跃次数
        /// </summary>
        public int Count { get; set; }
    }

    /// <summary>
    /// 工作区统计 DTO
    /// </summary>
    public class WorkspaceStatDto
    {
        public int WorkspaceId { get; set; }

        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 该工作区下的笔记数量（未删除）
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 笔记占比（0 ~ 100，保留一位小数）
        /// </summary>
        public double Percentage { get; set; }
    }
}
