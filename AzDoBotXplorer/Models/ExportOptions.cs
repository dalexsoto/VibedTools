namespace AzDoBotXplorer.Models;

/// <summary>
/// Configuration options for report generation and export.
/// </summary>
public class ExportOptions
{
    /// <summary>
    /// Output format: html, json, or csv.
    /// </summary>
    public string Format { get; set; } = "html";
    
    /// <summary>
    /// Output file path. If null, a default name is generated.
    /// </summary>
    public string? OutputFile { get; set; }
    
    /// <summary>
    /// Name of the pool being exported (used in report titles).
    /// </summary>
    public string PoolName { get; set; } = string.Empty;

    /// <summary>
    /// Gets the resolved output file path with appropriate extension.
    /// </summary>
    public string GetOutputPath() => 
        OutputFile ?? $"agents-report.{Format.ToLower()}";
}
