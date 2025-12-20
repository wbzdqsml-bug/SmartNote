using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Enums;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Services
{
    public class FriendService : IFriendService
    {
        private readonly ApplicationDbContext _db;

        public FriendService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<List<FriendDto>> GetMyFriendsAsync(int userId)
        {
            return await _db.Friendships
                .Where(f => (f.RequesterId == userId || f.AddresseeId == userId) && f.Status == FriendshipStatus.Accepted)
                .Select(f => new FriendDto(
                    f.Id,
                    f.RequesterId == userId ? f.AddresseeId : f.RequesterId,
                    f.RequesterId == userId ? f.Addressee.Username : f.Requester.Username,
                    f.RequesterId == userId 
                        ? (f.Addressee.Profile != null ? f.Addressee.Profile.AvatarUrl : null) 
                        : (f.Requester.Profile != null ? f.Requester.Profile.AvatarUrl : null)
                ))
                .ToListAsync();
        }

        public async Task SendRequestAsync(int userId, string targetUsername)
        {
            var targetUser = await _db.Users.FirstOrDefaultAsync(u => u.Username == targetUsername);
            
            if (targetUser == null) throw new BusinessException("用户不存在");
            if (targetUser.Id == userId) throw new BusinessException("不能添加自己为好友");

            var exists = await _db.Friendships.AnyAsync(f => 
                (f.RequesterId == userId && f.AddresseeId == targetUser.Id) ||
                (f.RequesterId == targetUser.Id && f.AddresseeId == userId));

            if (exists) throw new BusinessException("已有好友关系或申请中");

            var friendship = new Friendship
            {
                RequesterId = userId,
                AddresseeId = targetUser.Id,
                Status = FriendshipStatus.Pending,
                RequestTime = DateTime.UtcNow
            };

            _db.Friendships.Add(friendship);
            await _db.SaveChangesAsync();
        }

        public async Task<List<FriendRequestDto>> GetRequestsAsync(int userId)
        {
            return await _db.Friendships
                .Where(f => f.AddresseeId == userId && f.Status == FriendshipStatus.Pending)
                .Select(f => new FriendRequestDto(
                    f.Id,
                    f.RequesterId,
                    f.Requester.Username,
                    f.RequestTime
                ))
                .ToListAsync();
        }

        public async Task HandleRequestAsync(int userId, int requestId, string decision)
        {
            var friendship = await _db.Friendships.FindAsync(requestId);

            if (friendship == null) throw new BusinessException("申请不存在");
            if (friendship.AddresseeId != userId) throw new BusinessException("无权处理此申请");

            if (decision.ToLower() == "accept")
            {
                friendship.Status = FriendshipStatus.Accepted;
                friendship.ResponseTime = DateTime.UtcNow;
            }
            else if (decision.ToLower() == "reject")
            {
                friendship.Status = FriendshipStatus.Rejected;
                friendship.ResponseTime = DateTime.UtcNow;
            }
            else
            {
                throw new BusinessException("无效的操作");
            }
            await _db.SaveChangesAsync();
        }
    }
}
