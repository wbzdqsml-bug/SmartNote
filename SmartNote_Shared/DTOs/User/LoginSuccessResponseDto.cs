using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// SmartNote.Shared/DTOs/User/LoginSuccessResponseDto.cs
namespace SmartNote.Shared.DTOs.User
{
    public class LoginSuccessResponseDto
    {
        public string Token { get; set; } = string.Empty; // JWT Token
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        // (未来可以添加工作区列表等信息)
    }
}