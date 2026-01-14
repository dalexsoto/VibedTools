using AzDoBotXplorer.Models;

namespace AzDoBotXplorer.Exporters;

/// <summary>
/// Factory for creating the appropriate exporter based on output format.
/// </summary>
public static class ExporterFactory
{
    /// <summary>
    /// Creates an exporter for the specified format.
    /// </summary>
    /// <param name="format">The output format: html, json, or csv.</param>
    /// <returns>An IAgentExporter implementation for the format.</returns>
    /// <exception cref="ArgumentException">Thrown when format is not supported.</exception>
    public static IAgentExporter Create(string format)
    {
        return format.ToLower() switch
        {
            "html" => new HtmlExporter(),
            "json" => new JsonExporter(),
            "csv" => new CsvExporter(),
            _ => throw new ArgumentException($"Unsupported format: {format}. Use html, json, or csv.", nameof(format))
        };
    }

    /// <summary>
    /// Gets all supported export formats.
    /// </summary>
    public static IEnumerable<string> SupportedFormats => ["html", "json", "csv"];
}
