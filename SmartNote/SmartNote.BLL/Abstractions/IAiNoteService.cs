using SmartNote.Shared.Dtos;

namespace SmartNote.BLL.Abstractions
{
    public interface IAiNoteService
    {
        Task<AiSummaryResponse> GenerateSummaryAsync(int userId, AiSummaryRequest request);
        Task<AiKnowledgeExtensionResponse> GenerateKnowledgeExtensionAsync(int userId, AiKnowledgeExtensionRequest request);
        Task<MindMapGraphDto> GenerateMindMapAsync(int userId, AiTextToMindMapRequest request);
        Task<AiQuizResponse> GenerateQuizAsync(int userId, AiQuizRequest request);
    }
}

