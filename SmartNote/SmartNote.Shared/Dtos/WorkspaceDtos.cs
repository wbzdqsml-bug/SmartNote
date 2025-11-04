using SmartNote.Domain.Entities.Enums;

namespace SmartNote.Shared.Dtos
{
    // 创建工作区请求模型
    public class WorkspaceCreateDto
    {
        public string Name { get; set; } = string.Empty;
        public WorkspaceType Type { get; set; } = WorkspaceType.Personal;
    }

    // 工作区视图模型（展示给前端）
    public class WorkspaceViewDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public WorkspaceType Type { get; set; }
        public int OwnerUserId { get; set; }
        public DateTime CreateTime { get; set; }
        public int MemberCount { get; set; }
        public int NoteCount { get; set; }
    }

    // 成员信息模型
    public class WorkspaceMemberDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = "Member";
        public DateTime JoinTime { get; set; }
    }
}
