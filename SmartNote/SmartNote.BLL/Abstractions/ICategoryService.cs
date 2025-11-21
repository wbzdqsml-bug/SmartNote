using SmartNote.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.BLL.Abstractions
{
    public interface ICategoryService
    {
        Task<IReadOnlyList<CategoryDto>> GetUserCategoriesAsync(int userId);
        Task<int> CreateCategoryAsync(int userId, CategoryCreateRequest req);
        Task UpdateCategoryAsync(int userId, int categoryId, CategoryUpdateRequest req);
        Task DeleteCategoryAsync(int userId, int categoryId);
    }
}
