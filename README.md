# Infonetica Workflow Engine

## Quick Start

1. **Requirements:**
   - [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

2. **Build & Run:**
   ```bash
   dotnet build
   dotnet run --project Infonetica.WorkflowEngine
   ```
   The API will start and listen on the default port (see launchSettings.json for details).

3. **Configuration:**
   - Edit `appsettings.json` or `appsettings.Development.json` for environment-specific settings.

## Assumptions, Shortcuts, and Known Limitations

- Assumes a local development environment with .NET 8.0 installed.
- No database or external service integration is required for initial run.
- API documentation (Swagger UI) is available by default in the Development environment.
  - Access it at: `http://localhost:5087/swagger` (or the port specified in your launchSettings.json).
  - Swagger is enabled by default in `appsettings.Development.json` and may be disabled in production for security reasons.
  - To enable/disable Swagger, adjust the relevant settings in your `Program.cs` or configuration files.
  - Environment can be set using the `ASPNETCORE_ENVIRONMENT` variable (e.g., `Development`, `Production`).
- For any ambiguity, see comments in code or contact the author.

---

**Repository URL:** https://github.com/krisdheniya/Workflow-System.git 
