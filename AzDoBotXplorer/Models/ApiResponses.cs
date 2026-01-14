using System.Text.Json.Serialization;

namespace AzDoBotXplorer.Models;

/// <summary>
/// API response wrapper for pool list endpoint.
/// </summary>
public class PoolResponse
{
    [JsonPropertyName("value")]
    public List<Pool>? Value { get; set; }
}

/// <summary>
/// API response wrapper for agent list endpoint.
/// </summary>
public class AgentResponse
{
    [JsonPropertyName("value")]
    public List<Agent>? Value { get; set; }
}
