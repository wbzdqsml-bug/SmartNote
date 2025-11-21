using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartNote.Shared.Dtos
{
    public class CategoryDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int SortOrder { get; set; }
    }

    public class CategoryCreateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
    }

    public class CategoryUpdateRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Color { get; set; }
        public int SortOrder { get; set; }
    }
}
