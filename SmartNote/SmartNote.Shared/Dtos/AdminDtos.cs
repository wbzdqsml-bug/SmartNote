namespace SmartNote.Shared.Dtos
{
    /// <summary>
    /// 管理端简单健康检查/占位响应。
    /// </summary>
    public record AdminPingResult(string Area, string Message, DateTime Time);
}
