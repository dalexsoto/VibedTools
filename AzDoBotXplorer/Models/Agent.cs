using System.Text.Json.Serialization;

namespace AzDoBotXplorer.Models;

/// <summary>
/// Represents an Azure DevOps build agent with its properties and capabilities.
/// </summary>
public class Agent
{
    /// <summary>
    /// Unique identifier of the agent.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    /// <summary>
    /// Display name of the agent.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    /// <summary>
    /// Current status of the agent (e.g., "online", "offline").
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; set; }
    
    /// <summary>
    /// Indicates whether the agent is enabled and can accept jobs.
    /// </summary>
    [JsonPropertyName("enabled")]
    public bool? Enabled { get; set; }
    
    /// <summary>
    /// Version of the agent software.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }
    
    /// <summary>
    /// Description of the operating system running the agent.
    /// </summary>
    [JsonPropertyName("osDescription")]
    public string? OsDescription { get; set; }
    
    /// <summary>
    /// System-detected capabilities (e.g., installed software, environment variables).
    /// </summary>
    [JsonPropertyName("systemCapabilities")]
    public Dictionary<string, string>? SystemCapabilities { get; set; }
    
    /// <summary>
    /// User-defined capabilities configured on the agent.
    /// </summary>
    [JsonPropertyName("userCapabilities")]
    public Dictionary<string, string>? UserCapabilities { get; set; }

    /// <summary>
    /// Gets all capabilities (system + user) merged into a single dictionary.
    /// User capabilities override system capabilities if keys conflict.
    /// </summary>
    public Dictionary<string, string> GetAllCapabilities()
    {
        var all = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        if (SystemCapabilities != null)
        {
            foreach (var kvp in SystemCapabilities)
                all[kvp.Key] = kvp.Value;
        }
        
        if (UserCapabilities != null)
        {
            foreach (var kvp in UserCapabilities)
                all[kvp.Key] = kvp.Value;
        }
        
        return all;
    }

    /// <summary>
    /// Checks if the agent is currently online and enabled.
    /// </summary>
    public bool IsOnlineAndEnabled => 
        Status?.Equals("online", StringComparison.OrdinalIgnoreCase) == true && Enabled == true;
}
