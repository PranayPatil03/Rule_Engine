using System.Text.Json;
namespace DynamicRuleEngine.Application.DTOs;
public sealed class EvaluateRequestDto
{
    public string WorkflowCode { get; set; } = string.Empty;
    public Dictionary<string, JsonElement> Parameters { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
