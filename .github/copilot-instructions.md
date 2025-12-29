# Veeam Health Check - Copilot Instructions

## Project Overview
A .NET 8 WPF Windows desktop application that generates comprehensive health check reports for Veeam Backup & Replication (VBR) and Veeam Backup for Microsoft 365 (VB365) installations. Outputs HTML, PDF, and PowerPoint reports.

## Architecture

### Core Data Flow
```
PowerShell Scripts → CSV Files → CsvParser → DataFormer → HtmlExporter → HTML/PDF/PPTX
```

1. **Collection** (`Functions/Collection/`): PowerShell scripts collect VBR/VB365 configuration via `PSInvoker.cs`
2. **Parsing** (`Functions/Reporting/CsvHandlers/`): `CCsvParser` reads CSV data into dynamic objects
3. **Analysis** (`Functions/Analysis/DataModels/`): Data models for licenses, SOBR, servers
4. **Reporting** (`Functions/Reporting/Html/`): `CDataFormer` transforms data → `CHtmlExporter` generates output

### Key Classes
- `CGlobals` ([Common/CGlobals.cs](vHC/HC_Reporting/Common/CGlobals.cs)) - Global state, flags, and configuration
- `CVariables` ([Startup/CVariables.cs](vHC/HC_Reporting/Startup/CVariables.cs)) - Path constants and directory management
- `CArgsParser` ([Startup/CArgsParser.cs](vHC/HC_Reporting/Startup/CArgsParser.cs)) - CLI argument handling
- `CDataFormer` ([Functions/Reporting/Html/CDataFormer.cs](vHC/HC_Reporting/Functions/Reporting/Html/CDataFormer.cs)) - Main data transformation (1000+ lines)

## Build & Test

```powershell
# Build
dotnet build vHC/HC.sln --configuration Debug

# Run all tests
dotnet test vHC/HC.sln --no-build --configuration Debug

# Run specific test class
dotnet test vHC/VhcXTests/VhcXTests.csproj --filter "FullyQualifiedName~CredentialHelperTests"

# Publish single-file executable
dotnet publish vHC/HC_Reporting/VeeamHealthCheck.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish/out
```

## CLI Flags
Key command-line arguments (see `CArgsParser.ParseAllArgs()`):
- `/run` - Execute health check without GUI
- `/vbr` or `/vb365` - Target product
- `/scrub:true|false` - Anonymize output
- `/days:7|30|90` - Report days lookback
- `/pdf` `/pptx` - Export formats
- `/security` - Security-only report
- `/remote /HOST=server` - Remote execution

## Naming Conventions

### Class Naming (UPDATED)
- **NEW classes**: Use standard PascalCase WITHOUT `C` prefix
  - ✓ `HealthCheckOptions`, `ProcessRunner`, `LogSanitizer`
  - ✗ `CHealthCheckOptions`, `CProcessRunner`
- **Legacy classes**: Many use `C` prefix (e.g., `CGlobals`, `CDataFormer`)
  - Do NOT add new classes with `C` prefix
  - Gradual migration is in progress
- **Test files**: `*TEST.cs` or `*Tests.cs` suffix
- **PowerShell scripts**: Located in `Tools/Scripts/HealthCheck/`

### Modern Patterns (Preferred)
- Use `HealthCheckOptions` for configuration instead of adding to `CGlobals`
- Use `HealthCheckContext` for runtime state
- Use async methods (`*Async`) for I/O operations
- Use `ProcessRunner` for async process execution

## Project-Specific Patterns

### Configuration (NEW Pattern)
Use `HealthCheckOptions` for new configuration:
```csharp
var options = new HealthCheckOptions
{
    ReportType = ReportType.VbrFull,
    ReportDays = 30,
    Scrub = true
};
options.ApplyToGlobals(); // Bridge to legacy code
```

### Global State (Legacy Pattern)
Configuration flows through `CGlobals` static properties. When adding features:
```csharp
// In CGlobals.cs - add flag
public static bool EXPORTNEWFORMAT = false;

// In CArgsParser.cs - add CLI support
case "/newformat":
    CGlobals.EXPORTNEWFORMAT = true;
    break;
```

### CSV Data Pattern
Data is collected via PowerShell → CSV → parsed dynamically:
```csharp
// Get data from CSV with null-safety
var proxies = CCsvParser.GetDynViProxy() ?? Enumerable.Empty<dynamic>();
foreach (dynamic proxy in proxies)
{
    string name = proxy.Name;
}
```

### Report Generation
New report sections follow this pattern in `CDataFormer`:
```csharp
public CNewTable NewSection()
{
    CNewTable t = new();
    t.AddData(/* transformed data */);
    return t;
}
```

### Test Patterns
Use xUnit with explicit Arrange/Act/Assert. See [test-writer.agent.md](.github/agents/test-writer.agent.md):
```csharp
[Fact]
public void MethodName_Scenario_ExpectedResult()
{
    // Arrange
    var sut = new MyClass();
    // Act
    var result = sut.DoSomething();
    // Assert
    Assert.Equal(expected, result);
}
```

## Critical Gotchas

1. **Static Initialization**: `CVariables` and `CGlobals` have circular dependency potential. Test changes with `StaticInitializationTEST.cs`

2. **Windows-Only**: Requires Windows for WPF, PowerShell integration, and Veeam cmdlets

3. **PowerShell Detection**: `PSInvoker` auto-detects PS5 vs PS7; prefer PS7 when available

4. **Scrubbing**: When `CGlobals.Scrub=true`, sensitive data is anonymized - test both modes

5. **Output Paths**: Default output is `C:\temp\vHC\`. Path structure: `{base}\{Anonymous|Original}\VBR\{server}\{timestamp}`

## Related Projects (Same Workspace)
- **VHC-TestAgent**: Copilot Studio agent for analyzing VHC HTML reports using GPT-5
- **AI-Agent-Prompts**: Knowledge base documents for the AI agent (orchestration, security analysis, reporting prompts)
