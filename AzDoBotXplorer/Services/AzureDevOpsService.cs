using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AzDoBotXplorer.Models;

namespace AzDoBotXplorer.Services;

/// <summary>
/// Service for interacting with the Azure DevOps REST API.
/// Handles authentication, pool lookup, and agent retrieval.
/// </summary>
public class AzureDevOpsService : IDisposable
{
    private readonly HttpClient _client;
    private readonly string _baseUrl;

    /// <summary>
    /// Initializes a new instance of the AzureDevOpsService.
    /// </summary>
    /// <param name="organization">Azure DevOps organization URL or name.</param>
    /// <param name="personalAccessToken">Personal Access Token for authentication.</param>
    public AzureDevOpsService(string organization, string personalAccessToken)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic",
            Convert.ToBase64String(Encoding.ASCII.GetBytes($":{personalAccessToken}")));
        _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        // Normalize the organization URL
        _baseUrl = NormalizeOrganizationUrl(organization);
    }

    /// <summary>
    /// Normalizes the organization input to a proper Azure DevOps URL.
    /// </summary>
    private static string NormalizeOrganizationUrl(string organization)
    {
        var url = organization.TrimEnd('/');
        
        // If it's already a full URL, use it as-is
        if (url.Contains("dev.azure.com") || url.Contains(".visualstudio.com"))
        {
            return url;
        }
        
        // Otherwise, assume it's just the org name
        return $"https://dev.azure.com/{url}";
    }

    /// <summary>
    /// Gets the pool ID from a pool name or ID string.
    /// </summary>
    /// <param name="pool">Pool name or numeric ID.</param>
    /// <returns>The pool ID if found; otherwise null.</returns>
    public async Task<int?> GetPoolIdAsync(string pool)
    {
        // If it's already a numeric ID, return it directly
        if (int.TryParse(pool, out var id))
        {
            return id;
        }

        // Otherwise, look up the pool by name
        var response = await _client.GetAsync($"{_baseUrl}/_apis/distributedtask/pools?api-version=7.1");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var pools = JsonSerializer.Deserialize<PoolResponse>(content);
        
        var matchedPool = pools?.Value?.FirstOrDefault(p => 
            p.Name?.Equals(pool, StringComparison.OrdinalIgnoreCase) == true);
        
        return matchedPool?.Id;
    }

    /// <summary>
    /// Retrieves all agents in the specified pool.
    /// </summary>
    /// <param name="poolId">The pool ID to query.</param>
    /// <returns>List of agents with their capabilities.</returns>
    public async Task<List<Agent>> GetAgentsAsync(int poolId)
    {
        var response = await _client.GetAsync(
            $"{_baseUrl}/_apis/distributedtask/pools/{poolId}/agents?includeCapabilities=true&api-version=7.1");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        var agentResponse = JsonSerializer.Deserialize<AgentResponse>(content);
        
        return agentResponse?.Value ?? [];
    }

    /// <summary>
    /// Retrieves only online and enabled agents from the specified pool.
    /// </summary>
    /// <param name="poolId">The pool ID to query.</param>
    /// <returns>List of online and enabled agents.</returns>
    public async Task<List<Agent>> GetOnlineEnabledAgentsAsync(int poolId)
    {
        var agents = await GetAgentsAsync(poolId);
        return agents.Where(a => a.IsOnlineAndEnabled).ToList();
    }

    public void Dispose()
    {
        _client.Dispose();
        GC.SuppressFinalize(this);
    }
}
