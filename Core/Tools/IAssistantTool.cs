namespace AiAssistant.Core.Tools;

public interface IAssistantTool<TArgs>
{
    string Name { get; }
    string Description { get; }
    Task<ToolResult> ExecuteAsync(TArgs arguments);
}

