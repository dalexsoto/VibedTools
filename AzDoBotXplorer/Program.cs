using System.CommandLine;
using AzDoBotXplorer.Exporters;
using AzDoBotXplorer.Models;
using AzDoBotXplorer.Services;

// =============================================================================
// AzDoBotXplorer - Azure DevOps Agent Pool Explorer
// =============================================================================
// A command-line tool for exploring Azure DevOps agent pools, listing agents,
// filtering by capabilities, and generating reports in multiple formats.
// =============================================================================

// Define command-line options
var organizationOption = new Option<string>(
    name: "--organization",
    description: "Azure DevOps organization URL or name (e.g., 'myorg' or 'https://dev.azure.com/myorg')")
{ IsRequired = true };

var patOption = new Option<string>(
    name: "--pat",
    description: "Personal Access Token with Agent Pools (read) permission")
{ IsRequired = true };

var poolOption = new Option<string>(
    name: "--pool",
    description: "Agent pool name or numeric ID")
{ IsRequired = true };

var capabilityFilterOption = new Option<string?>(
    name: "--capability-filter",
    description: "Filter agents by capability. Format: 'name' or 'name=value' (e.g., 'dotnet' or 'Agent.OS=Windows')");

var outputFormatOption = new Option<string>(
    name: "--output",
    getDefaultValue: () => "html",
    description: "Output format: html, json, or csv");

var outputFileOption = new Option<string?>(
    name: "--output-file",
    description: "Output file path (default: agents-report.{format})");

// Build the root command
var rootCommand = new RootCommand(
    "AzDoBotXplorer - Azure DevOps Agent Pool Explorer\n\n" +
    "Lists online and enabled agents in an Azure DevOps pool with their capabilities.\n" +
    "Generates interactive HTML reports or exports data to JSON/CSV.")
{
    organizationOption,
    patOption,
    poolOption,
    capabilityFilterOption,
    outputFormatOption,
    outputFileOption
};

// Set the command handler
rootCommand.SetHandler(async (organization, pat, pool, capabilityFilter, outputFormat, outputFile) =>
{
    await RunAsync(organization, pat, pool, capabilityFilter, outputFormat, outputFile);
}, organizationOption, patOption, poolOption, capabilityFilterOption, outputFormatOption, outputFileOption);

return await rootCommand.InvokeAsync(args);

// =============================================================================
// Main Application Logic
// =============================================================================

/// <summary>
/// Main application entry point that orchestrates the agent discovery and export process.
/// </summary>
async Task RunAsync(
    string organization, 
    string pat, 
    string pool, 
    string? capabilityFilter, 
    string outputFormat, 
    string? outputFile)
{
    try
    {
        Console.WriteLine("AzDoBotXplorer - Azure DevOps Agent Pool Explorer");
        Console.WriteLine(new string('=', 50));

        // Initialize the Azure DevOps service
        using var azureDevOps = new AzureDevOpsService(organization, pat);

        // Resolve pool name to ID
        Console.WriteLine($"Looking up pool: {pool}...");
        var poolId = await azureDevOps.GetPoolIdAsync(pool);
        
        if (poolId == null)
        {
            Console.WriteLine($"Error: Pool '{pool}' not found.");
            Console.WriteLine("Please verify the pool name or ID and your permissions.");
            Environment.ExitCode = 1;
            return;
        }

        // Retrieve all agents (filtering will be done in HTML report)
        Console.WriteLine($"Fetching agents from pool ID {poolId}...");
        var agents = await azureDevOps.GetAgentsAsync(poolId.Value);
        
        Console.WriteLine($"Found {agents.Count} agent(s) in pool");

        // Apply capability filter if specified
        if (!string.IsNullOrEmpty(capabilityFilter))
        {
            Console.WriteLine($"Applying filter: {capabilityFilter}");
            agents = AgentFilterService.FilterByCapability(agents, capabilityFilter);
            Console.WriteLine($"Filtered to {agents.Count} agent(s)");
        }

        // Configure export options
        var exportOptions = new ExportOptions
        {
            Format = outputFormat,
            OutputFile = outputFile,
            PoolName = pool
        };

        // Create exporter and generate output
        var exporter = ExporterFactory.Create(outputFormat);
        await exporter.ExportAsync(agents, exportOptions);

        Console.WriteLine($"\nReport generated: {exportOptions.GetOutputPath()}");
        Console.WriteLine("Done!");
    }
    catch (HttpRequestException ex)
    {
        Console.WriteLine($"Error communicating with Azure DevOps: {ex.Message}");
        Console.WriteLine("Please verify your organization URL and PAT.");
        Environment.ExitCode = 1;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error: {ex.Message}");
        Environment.ExitCode = 1;
    }
}
