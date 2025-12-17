using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Common.Extensions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;

namespace SmartNote.WebAPI.User.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/ai")]
    public class AiController : ControllerBase
    {
        private readonly IAiNoteService _ai;

        public AiController(IAiNoteService ai)
        {
            _ai = ai;
        }

        [HttpPost("summary")]
        public async Task<IActionResult> Summary([FromBody] AiSummaryRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateSummaryAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }

        [HttpPost("knowledge-extension")]
        public async Task<IActionResult> KnowledgeExtension([FromBody] AiKnowledgeExtensionRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateKnowledgeExtensionAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }

        [HttpPost("text-to-mindmap")]
        public async Task<IActionResult> TextToMindMap([FromBody] AiTextToMindMapRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateMindMapAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }

        [HttpPost("quiz")]
        public async Task<IActionResult> Quiz([FromBody] AiQuizRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateQuizAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }
    }
}

