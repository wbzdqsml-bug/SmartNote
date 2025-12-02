using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
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

        private int GetUserId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> CategoryStats()
        {
            var data = await _service.GetCategoryStatsAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        [HttpGet("tags")]
        public async Task<IActionResult> TagStats()
        {
            var data = await _service.GetTagStatsAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        [HttpGet("trend")]
        public async Task<IActionResult> Trend()
        {
            var data = await _service.GetDailyTrendAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        [HttpGet("heatmap")]
        public async Task<IActionResult> Heatmap()
        {
            var data = await _service.GetHeatmapAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }

        [HttpGet("workspaces")]
        public async Task<IActionResult> WorkspaceStats()
        {
            var data = await _service.GetWorkspaceStatsAsync(GetUserId());
            return Ok(ApiResponse.Success(data));
        }
    }
}
