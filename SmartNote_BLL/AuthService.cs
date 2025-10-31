// SmartNote_BLL/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartNote_Common;
using SmartNote_DAL;
using SmartNote_Domain;
using SmartNote_Shared.DTOs.User;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote_BLL // (请确保命名空间正确)
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher _passwordHasher;

        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher();
        }

        // --- 注册 (RegisterAsync) ---
        public async Task<string> RegisterAsync(UserRegisterRequestDto request)
        {
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException("用户名已存在。");
            }

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 1. 创建 User
                    var user = new User
                    {
                        Username = request.Username,
                        PasswordHash = _passwordHasher.Hash(request.Password),
                        CreateTime = DateTime.UtcNow
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    // 2. 创建个人工作区
                    var workspace = new Workspace
                    {
                        Name = $"{user.Username}的个人空间",
                        Type = "Personal",
                        OwnerUserId = user.Id,
                        CreateTime = DateTime.UtcNow
                    };
                    _context.Workspaces.Add(workspace);
                    await _context.SaveChangesAsync();

                    // 3. 创建成员关系
                    var member = new WorkspaceMember
                    {
                        UserId = user.Id,
                        WorkspaceId = workspace.Id,
                        Role = "Owner",
                        JoinTime = DateTime.UtcNow
                    };
                    _context.WorkspaceMembers.Add(member);
                    await _context.SaveChangesAsync();

                    // 4. (可选) 创建默认 UserProfile
                    var profile = new UserProfile
                    {
                        UserId = user.Id,
                        Nickname = user.Username,
                        LastUpdateTime = DateTime.UtcNow
                    };
                    _context.UserProfiles.Add(profile);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return "注册成功！";
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw new Exception("注册失败，数据库操作已回滚。", ex);
                }
            }
        }

        // --- 登录 (LoginAsync) ---
        public async Task<LoginSuccessResponseDto> LoginAsync(UserLoginRequestDto request)
        {
            var user = await _context.Users
                .AsNoTracking()
                .Include(u => u.UserProfile)
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            if (user == null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            {
                throw new UnauthorizedAccessException("用户名或密码错误。");
            }

            var personalWorkspace = await _context.Workspaces
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.OwnerUserId == user.Id && w.Type == "Personal");

            var personalWorkspaceId = personalWorkspace?.Id ?? 0;

            var token = GenerateJwtToken(user);

            return new LoginSuccessResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Nickname = user.UserProfile?.Nickname ?? user.Username,
                PersonalWorkspaceId = personalWorkspaceId
            };
        }


        // --- JWT Token 生成辅助方法 ---
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key 未在 appsettings.json 中配置");
            }

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            // VVVVVV (修正) HmacSha26 -> HmacSha256 VVVVVV
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            // ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, user.Username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}