using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;


namespace SmartNote.BLL.Services
{
    public class ProfileService : IProfileService
    {
        private readonly ApplicationDbContext _db;

        public ProfileService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _db.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new BusinessException("用户不存在");

            var p = user.Profile ?? new UserProfile();

            return new UserProfileDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = p.Email,
                Phone = p.Phone,
                AvatarUrl = p.AvatarUrl,
                Bio = p.Bio
            };
        }

        public async Task UpdateProfileAsync(int userId, UpdateUserProfileRequest req)
        {
            var user = await _db.Users
                .Include(u => u.Profile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new BusinessException("用户不存在");

            if (user.Profile == null)
            {
                user.Profile = new UserProfile { UserId = userId };
                _db.UserProfiles.Add(user.Profile);
            }

            user.Profile.Email = req.Email;
            user.Profile.Phone = req.Phone;
            user.Profile.AvatarUrl = req.AvatarUrl;
            user.Profile.Bio = req.Bio;
            user.Profile.UpdateTime = DateTime.UtcNow;

            await _db.SaveChangesAsync();
        }
    }
}
