using System.Text.Json.Serialization;

namespace AzDoBotXplorer.Models;

/// <summary>
/// Represents an Azure DevOps agent pool.
/// </summary>
public class Pool
{
    /// <summary>
    /// Unique identifier of the pool.
    /// </summary>
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    /// <summary>
    /// Display name of the pool.
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }
}
