using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;
using SmartNote.Shared.Results;
using System.Security.Claims;

namespace SmartNote.WebAPI.User.Controllers
{
    /// <summary>
    /// 社区内容相关接口。
    /// </summary>
    [ApiController]
    [Authorize]
    [Route("api/community")]
    public class CommunityController : ControllerBase
    {
        private readonly ICommunityService _service;

        /// <summary>
        /// 初始化社区控制器。
        /// </summary>
        public CommunityController(ICommunityService service)
        {
            _service = service;
        }

        /// <summary>
        /// 获取当前登录用户 Id。
        /// </summary>
        private int GetUserId()
        {
            var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(idStr) || !int.TryParse(idStr, out var id))
                throw new UnauthorizedAccessException("无效的身份标识。");
            return id;
        }

        /// <summary>
        /// 获取已发布内容分页列表。
        /// </summary>
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetPublished(
            [FromQuery] string? keyword,
            [FromQuery] int? contentType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var data = await _service.GetPublishedPageAsync(keyword, contentType, page, pageSize);
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 获取已发布内容详情。
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetDetail(int id)
        {
            var viewerUserId = User.Identity?.IsAuthenticated == true ? GetUserId() : (int?)null;
            var data = await _service.GetPublicContentDetailAsync(id, true, viewerUserId);
            return data == null
                ? NotFound(ApiResponse.Fail("未找到内容"))
                : Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 获取内容评论树。
        /// </summary>
        [AllowAnonymous]
        [HttpGet("{id:int}/comments")]
        public async Task<IActionResult> GetComments(int id)
        {
            var data = await _service.GetCommentsAsync(id);
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 获取我发布的内容（支持状态筛选）。
        /// </summary>
        [HttpGet("mine")]
        public async Task<IActionResult> GetMine([FromQuery] PublicContentStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var data = await _service.GetMyPublicContentsAsync(GetUserId(), status, page, pageSize);
            return Ok(ApiResponse.Success(data));
        }

        /// <summary>
        /// 新增评论。
        /// </summary>
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] PublicCommentCreateDto dto)
        {
            try
            {
                var data = await _service.AddCommentAsync(GetUserId(), dto);
                return Ok(ApiResponse.Success(data, "评论成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 删除评论。
        /// </summary>
        [HttpDelete("comments/{id:int}")]
        public async Task<IActionResult> DeleteComment(int id)
        {
            try
            {
                await _service.DeleteCommentAsync(GetUserId(), id);
                return Ok(ApiResponse.Success("删除成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 克隆内容到指定工作区。
        /// </summary>
        [HttpPost("clone")]
        public async Task<IActionResult> Clone([FromBody] PublicContentCloneRequest request)
        {
            try
            {
                var id = await _service.CloneAsync(GetUserId(), request);
                return Ok(ApiResponse.Success(new { id }, "克隆成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 点赞/收藏切换。
        /// </summary>
        [HttpPost("reaction")]
        public async Task<IActionResult> ToggleReaction([FromBody] PublicContentReactionRequest request)
        {
            try
            {
                await _service.ToggleReactionAsync(GetUserId(), request);
                return Ok(ApiResponse.Success("操作成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 发布内容。
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] PublicContentPublishRequest request)
        {
            try
            {
                var id = await _service.PublishAsync(GetUserId(), request);
                return Ok(ApiResponse.Success(new { id }, "发布成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }

        /// <summary>
        /// 更新内容状态。
        /// </summary>
        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] PublicContentStatusUpdateRequest request)
        {
            try
            {
                await _service.UpdateStatusAsync(GetUserId(), request);
                return Ok(ApiResponse.Success("状态更新成功"));
            }
            catch (BusinessException ex)
            {
                return BadRequest(ApiResponse.Fail(ex.Message));
            }
        }
    }
}
