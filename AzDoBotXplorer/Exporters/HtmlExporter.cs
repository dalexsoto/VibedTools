using System.Text;
using System.Text.Json;
using System.Web;
using AzDoBotXplorer.Models;
using AzDoBotXplorer.Services;

namespace AzDoBotXplorer.Exporters;

/// <summary>
/// Exports agent data to an interactive HTML report.
/// Features include:
/// - Responsive card-based layout
/// - Real-time filtering by capability
/// - Sortable capability columns
/// - Collapsible capability sections
/// </summary>
public class HtmlExporter : IAgentExporter
{
    public string FileExtension => "html";

    public async Task ExportAsync(List<Agent> agents, ExportOptions options)
    {
        var html = GenerateHtml(agents, options.PoolName);
        await File.WriteAllTextAsync(options.GetOutputPath(), html);
    }

    /// <summary>
    /// Generates the complete HTML document.
    /// </summary>
    private static string GenerateHtml(List<Agent> agents, string poolName)
    {
        var sb = new StringBuilder();
        var allCapKeys = AgentFilterService.GetAllCapabilityKeys(agents);

        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html lang=\"en\">");
        sb.AppendLine("<head>");
        sb.AppendLine("    <meta charset=\"UTF-8\">");
        sb.AppendLine("    <meta name=\"viewport\" content=\"width=device-width, initial-scale=1.0\">");
        sb.AppendLine($"    <title>Agent Pool Report - {HttpUtility.HtmlEncode(poolName)}</title>");
        AppendStyles(sb);
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        
        AppendHeader(sb, poolName, agents.Count);
        AppendFilterControls(sb, allCapKeys);
        AppendAgentCards(sb, agents);
        AppendScripts(sb);
        
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }

    /// <summary>
    /// Appends CSS styles to the document.
    /// </summary>
    private static void AppendStyles(StringBuilder sb)
    {
        sb.AppendLine("    <style>");
        sb.AppendLine("        :root { --primary: #0078d4; --success: #107c10; --bg: #f5f5f5; --card-bg: #fff; }");
        sb.AppendLine("        * { box-sizing: border-box; }");
        sb.AppendLine("        body { font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; margin: 0; padding: 20px; background: var(--bg); }");
        sb.AppendLine("        h1 { color: var(--primary); margin-bottom: 5px; }");
        sb.AppendLine("        .container { max-width: 1400px; margin: 0 auto; }");
        sb.AppendLine("        .summary { background: var(--card-bg); padding: 15px 20px; border-radius: 8px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine("        .controls { background: var(--card-bg); padding: 20px; border-radius: 8px; margin-bottom: 20px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine("        .controls-row { display: flex; flex-wrap: wrap; gap: 15px; align-items: flex-end; }");
        sb.AppendLine("        .control-group { display: flex; flex-direction: column; gap: 5px; }");
        sb.AppendLine("        .control-group label { font-weight: 600; font-size: 0.85em; color: #666; }");
        sb.AppendLine("        .controls input, .controls select { padding: 8px 12px; border: 1px solid #ccc; border-radius: 4px; font-size: 14px; }");
        sb.AppendLine("        .controls input { width: 280px; }");
        sb.AppendLine("        .controls select { min-width: 200px; }");
        sb.AppendLine("        .agent-card { background: var(--card-bg); padding: 20px; border-radius: 8px; margin-bottom: 15px; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }");
        sb.AppendLine("        .agent-name { font-size: 1.3em; font-weight: bold; color: #333; margin-bottom: 10px; }");
        sb.AppendLine("        .agent-info { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 10px; margin-bottom: 15px; }");
        sb.AppendLine("        .info-item { background: #f0f0f0; padding: 8px 12px; border-radius: 4px; }");
        sb.AppendLine("        .info-label { font-weight: 600; color: #666; font-size: 0.85em; display: block; }");
        sb.AppendLine("        .info-value { color: #333; }");
        sb.AppendLine("        .status-online { color: var(--success); font-weight: 600; }");
        sb.AppendLine("        .status-offline { color: #d13438; font-weight: 600; }");
        sb.AppendLine("        .status-disabled { color: #797775; font-style: italic; }");
        sb.AppendLine("        .capabilities { margin-top: 15px; }");
        sb.AppendLine("        .cap-section { margin-bottom: 15px; }");
        sb.AppendLine("        .cap-title { font-weight: 600; color: var(--primary); margin-bottom: 8px; cursor: pointer; user-select: none; }");
        sb.AppendLine("        .cap-title:hover { text-decoration: underline; }");
        sb.AppendLine("        .cap-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(300px, 1fr)); gap: 8px; }");
        sb.AppendLine("        .cap-item { background: #e8f4fd; padding: 6px 10px; border-radius: 4px; font-size: 0.9em; word-break: break-all; }");
        sb.AppendLine("        .cap-name { font-weight: 600; color: var(--primary); }");
        sb.AppendLine("        .cap-value { color: #333; }");
        sb.AppendLine("        .hidden { display: none; }");
        sb.AppendLine("        .timestamp { color: #666; font-size: 0.9em; margin-bottom: 15px; }");
        sb.AppendLine("        .no-results { text-align: center; padding: 40px; color: #666; font-size: 1.1em; }");
        sb.AppendLine("        .sort-indicator { font-size: 0.8em; margin-left: 5px; }");
        sb.AppendLine("        .export-buttons { display: flex; gap: 10px; margin-left: auto; }");
        sb.AppendLine("        .export-btn { padding: 8px 16px; border: none; border-radius: 4px; cursor: pointer; font-size: 14px; font-weight: 600; transition: background 0.2s; }");
        sb.AppendLine("        .export-btn-json { background: #107c10; color: white; }");
        sb.AppendLine("        .export-btn-json:hover { background: #0e6b0e; }");
        sb.AppendLine("        .export-btn-csv { background: #0078d4; color: white; }");
        sb.AppendLine("        .export-btn-csv:hover { background: #006cbe; }");
        sb.AppendLine("        .modal-overlay { position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); display: none; align-items: center; justify-content: center; z-index: 1000; }");
        sb.AppendLine("        .modal-overlay.visible { display: flex; }");
        sb.AppendLine("        .modal { background: white; border-radius: 8px; padding: 24px; max-width: 600px; width: 90%; max-height: 80vh; display: flex; flex-direction: column; box-shadow: 0 4px 20px rgba(0,0,0,0.3); }");
        sb.AppendLine("        .modal-header { display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; }");
        sb.AppendLine("        .modal-header h2 { margin: 0; color: var(--primary); }");
        sb.AppendLine("        .modal-close { background: none; border: none; font-size: 24px; cursor: pointer; color: #666; }");
        sb.AppendLine("        .modal-close:hover { color: #333; }");
        sb.AppendLine("        .modal-body { overflow-y: auto; flex: 1; }");
        sb.AppendLine("        .modal-footer { display: flex; justify-content: flex-end; gap: 10px; margin-top: 16px; padding-top: 16px; border-top: 1px solid #eee; }");
        sb.AppendLine("        .cap-select-controls { display: flex; gap: 10px; margin-bottom: 12px; }");
        sb.AppendLine("        .cap-select-controls button { padding: 4px 12px; font-size: 12px; border: 1px solid #ccc; background: #f5f5f5; border-radius: 4px; cursor: pointer; }");
        sb.AppendLine("        .cap-select-controls button:hover { background: #e8e8e8; }");
        sb.AppendLine("        .cap-select-filter { padding: 8px; border: 1px solid #ccc; border-radius: 4px; width: 100%; margin-bottom: 12px; }");
        sb.AppendLine("        .cap-list { display: grid; grid-template-columns: repeat(auto-fill, minmax(250px, 1fr)); gap: 8px; }");
        sb.AppendLine("        .cap-checkbox { display: flex; align-items: center; gap: 8px; padding: 6px 10px; background: #f5f5f5; border-radius: 4px; cursor: pointer; font-size: 13px; }");
        sb.AppendLine("        .cap-checkbox:hover { background: #e8f4fd; }");
        sb.AppendLine("        .cap-checkbox input { cursor: pointer; }");
        sb.AppendLine("        .cap-checkbox.hidden { display: none; }");
        sb.AppendLine("        @media (max-width: 600px) { .controls input, .controls select { width: 100%; } .export-buttons { margin-left: 0; margin-top: 10px; } }");
        sb.AppendLine("    </style>");
    }

    /// <summary>
    /// Appends the page header and summary section.
    /// </summary>
    private static void AppendHeader(StringBuilder sb, string poolName, int agentCount)
    {
        sb.AppendLine("<div class=\"container\">");
        sb.AppendLine($"    <h1>Agent Pool Report: {HttpUtility.HtmlEncode(poolName)}</h1>");
        sb.AppendLine($"    <p class=\"timestamp\">Generated: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC</p>");
        sb.AppendLine("    <div class=\"summary\">");
        sb.AppendLine($"        <strong>Total Online & Enabled Agents:</strong> <span id=\"visibleCount\">{agentCount}</span> / {agentCount}");
        sb.AppendLine("    </div>");
    }

    /// <summary>
    /// Appends the filter and sort controls.
    /// </summary>
    private static void AppendFilterControls(StringBuilder sb, List<string> capabilityKeys)
    {
        sb.AppendLine("    <div class=\"controls\">");
        sb.AppendLine("        <div class=\"controls-row\">");
        sb.AppendLine("            <div class=\"control-group\">");
        sb.AppendLine("                <label for=\"capFilter\">Filter by Capability</label>");
        sb.AppendLine("                <input type=\"text\" id=\"capFilter\" placeholder=\"Type to filter (e.g., dotnet, Agent.OS)\" oninput=\"applyFiltersAndSort()\">");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"control-group\">");
        sb.AppendLine("                <label for=\"statusFilter\">Status</label>");
        sb.AppendLine("                <select id=\"statusFilter\" onchange=\"applyFiltersAndSort()\">");
        sb.AppendLine("                    <option value=\"all\">All Statuses</option>");
        sb.AppendLine("                    <option value=\"online\" selected>Online Only</option>");
        sb.AppendLine("                    <option value=\"offline\">Offline Only</option>");
        sb.AppendLine("                </select>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"control-group\">");
        sb.AppendLine("                <label for=\"enabledFilter\">Enabled</label>");
        sb.AppendLine("                <select id=\"enabledFilter\" onchange=\"applyFiltersAndSort()\">");
        sb.AppendLine("                    <option value=\"all\">All Agents</option>");
        sb.AppendLine("                    <option value=\"enabled\" selected>Enabled Only</option>");
        sb.AppendLine("                    <option value=\"disabled\">Disabled Only</option>");
        sb.AppendLine("                </select>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"control-group\">");
        sb.AppendLine("                <label for=\"sortBy\">Sort By</label>");
        sb.AppendLine("                <select id=\"sortBy\" onchange=\"applyFiltersAndSort()\">");
        sb.AppendLine("                    <option value=\"name\">Agent Name</option>");
        sb.AppendLine("                    <option value=\"id\">Agent ID</option>");
        sb.AppendLine("                    <option value=\"version\">Agent Version</option>");
        sb.AppendLine("                    <option value=\"os\">Operating System</option>");
        sb.AppendLine("                    <option value=\"status\">Status</option>");
        
        // Add capability-based sort options
        foreach (var key in capabilityKeys.Take(50)) // Limit to prevent huge dropdowns
        {
            sb.AppendLine($"                    <option value=\"cap:{HttpUtility.HtmlAttributeEncode(key)}\">{HttpUtility.HtmlEncode(key)}</option>");
        }
        
        sb.AppendLine("                </select>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"control-group\">");
        sb.AppendLine("                <label for=\"sortDir\">Direction</label>");
        sb.AppendLine("                <select id=\"sortDir\" onchange=\"applyFiltersAndSort()\">");
        sb.AppendLine("                    <option value=\"asc\">Ascending (A-Z)</option>");
        sb.AppendLine("                    <option value=\"desc\">Descending (Z-A)</option>");
        sb.AppendLine("                </select>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"export-buttons\">");
        sb.AppendLine("                <button class=\"export-btn export-btn-json\" onclick=\"showExportModal('json')\">Export JSON</button>");
        sb.AppendLine("                <button class=\"export-btn export-btn-csv\" onclick=\"showExportModal('csv')\">Export CSV</button>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
    }

    /// <summary>
    /// Appends the agent cards to the document.
    /// </summary>
    private static void AppendAgentCards(StringBuilder sb, List<Agent> agents)
    {
        sb.AppendLine("    <div id=\"agentContainer\">");

        foreach (var agent in agents)
        {
            var allCaps = agent.GetAllCapabilities();
            var status = agent.Status?.ToLower() ?? "unknown";
            var enabled = agent.Enabled == true;
            var statusClass = status == "online" ? "status-online" : "status-offline";
            if (!enabled) statusClass += " status-disabled";

            sb.AppendLine($"    <div class=\"agent-card\" data-id=\"{agent.Id}\" data-name=\"{HttpUtility.HtmlAttributeEncode(agent.Name ?? "")}\" data-version=\"{HttpUtility.HtmlAttributeEncode(agent.Version ?? "")}\" data-os=\"{HttpUtility.HtmlAttributeEncode(agent.OsDescription ?? "")}\" data-status=\"{status}\" data-enabled=\"{enabled.ToString().ToLower()}\" data-capabilities='{JsonSerializer.Serialize(allCaps)}'>");
            sb.AppendLine($"        <div class=\"agent-name\">{HttpUtility.HtmlEncode(agent.Name)}{(!enabled ? " <span class=\"status-disabled\">(Disabled)</span>" : "")}</div>");
            sb.AppendLine("        <div class=\"agent-info\">");
            sb.AppendLine($"            <div class=\"info-item\"><span class=\"info-label\">ID</span><span class=\"info-value\">{agent.Id}</span></div>");
            sb.AppendLine($"            <div class=\"info-item\"><span class=\"info-label\">Status</span><span class=\"info-value {statusClass}\">{agent.Status ?? "Unknown"}</span></div>");
            sb.AppendLine($"            <div class=\"info-item\"><span class=\"info-label\">Enabled</span><span class=\"info-value\">{(enabled ? "Yes" : "No")}</span></div>");
            sb.AppendLine($"            <div class=\"info-item\"><span class=\"info-label\">Version</span><span class=\"info-value\">{HttpUtility.HtmlEncode(agent.Version ?? "N/A")}</span></div>");
            sb.AppendLine($"            <div class=\"info-item\"><span class=\"info-label\">OS</span><span class=\"info-value\">{HttpUtility.HtmlEncode(agent.OsDescription ?? "N/A")}</span></div>");
            sb.AppendLine("        </div>");
            
            AppendCapabilitySections(sb, agent);
            
            sb.AppendLine("    </div>");
        }

        sb.AppendLine("    </div>");
        sb.AppendLine("    <div id=\"noResults\" class=\"no-results hidden\">No agents match the current filter.</div>");
        
        // Add export modal (hidden by default via CSS)
        sb.AppendLine("    <div id=\"exportModal\" class=\"modal-overlay\" onclick=\"closeModalOnOverlay(event)\">");
        sb.AppendLine("        <div class=\"modal\">");
        sb.AppendLine("            <div class=\"modal-header\">");
        sb.AppendLine("                <h2 id=\"modalTitle\">Export Options</h2>");
        sb.AppendLine("                <button class=\"modal-close\" onclick=\"closeExportModal()\">&times;</button>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"modal-body\">");
        sb.AppendLine("                <input type=\"text\" class=\"cap-select-filter\" id=\"capSearchFilter\" placeholder=\"Search capabilities...\" oninput=\"filterCapabilityCheckboxes()\">");
        sb.AppendLine("                <div class=\"cap-select-controls\">");
        sb.AppendLine("                    <button onclick=\"selectAllCaps()\">Select All</button>");
        sb.AppendLine("                    <button onclick=\"selectNoneCaps()\">Select None</button>");
        sb.AppendLine("                    <button onclick=\"selectCommonCaps()\">Select Common</button>");
        sb.AppendLine("                </div>");
        sb.AppendLine("                <div id=\"capList\" class=\"cap-list\"></div>");
        sb.AppendLine("            </div>");
        sb.AppendLine("            <div class=\"modal-footer\">");
        sb.AppendLine("                <button class=\"export-btn\" style=\"background:#666;color:white\" onclick=\"closeExportModal()\">Cancel</button>");
        sb.AppendLine("                <button class=\"export-btn export-btn-json\" id=\"modalExportBtn\" onclick=\"executeExport()\">Export</button>");
        sb.AppendLine("            </div>");
        sb.AppendLine("        </div>");
        sb.AppendLine("    </div>");
        
        sb.AppendLine("</div>"); // Close container
    }

    /// <summary>
    /// Appends the capability sections for an agent.
    /// </summary>
    private static void AppendCapabilitySections(StringBuilder sb, Agent agent)
    {
        sb.AppendLine("        <div class=\"capabilities\">");
        
        if (agent.SystemCapabilities?.Count > 0)
        {
            sb.AppendLine("            <div class=\"cap-section\">");
            sb.AppendLine($"                <div class=\"cap-title\" onclick=\"toggleSection(this)\">▼ System Capabilities ({agent.SystemCapabilities.Count})</div>");
            sb.AppendLine("                <div class=\"cap-grid\">");
            foreach (var cap in agent.SystemCapabilities.OrderBy(c => c.Key))
            {
                sb.AppendLine($"                    <div class=\"cap-item\"><span class=\"cap-name\">{HttpUtility.HtmlEncode(cap.Key)}:</span> <span class=\"cap-value\">{HttpUtility.HtmlEncode(cap.Value)}</span></div>");
            }
            sb.AppendLine("                </div>");
            sb.AppendLine("            </div>");
        }
        
        if (agent.UserCapabilities?.Count > 0)
        {
            sb.AppendLine("            <div class=\"cap-section\">");
            sb.AppendLine($"                <div class=\"cap-title\" onclick=\"toggleSection(this)\">▼ User Capabilities ({agent.UserCapabilities.Count})</div>");
            sb.AppendLine("                <div class=\"cap-grid\">");
            foreach (var cap in agent.UserCapabilities.OrderBy(c => c.Key))
            {
                sb.AppendLine($"                    <div class=\"cap-item\"><span class=\"cap-name\">{HttpUtility.HtmlEncode(cap.Key)}:</span> <span class=\"cap-value\">{HttpUtility.HtmlEncode(cap.Value)}</span></div>");
            }
            sb.AppendLine("                </div>");
            sb.AppendLine("            </div>");
        }
        
        sb.AppendLine("        </div>");
    }

    /// <summary>
    /// Appends JavaScript for filtering, sorting, and interactivity.
    /// </summary>
    private static void AppendScripts(StringBuilder sb)
    {
        sb.AppendLine("    <script>");
        sb.AppendLine(@"
        // Store original order of cards
        const container = document.getElementById('agentContainer');
        const cards = Array.from(container.querySelectorAll('.agent-card'));
        
        // Apply initial filter on page load
        setTimeout(applyFiltersAndSort, 0);
        
        /**
         * Applies both filter and sort to agent cards.
         */
        function applyFiltersAndSort() {
            const filter = document.getElementById('capFilter').value.toLowerCase();
            const statusFilter = document.getElementById('statusFilter').value;
            const enabledFilter = document.getElementById('enabledFilter').value;
            const sortBy = document.getElementById('sortBy').value;
            const sortDir = document.getElementById('sortDir').value;
            
            // Filter cards
            let visibleCards = cards.filter(card => {
                // Status filter
                if (statusFilter !== 'all') {
                    const cardStatus = card.dataset.status;
                    if (statusFilter === 'online' && cardStatus !== 'online') return false;
                    if (statusFilter === 'offline' && cardStatus === 'online') return false;
                }
                
                // Enabled filter
                if (enabledFilter !== 'all') {
                    const cardEnabled = card.dataset.enabled === 'true';
                    if (enabledFilter === 'enabled' && !cardEnabled) return false;
                    if (enabledFilter === 'disabled' && cardEnabled) return false;
                }
                
                // Capability filter
                if (filter) {
                    const caps = JSON.parse(card.dataset.capabilities);
                    const name = card.dataset.name.toLowerCase();
                    const os = card.dataset.os.toLowerCase();
                    
                    // Search in agent name, OS, and all capabilities
                    if (name.includes(filter) || os.includes(filter)) return true;
                    
                    return Object.entries(caps).some(([k, v]) =>
                        k.toLowerCase().includes(filter) || 
                        v.toLowerCase().includes(filter));
                }
                
                return true;
            });
            
            // Sort cards
            visibleCards.sort((a, b) => {
                let valA, valB;
                
                if (sortBy === 'name') {
                    valA = a.dataset.name;
                    valB = b.dataset.name;
                } else if (sortBy === 'id') {
                    valA = parseInt(a.dataset.id);
                    valB = parseInt(b.dataset.id);
                    return sortDir === 'asc' ? valA - valB : valB - valA;
                } else if (sortBy === 'version') {
                    valA = a.dataset.version;
                    valB = b.dataset.version;
                } else if (sortBy === 'os') {
                    valA = a.dataset.os;
                    valB = b.dataset.os;
                } else if (sortBy === 'status') {
                    valA = a.dataset.status;
                    valB = b.dataset.status;
                } else if (sortBy.startsWith('cap:')) {
                    const capKey = sortBy.substring(4);
                    const capsA = JSON.parse(a.dataset.capabilities);
                    const capsB = JSON.parse(b.dataset.capabilities);
                    valA = capsA[capKey] || capsA[Object.keys(capsA).find(k => k.toLowerCase() === capKey.toLowerCase())] || '';
                    valB = capsB[capKey] || capsB[Object.keys(capsB).find(k => k.toLowerCase() === capKey.toLowerCase())] || '';
                }
                
                const comparison = String(valA).localeCompare(String(valB), undefined, {numeric: true, sensitivity: 'base'});
                return sortDir === 'asc' ? comparison : -comparison;
            });
            
            // Update DOM
            container.innerHTML = '';
            const hiddenCards = cards.filter(c => !visibleCards.includes(c));
            
            visibleCards.forEach(card => {
                card.classList.remove('hidden');
                container.appendChild(card);
            });
            
            hiddenCards.forEach(card => {
                card.classList.add('hidden');
                container.appendChild(card);
            });
            
            // Update counter and no-results message
            document.getElementById('visibleCount').textContent = visibleCards.length;
            document.getElementById('noResults').classList.toggle('hidden', visibleCards.length > 0);
        }
        
        /**
         * Toggles visibility of a capability section.
         */
        function toggleSection(el) {
            const grid = el.nextElementSibling;
            const isHidden = grid.classList.toggle('hidden');
            el.textContent = el.textContent.replace(/[▼▶]/, isHidden ? '▶' : '▼');
        }
        ");

        // Add export functionality as separate script block to avoid escaping issues
        sb.AppendLine(@"
        let currentExportFormat = 'json';
        let allCapabilityKeys = [];
        
        /**
         * Gets the currently visible (filtered) agents' data.
         */
        function getVisibleAgentsData() {
            const visibleCards = Array.from(container.querySelectorAll('.agent-card:not(.hidden)'));
            return visibleCards.map(card => ({
                id: parseInt(card.dataset.id),
                name: card.dataset.name,
                status: card.dataset.status,
                enabled: card.dataset.enabled === 'true',
                version: card.dataset.version,
                osDescription: card.dataset.os,
                capabilities: JSON.parse(card.dataset.capabilities)
            }));
        }
        
        /**
         * Shows the export modal with capability selection.
         */
        function showExportModal(format) {
            currentExportFormat = format;
            const agents = getVisibleAgentsData();
            
            if (agents.length === 0) {
                alert('No agents to export');
                return;
            }
            
            // Get all unique capability keys from visible agents
            allCapabilityKeys = [...new Set(agents.flatMap(a => Object.keys(a.capabilities)))].sort();
            
            // Update modal title and button
            document.getElementById('modalTitle').textContent = 'Export to ' + format.toUpperCase();
            const btn = document.getElementById('modalExportBtn');
            btn.textContent = 'Export ' + format.toUpperCase();
            btn.className = 'export-btn ' + (format === 'json' ? 'export-btn-json' : 'export-btn-csv');
            
            // Build capability checkboxes
            const capList = document.getElementById('capList');
            capList.innerHTML = allCapabilityKeys.map(key => 
                '<label class=""cap-checkbox"">' +
                '<input type=""checkbox"" checked value=""' + escapeHtml(key) + '""> ' +
                escapeHtml(key) +
                '</label>'
            ).join('');
            
            // Clear search filter
            document.getElementById('capSearchFilter').value = '';
            
            // Show modal
            document.getElementById('exportModal').classList.add('visible');
        }
        
        /**
         * Closes the export modal.
         */
        function closeExportModal() {
            document.getElementById('exportModal').classList.remove('visible');
        }
        
        /**
         * Closes modal when clicking on overlay.
         */
        function closeModalOnOverlay(event) {
            if (event.target.id === 'exportModal') {
                closeExportModal();
            }
        }
        
        /**
         * Filters capability checkboxes by search text.
         */
        function filterCapabilityCheckboxes() {
            const filter = document.getElementById('capSearchFilter').value.toLowerCase();
            document.querySelectorAll('.cap-checkbox').forEach(label => {
                const text = label.textContent.toLowerCase();
                label.classList.toggle('hidden', filter && !text.includes(filter));
            });
        }
        
        /**
         * Selects all capability checkboxes.
         */
        function selectAllCaps() {
            document.querySelectorAll('.cap-checkbox:not(.hidden) input').forEach(cb => cb.checked = true);
        }
        
        /**
         * Deselects all capability checkboxes.
         */
        function selectNoneCaps() {
            document.querySelectorAll('.cap-checkbox input').forEach(cb => cb.checked = false);
        }
        
        /**
         * Selects common/important capabilities.
         */
        function selectCommonCaps() {
            const commonPatterns = ['agent.', 'node', 'npm', 'dotnet', 'java', 'python', 'maven', 'msbuild', 'visualstudio', 'docker', 'git'];
            document.querySelectorAll('.cap-checkbox input').forEach(cb => {
                const val = cb.value.toLowerCase();
                cb.checked = commonPatterns.some(p => val.includes(p));
            });
        }
        
        /**
         * Gets the selected capability keys.
         */
        function getSelectedCapabilities() {
            return Array.from(document.querySelectorAll('.cap-checkbox input:checked')).map(cb => cb.value);
        }
        
        /**
         * Executes the export with selected capabilities.
         */
        function executeExport() {
            const selectedCaps = getSelectedCapabilities();
            
            if (currentExportFormat === 'json') {
                exportToJson(selectedCaps);
            } else {
                exportToCsv(selectedCaps);
            }
            
            closeExportModal();
        }
        
        /**
         * Escapes HTML special characters.
         */
        function escapeHtml(text) {
            const div = document.createElement('div');
            div.textContent = text;
            return div.innerHTML;
        }
        
        /**
         * Exports currently visible agents to JSON with selected capabilities.
         */
        function exportToJson(selectedCaps) {
            const agents = getVisibleAgentsData();
            
            // Filter capabilities for each agent
            const filteredAgents = agents.map(agent => {
                const filteredCaps = {};
                selectedCaps.forEach(key => {
                    if (agent.capabilities[key] !== undefined) {
                        filteredCaps[key] = agent.capabilities[key];
                    }
                });
                return {
                    ...agent,
                    capabilities: filteredCaps
                };
            });
            
            const report = {
                generatedAt: new Date().toISOString(),
                exportedFrom: 'HTML Report',
                totalAgents: filteredAgents.length,
                selectedCapabilities: selectedCaps,
                agents: filteredAgents
            };
            
            const json = JSON.stringify(report, null, 2);
            downloadFile(json, 'agents-export.json', 'application/json');
        }
        
        /**
         * Exports currently visible agents to CSV with selected capabilities.
         */
        function exportToCsv(selectedCaps) {
            const agents = getVisibleAgentsData();
            
            // Build CSV header
            let csv = 'Id,Name,Status,Enabled,Version,OsDescription';
            selectedCaps.forEach(key => {
                csv += ','  + '""' + escapeCsv(key) + '""';
            });
            csv += '\n';
            
            // Build CSV rows
            agents.forEach(agent => {
                csv += agent.id + ',';
                csv += '""' + escapeCsv(agent.name) + '"",';
                csv += agent.status + ',';
                csv += agent.enabled + ',';
                csv += '""' + escapeCsv(agent.version) + '"",';
                csv += '""' + escapeCsv(agent.osDescription) + '""';
                
                selectedCaps.forEach(key => {
                    const value = agent.capabilities[key] || '';
                    csv += ',' + '""' + escapeCsv(value) + '""';
                });
                csv += '\n';
            });
            
            downloadFile(csv, 'agents-export.csv', 'text/csv');
        }
        
        /**
         * Escapes special characters for CSV format.
         */
        function escapeCsv(value) {
            if (value == null) return '';
            return String(value).replace(/""/g, '""""');
        }
        
        /**
         * Triggers a file download in the browser.
         */
        function downloadFile(content, filename, mimeType) {
            const blob = new Blob([content], { type: mimeType });
            const url = URL.createObjectURL(blob);
            const a = document.createElement('a');
            a.href = url;
            a.download = filename;
            document.body.appendChild(a);
            a.click();
            document.body.removeChild(a);
            URL.revokeObjectURL(url);
        }
        ");
        sb.AppendLine("    </script>");
    }
}
