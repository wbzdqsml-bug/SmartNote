using Microsoft.AspNetCore.Mvc;
using SmartNote.BLL.Abstractions;
using SmartNote.Shared.Dtos;

namespace SmartNote.WebAPI.Admin.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvitationController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public InvitationController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("ping")]
        public async Task<ActionResult<AdminPingResult>> Ping()
        {
            var res = await _adminService.PingAsync();
            return Ok(res with { Area = "invitation" });
        }
    }
}
