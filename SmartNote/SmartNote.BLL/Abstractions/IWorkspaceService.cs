using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IWorkspaceService
    {
        Task<WorkspaceViewDto> CreateWorkspaceAsync(int userId, WorkspaceCreateDto dto);
        Task<IEnumerable<WorkspaceViewDto>> GetUserWorkspacesAsync(int userId);
        Task<WorkspaceViewDto?> GetWorkspaceDetailAsync(int workspaceId, int userId);
        Task<bool> DeleteWorkspaceAsync(int workspaceId, int userId, bool forceDelete = false);
    }
}
