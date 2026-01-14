using System.Text;
using AzDoBotXplorer.Models;
using AzDoBotXplorer.Services;

namespace AzDoBotXplorer.Exporters;

/// <summary>
/// Exports agent data to a CSV file.
/// Creates a flat structure with one row per agent and columns for each capability.
/// </summary>
public class CsvExporter : IAgentExporter
{
    public string FileExtension => "csv";

    public async Task ExportAsync(List<Agent> agents, ExportOptions options)
    {
        var csv = GenerateCsv(agents);
        await File.WriteAllTextAsync(options.GetOutputPath(), csv);
    }

    /// <summary>
    /// Generates the CSV content with all agents and their capabilities.
    /// </summary>
    private static string GenerateCsv(List<Agent> agents)
    {
        var sb = new StringBuilder();
        
        // Get all unique capability keys across all agents
        var allCapKeys = AgentFilterService.GetAllCapabilityKeys(agents);

        // Write header row
        sb.Append("Id,Name,Status,Enabled,Version,OsDescription");
        foreach (var key in allCapKeys)
        {
            sb.Append($",\"{EscapeCsvValue(key)}\"");
        }
        sb.AppendLine();

        // Write data rows
        foreach (var agent in agents)
        {
            var caps = agent.GetAllCapabilities();
            
            sb.Append($"{agent.Id},");
            sb.Append($"\"{EscapeCsvValue(agent.Name ?? "")}\",");
            sb.Append($"{agent.Status},");
            sb.Append($"{agent.Enabled},");
            sb.Append($"\"{EscapeCsvValue(agent.Version ?? "")}\",");
            sb.Append($"\"{EscapeCsvValue(agent.OsDescription ?? "")}\"");
            
            foreach (var key in allCapKeys)
            {
                var value = caps.TryGetValue(key, out var v) ? v : "";
                sb.Append($",\"{EscapeCsvValue(value)}\"");
            }
            sb.AppendLine();
        }

        return sb.ToString();
    }

    /// <summary>
    /// Escapes special characters in CSV values.
    /// </summary>
    private static string EscapeCsvValue(string value) => 
        value.Replace("\"", "\"\"");
}
