using System.Text.Json;
using AzDoBotXplorer.Models;

namespace AzDoBotXplorer.Exporters;

/// <summary>
/// Exports agent data to a JSON file.
/// Produces formatted JSON with all agent properties and capabilities.
/// </summary>
public class JsonExporter : IAgentExporter
{
    public string FileExtension => "json";

    public async Task ExportAsync(List<Agent> agents, ExportOptions options)
    {
        var report = new
        {
            GeneratedAt = DateTime.UtcNow,
            PoolName = options.PoolName,
            TotalAgents = agents.Count,
            Agents = agents.Select(a => new
            {
                a.Id,
                a.Name,
                a.Status,
                a.Enabled,
                a.Version,
                a.OsDescription,
                SystemCapabilities = a.SystemCapabilities ?? new Dictionary<string, string>(),
                UserCapabilities = a.UserCapabilities ?? new Dictionary<string, string>(),
                AllCapabilities = a.GetAllCapabilities()
            })
        };

        var jsonOptions = new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
        
        var json = JsonSerializer.Serialize(report, jsonOptions);
        await File.WriteAllTextAsync(options.GetOutputPath(), json);
    }
}
