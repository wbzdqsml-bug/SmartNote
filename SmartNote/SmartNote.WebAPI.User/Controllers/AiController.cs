﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Common.Extensions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// AI 辅助功能控制器。
    /// </summary>
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

        /// <summary>
        /// 生成内容摘要。
        /// </summary>
        [HttpPost("summary")]
        public async Task<IActionResult> Summary([FromBody] AiSummaryRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateSummaryAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }

        /// <summary>
        /// 生成知识扩展。
        /// </summary>
        [HttpPost("knowledge-extension")]
        public async Task<IActionResult> KnowledgeExtension([FromBody] AiKnowledgeExtensionRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateKnowledgeExtensionAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }

        /// <summary>
        /// 将文本转换为思维导图结构。
        /// </summary>
        [HttpPost("text-to-mindmap")]
        public async Task<IActionResult> TextToMindMap([FromBody] AiTextToMindMapRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateMindMapAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }

        /// <summary>
        /// 生成测验题。
        /// </summary>
        [HttpPost("quiz")]
        public async Task<IActionResult> Quiz([FromBody] AiQuizRequest request)
        {
            var userId = User.GetUserId();
            var res = await _ai.GenerateQuizAsync(userId, request);
            return Ok(ApiResponse.Success(res));
        }
    }
}
