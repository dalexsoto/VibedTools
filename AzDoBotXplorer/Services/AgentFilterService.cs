using AzDoBotXplorer.Models;

namespace AzDoBotXplorer.Services;

/// <summary>
/// Service for filtering agents based on various criteria.
/// </summary>
public static class AgentFilterService
{
    /// <summary>
    /// Filters agents by capability name and optional value.
    /// </summary>
    /// <param name="agents">The list of agents to filter.</param>
    /// <param name="filter">Filter string in format "name" or "name=value".</param>
    /// <returns>Filtered list of agents matching the capability criteria.</returns>
    /// <remarks>
    /// Filter examples:
    /// - "dotnet" - matches agents with any capability named "dotnet"
    /// - "Agent.OS=Windows" - matches agents where Agent.OS contains "Windows"
    /// - "npm" - matches agents that have an "npm" capability
    /// </remarks>
    public static List<Agent> FilterByCapability(List<Agent> agents, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
        {
            return agents;
        }

        var parts = filter.Split('=', 2);
        var capName = parts[0].Trim();
        var capValue = parts.Length > 1 ? parts[1].Trim() : null;

        return agents.Where(agent =>
        {
            var capabilities = agent.GetAllCapabilities();
            
            if (capValue == null)
            {
                // Filter by capability name only (case-insensitive)
                return capabilities.Keys.Any(k => 
                    k.Contains(capName, StringComparison.OrdinalIgnoreCase));
            }
            
            // Filter by capability name and value
            return capabilities.TryGetValue(capName, out var val) && 
                   val.Contains(capValue, StringComparison.OrdinalIgnoreCase);
        }).ToList();
    }

    /// <summary>
    /// Filters agents that have a specific capability with an exact value match.
    /// </summary>
    /// <param name="agents">The list of agents to filter.</param>
    /// <param name="capabilityName">The capability name to match.</param>
    /// <param name="exactValue">The exact value to match.</param>
    /// <returns>Filtered list of agents.</returns>
    public static List<Agent> FilterByExactCapability(
        List<Agent> agents, 
        string capabilityName, 
        string exactValue)
    {
        return agents.Where(agent =>
        {
            var capabilities = agent.GetAllCapabilities();
            return capabilities.TryGetValue(capabilityName, out var val) && 
                   val.Equals(exactValue, StringComparison.OrdinalIgnoreCase);
        }).ToList();
    }

    /// <summary>
    /// Gets all unique capability keys across all agents.
    /// </summary>
    /// <param name="agents">The list of agents to analyze.</param>
    /// <returns>Sorted list of unique capability keys.</returns>
    public static List<string> GetAllCapabilityKeys(List<Agent> agents)
    {
        return agents
            .SelectMany(a => a.GetAllCapabilities().Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(k => k)
            .ToList();
    }
}
