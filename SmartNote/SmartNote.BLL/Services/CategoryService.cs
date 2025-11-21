using Microsoft.EntityFrameworkCore;
using SmartNote.BLL.Abstractions;
using SmartNote.DAL;
using SmartNote.Domain.Entities;
using SmartNote.Domain.Exceptions;
using SmartNote.Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.BLL.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ApplicationDbContext _db;

        public CategoryService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<CategoryDto>> GetUserCategoriesAsync(int userId)
        {
            var list = await _db.Categories
                .AsNoTracking()
                .Where(c => c.UserId == userId)
                .OrderBy(c => c.SortOrder)
                .ThenBy(c => c.Id)
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Color = c.Color,
                    SortOrder = c.SortOrder
                })
                .ToListAsync();

            return list;
        }

        public async Task<int> CreateCategoryAsync(int userId, CategoryCreateRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Name))
                throw new BusinessException("分类名称不能为空。");

            var exists = await _db.Categories
                .AnyAsync(c => c.UserId == userId && c.Name == req.Name);
            if (exists)
                throw new BusinessException("已存在同名分类。");

            var maxOrder = await _db.Categories
                .Where(c => c.UserId == userId)
                .Select(c => (int?)c.SortOrder)
                .MaxAsync() ?? 0;

            var cat = new Category
            {
                UserId = userId,
                Name = req.Name.Trim(),
                Color = req.Color,
                SortOrder = maxOrder + 1,
                CreateTime = DateTime.UtcNow
            };

            _db.Categories.Add(cat);
            await _db.SaveChangesAsync();
            return cat.Id;
        }

        public async Task UpdateCategoryAsync(int userId, int categoryId, CategoryUpdateRequest req)
        {
            var cat = await _db.Categories.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);
            if (cat == null)
                throw new BusinessException("分类不存在。");

            if (string.IsNullOrWhiteSpace(req.Name))
                throw new BusinessException("分类名称不能为空。");

            var nameExists = await _db.Categories.AnyAsync(c =>
                c.UserId == userId && c.Id != categoryId && c.Name == req.Name);
            if (nameExists)
                throw new BusinessException("已存在同名分类。");

            cat.Name = req.Name.Trim();
            cat.Color = req.Color;
            cat.SortOrder = req.SortOrder;

            await _db.SaveChangesAsync();
        }

        public async Task DeleteCategoryAsync(int userId, int categoryId)
        {
            var cat = await _db.Categories
                .Include(c => c.Notes)
                .FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);

            if (cat == null)
                throw new BusinessException("分类不存在。");

            // 删除分类时，将关联笔记的 CategoryId 置空（避免强制删除笔记）
            foreach (var note in cat.Notes)
            {
                note.CategoryId = null;
            }

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
        }
    }
}
