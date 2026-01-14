# AzDoBotXplorer - Specification

## Overview

AzDoBotXplorer is a .NET console application for exploring Azure DevOps agent pools. It connects to the Azure DevOps REST API to retrieve agent information, filter agents by capabilities, and generate reports in multiple formats.

**Version:** 1.1.0  
**Target Framework:** .NET 10.0  
**Last Updated:** 2026-01-14

---

## Features

### Core Features

| Feature | Description |
|---------|-------------|
| **Agent Discovery** | Lists all agents in a specified Azure DevOps pool (online, offline, enabled, disabled) |
| **Status Filtering** | Filter agents by online/offline status in the HTML report |
| **Enabled Filtering** | Filter agents by enabled/disabled state in the HTML report |
| **Capability Filtering** | Filter agents by capability name or name=value pairs |
| **Multi-Format Export** | Export to HTML, JSON, or CSV formats |
| **Interactive HTML Report** | Responsive report with filtering, sorting, and export capabilities |

### HTML Report Features

- **Real-time Filtering**: Type to filter agents by any capability, name, or OS
- **Status Filter**: Filter by Online/Offline/All status (defaults to Online)
- **Enabled Filter**: Filter by Enabled/Disabled/All state (defaults to Enabled)
- **Sortable Columns**: Sort by agent name, ID, version, OS, status, or any capability
- **Collapsible Sections**: Expand/collapse system and user capabilities
- **Responsive Design**: Works on desktop and mobile devices
- **Agent Count Display**: Shows filtered vs. total agent count
- **In-Browser Export**: Export filtered results directly to JSON or CSV from the HTML report
- **Capability Selection**: Choose which capabilities to include when exporting (with Select All/None/Common shortcuts)
- **Visual Status Indicators**: Color-coded status (green=online, red=offline, gray=disabled)

### Export Formats

| Format | Description | Use Case |
|--------|-------------|----------|
| **HTML** | Interactive web report | Human-readable dashboards, sharing with teams |
| **JSON** | Structured data with metadata | Integration with other tools, programmatic processing |
| **CSV** | Flat tabular format | Excel analysis, database import, spreadsheets |

---

## Architecture

### Project Structure

```
AzDoBotXplorer/
├── Program.cs              # Application entry point and CLI configuration
├── Models/                 # Data models
│   ├── Agent.cs           # Agent entity with capabilities
│   ├── Pool.cs            # Agent pool entity
│   ├── ApiResponses.cs    # API response wrappers
│   └── ExportOptions.cs   # Export configuration
├── Services/               # Business logic
│   ├── AzureDevOpsService.cs    # Azure DevOps API client
│   └── AgentFilterService.cs    # Agent filtering logic
├── Exporters/              # Report generators
│   ├── IAgentExporter.cs  # Exporter interface
│   ├── ExporterFactory.cs # Factory for creating exporters
│   ├── HtmlExporter.cs    # HTML report generator
│   ├── JsonExporter.cs    # JSON export
│   └── CsvExporter.cs     # CSV export
└── spec.md                 # This specification document
```

### Design Patterns

- **Factory Pattern**: `ExporterFactory` creates appropriate exporters based on format
- **Interface Segregation**: `IAgentExporter` defines a clean contract for exporters
- **Single Responsibility**: Each class has a focused purpose
- **Dependency Injection Ready**: Services can be easily mocked for testing

---

## Usage

### Command Line Options

```
Options:
  --organization <org>     (REQUIRED) Azure DevOps organization URL or name
  --pat <token>            (REQUIRED) Personal Access Token
  --pool <name|id>         (REQUIRED) Agent pool name or ID
  --capability-filter      Filter by capability (format: name or name=value)
  --output <format>        Output format: html, json, csv (default: html)
  --output-file <path>     Output file path (default: agents-report.{format})
  -?, -h, --help           Show help
```

### Examples

```bash
# Generate HTML report for "Default" pool (all agents, filter in browser)
dotnet run -- --organization "myorg" --pat "xxxxx" --pool "Default"

# Export to JSON
dotnet run -- --organization "myorg" --pat "xxxxx" --pool "Default" --output json

# Export to CSV with custom filename
dotnet run -- --organization "myorg" --pat "xxxxx" --pool "Default" \
    --output csv --output-file "my-agents.csv"

# Filter by capability name (agents that have 'dotnet')
dotnet run -- --organization "myorg" --pat "xxxxx" --pool "Default" \
    --capability-filter "dotnet"

# Filter by capability value (Windows agents only)
dotnet run -- --organization "myorg" --pat "xxxxx" --pool "Default" \
    --capability-filter "Agent.OS=Windows"

# Using full organization URL
dotnet run -- --organization "https://dev.azure.com/myorg" \
    --pat "xxxxx" --pool "Azure Pipelines"

# Real world
dotnet run -- --organization "https://foo.visualstudio.com" --pat "YourPat" --pool "PoolName"
```

---

## API Integration

### Azure DevOps REST API Endpoints

| Endpoint | Purpose |
|----------|---------|
| `GET /_apis/distributedtask/pools` | List all agent pools |
| `GET /_apis/distributedtask/pools/{poolId}/agents?includeCapabilities=true` | List all agents with capabilities |

### Authentication

- Uses Personal Access Token (PAT) with Basic authentication
- Required scope: **Agent Pools (Read)**

### API Version

- Uses API version `7.1`

---

## Output Formats

### HTML Report Structure

```html
<!DOCTYPE html>
<html>
<head>
    <!-- Responsive styles with CSS Grid -->
</head>
<body>
    <h1>Agent Pool Report: {PoolName}</h1>
    <div class="summary">Total agents: X</div>
    <div class="controls">
        <!-- Capability filter input -->
        <!-- Status filter (All/Online/Offline) -->
        <!-- Enabled filter (All/Enabled/Disabled) -->
        <!-- Sort dropdown (includes all capabilities) -->
        <!-- Sort direction -->
        <!-- Export buttons (JSON/CSV) -->
    </div>
    <div class="agent-card" data-status="online" data-enabled="true" data-capabilities='{...}'>
        <!-- Agent info with status indicators -->
        <!-- Collapsible capability sections -->
    </div>
    <!-- Export modal with capability selection -->
    <script>
        <!-- Filter, sort, export, and toggle functions -->
    </script>
</body>
</html>
```

### JSON Schema

```json
{
  "generatedAt": "2026-01-13T12:00:00Z",
  "poolName": "Default",
  "totalAgents": 5,
  "selectedCapabilities": ["Agent.OS", "dotnet"],
  "agents": [
    {
      "id": 1,
      "name": "agent-01",
      "status": "online",
      "enabled": true,
      "version": "3.x.x",
      "osDescription": "Windows 10",
      "capabilities": { "Agent.OS": "Windows", "dotnet": "8.0" }
    }
  ]
}
```

### CSV Structure

```csv
Id,Name,Status,Enabled,Version,OsDescription,Capability1,Capability2,...
1,"agent-01",online,True,"3.x.x","Windows 10","value1","value2",...
```

---

## Error Handling

| Error | Cause | Resolution |
|-------|-------|------------|
| Pool not found | Invalid pool name/ID | Verify pool exists and you have access |
| 401 Unauthorized | Invalid PAT | Generate new PAT with correct permissions |
| 403 Forbidden | Insufficient permissions | Ensure PAT has Agent Pools (Read) scope |
| Network error | Connectivity issues | Check network and organization URL |

---

## Dependencies

| Package | Version | Purpose |
|---------|---------|---------|
| System.CommandLine | 2.0.0-beta4 | Command-line argument parsing |

---

## Changelog

### Version 1.1.0 (2026-01-14)
- **NEW**: Fetches all agents from pool (not just online/enabled)
- **NEW**: Status filter in HTML report (Online/Offline/All)
- **NEW**: Enabled filter in HTML report (Enabled/Disabled/All)
- **NEW**: Visual status indicators (color-coded)
- **NEW**: Sort by status option
- **IMPROVED**: Default filters set to Online + Enabled for familiar behavior
- **IMPROVED**: Export includes actual status and enabled values

### Version 1.0.0 (2026-01-13)
- Initial release
- Agent discovery and capability listing
- HTML, JSON, CSV export formats
- Capability filtering and sorting
- In-browser export with capability selection

---

## Future Enhancements

- [ ] Support for multiple pool queries
- [ ] Agent health monitoring
- [ ] Capability comparison between agents
- [ ] Export to Excel format
- [ ] Configuration file support
- [ ] Caching for large pools
- [ ] Real-time agent status updates

---
