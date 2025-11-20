using SmartNote.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.BLL.Abstractions
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task UpdateProfileAsync(int userId, UpdateUserProfileRequest req);
    }
}
