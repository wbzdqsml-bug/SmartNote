using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SmartNote.BLL.Abstractions;
using SmartNote.Common.Helpers;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Entities.Enums;
using SmartNote.Shared.Dtos;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SmartNote.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly IConfiguration _config;
        private readonly IDistributedCache _cache;

        public AuthService(ApplicationDbContext db, IConfiguration config, IDistributedCache cache)
        {
            _db = db;
            _config = config;
            _cache = cache;
        }

        public async Task<bool> RegisterAsync(RegisterRequest request)
        {
            // 1) 用户名重复校验（唯一性）
            if (await _db.Users.AnyAsync(u => u.Username == request.Username))
                return false;

            await using var tx = await _db.Database.BeginTransactionAsync();
            try
            {
                // 2) 创建用户
                var user = new User
                {
                    Username = request.Username.Trim(),
                    PasswordHash = PasswordHasher.Hash(request.Username, request.Password),
                    CreateTime = DateTime.UtcNow
                };
                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // 3) 创建个人工作区
                var ws = new Workspace
                {
                    Name = $"{user.Username}的个人空间",
                    Type = WorkspaceType.Personal,
                    OwnerUserId = user.Id,
                    CreateTime = DateTime.UtcNow
                };
                _db.Workspaces.Add(ws);
                await _db.SaveChangesAsync();

                // 4) 成员关系：Owner
                _db.WorkspaceMembers.Add(new WorkspaceMember
                {
                    WorkspaceId = ws.Id,
                    UserId = user.Id,
                    Role = WorkspaceRole.Owner,
                    JoinTime = DateTime.UtcNow
                });
                await _db.SaveChangesAsync();

                await tx.CommitAsync();
                return true;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<LoginResponse?> LoginAsync(LoginRequest request)
        {
            // 单次读取（AsNoTracking 减少跟踪开销）
            var user = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Username == request.Username.Trim());

            if (user is null) return null;

            var ok = PasswordHasher.Verify(request.Username, request.Password, user.PasswordHash);
            if (!ok) return null;

            // 读取 JWT 配置
            var key = _config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
            var issuer = _config["Jwt:Issuer"] ?? "SmartNote.UserAPI";
            var audience = _config["Jwt:Audience"] ?? "SmartNoteClient";

            // 使用 JwtHelper 生成 Token
            var (token, expiresIn) = JwtHelper.GenerateToken(
                userId: user.Id,
                username: user.Username,
                role: "User",
                key: key,
                issuer: issuer,
                audience: audience,
                lifetime: TimeSpan.FromHours(1));

            // 缓存 token（key 可按需扩展到 deviceId 维度）
            var cacheKey = $"token:{user.Id}";
            await _cache.SetStringAsync(
                cacheKey,
                token,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(expiresIn)
                });

            return new LoginResponse
            {
                Token = token,
                Username = user.Username,
                ExpiresInSeconds = expiresIn
            };
        }
    }
}
