using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 数据分析与统计控制器。
    /// </summary>
    [ApiController]
    [Route("api/analysis")]
    [Authorize]
    public class AnalysisController : ControllerBase
    {
        private readonly IAnalysisService _service;

        public AnalysisController(IAnalysisService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取当前登录用户的 ID。
        /// </summary>
        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        /// <summary>
        /// 获取分类统计数据。
        /// </summary>
        [HttpGet("categories")]
        public async Task<IActionResult> CategoryStats()
        {
            var data = await _service.GetCategoryStatsAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 获取标签统计数据。
        /// </summary>
        [HttpGet("tags")]
        public async Task<IActionResult> TagStats()
        {
            var data = await _service.GetTagStatsAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 获取每日趋势统计。
        /// </summary>
        [HttpGet("trend")]
        public async Task<IActionResult> Trend()
        {
            var data = await _service.GetDailyTrendAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 获取热力图数据。
        /// </summary>
        [HttpGet("heatmap")]
        public async Task<IActionResult> Heatmap()
        {
            var data = await _service.GetHeatmapAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 获取工作区统计数据。
        /// </summary>
        [HttpGet("workspaces")]
        public async Task<IActionResult> WorkspaceStats()
        {
            var data = await _service.GetWorkspaceStatsAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }
    }
}
