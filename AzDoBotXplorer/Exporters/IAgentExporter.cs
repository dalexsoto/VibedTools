using AzDoBotXplorer.Models;

namespace AzDoBotXplorer.Exporters;

/// <summary>
/// Interface for exporting agent data to various formats.
/// Implement this interface to add new export formats.
/// </summary>
public interface IAgentExporter
{
    /// <summary>
    /// Gets the file extension for this export format (without dot).
    /// </summary>
    string FileExtension { get; }

    /// <summary>
    /// Exports agent data to the specified file.
    /// </summary>
    /// <param name="agents">The agents to export.</param>
    /// <param name="options">Export configuration options.</param>
    /// <returns>A task representing the async operation.</returns>
    Task ExportAsync(List<Agent> agents, ExportOptions options);
}
