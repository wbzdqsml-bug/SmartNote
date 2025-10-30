// SmartNote_BLL/AuthService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration; // (需要 NuGet: Microsoft.Extensions.Configuration)
using Microsoft.IdentityModel.Tokens;     // (需要 NuGet: Microsoft.IdentityModel.Tokens)
using SmartNote.Shared.DTOs.User;
using SmartNote_Common;                   // (引用 Common 项目)
using SmartNote_DAL;                      // (引用 DAL 项目)
using SmartNote_Domain;                   // (引用 Domain 项目)
using SmartNote_Shared.DTOs.User;         // (引用 Shared 项目)
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using SmartNote_BLL;

namespace SmartNote_BLL
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly PasswordHasher _passwordHasher;

        // 1. 注入 DbContext, Configuration 和我们刚创建的 Hasher
        public AuthService(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
            _passwordHasher = new PasswordHasher(); // (在 BLL 中, 依赖注入 PasswordHasher 更好)
        }

        // (更好的方式：通过 DI 注入 PasswordHasher)
        // public AuthService(ApplicationDbContext context, IConfiguration configuration, PasswordHasher hasher)
        // {
        //     _context = context;
        //     _configuration = configuration;
        //     _passwordHasher = hasher;
        // }
        // (我们先用 new，稍后可以在 Program.cs 中注册 Hasher)


        // --- 实现注册 (RegisterAsync) ---
        public async Task<string> RegisterAsync(UserRegisterRequestDto request)
        {
            // 1. 检查用户名是否已存在
            if (await _context.Users.AnyAsync(u => u.Username == request.Username))
            {
                throw new InvalidOperationException("用户名已存在。");
            }

            // 2. 使用数据库事务 (Transaction)
            // 确保“创建 User”和“创建 Workspace”要么同时成功，要么同时失败
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // 3. 创建 User 实体
                    var user = new User
                    {
                        Username = request.Username,
                        PasswordHash = _passwordHasher.Hash(request.Password),
                        CreateTime = DateTime.UtcNow
                    };
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync(); // (必须先保存 User 才能拿到 user.Id)

                    // 4. 创建对应的个人工作区 (Personal Workspace)
                    var workspace = new Workspace
                    {
                        Name = $"{user.Username}的个人空间",
                        Type = "Personal",
                        OwnerUserId = user.Id,
                        CreateTime = DateTime.UtcNow
                    };
                    _context.Workspaces.Add(workspace);
                    await _context.SaveChangesAsync(); // (保存 Workspace 才能拿到 workspace.Id)

                    // 5. 创建成员关系 (User <-> Workspace)
                    var member = new WorkspaceMember
                    {
                        UserId = user.Id,
                        WorkspaceId = workspace.Id,
                        Role = "Owner", // 用户是自己个人空间的 Owner
                        JoinTime = DateTime.UtcNow
                    };
                    _context.WorkspaceMembers.Add(member);
                    await _context.SaveChangesAsync();

                    // 6. 提交事务
                    await transaction.CommitAsync();

                    return "注册成功！";
                }
                catch (Exception ex)
                {
                    // 7. 如果任何一步失败，回滚所有操作
                    await transaction.RollbackAsync();
                    throw new Exception("注册失败，数据库操作已回滚。", ex);
                }
            }
        }


        // --- 实现登录 (LoginAsync) ---
        public async Task<LoginSuccessResponseDto> LoginAsync(UserLoginRequestDto request)
        {
            // 1. 查找用户
            var user = await _context.Users
                .Include(u => u.UserProfile) // (关键) 预加载 UserProfile
                .FirstOrDefaultAsync(u => u.Username == request.Username);

            // 2. 验证用户是否存在以及密码是否正确
            if (user == null || !_passwordHasher.Verify(user.PasswordHash, request.Password))
            {
                throw new UnauthorizedAccessException("用户名或密码错误。");
            }

            // 3. 验证通过，生成 JWT Token
            var token = GenerateJwtToken(user);

            // 4. 返回 DTO
            return new LoginSuccessResponseDto
            {
                Token = token,
                UserId = user.Id,
                Username = user.Username,
                Nickname = user.UserProfile?.Nickname ?? user.Username // 优先用昵称，没有则用用户名
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
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            // 2. 创建 Claims (Token 中携带的数据)
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()), // Subject (用户ID)
                new Claim(JwtRegisteredClaimNames.Name, user.Username),     // (用户名)
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) // (Token 唯一 ID)
                // (未来可以在这里添加 Role)
                // new Claim(ClaimTypes.Role, "Admin")
            };

            // 3. 创建 Token
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // Token 过期时间 (8 小时)
                signingCredentials: credentials);

            // 4. 序列化 Token
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}