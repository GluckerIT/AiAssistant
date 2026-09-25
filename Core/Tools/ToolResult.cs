namespace AiAssistant.Core.Tools;

public class ToolResult
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;

    public static ToolResult Ok  (string message) => new()
    {
        Success = true,
        Message = message
    };

    public static ToolResult Fail(string message) => new()
    {
        Success = false,
        Message = message
    };
}     
