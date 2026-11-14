# CyberBank L Edition

A local workstation organizes authorized assessment engagements, declared scope, imported evidence, assessor-authored findings and remediation notes. Domain objects enforce engagement rules. Infrastructure maintains local records and evidence hashes. The desktop and CLI expose the workflows.

## Engineering scope

The evidence and reporting workflow operates on local project files. The release notes describe login persistence in Alpha 0.1.1.

## Getting started

Use the .NET 10 SDK on Windows. From the repository root, build with `dotnet build CyberBank.LEdition.slnx`. To run the isolated domain suite, use `dotnet test tests/CyberBank.LEdition.Domain.Tests/CyberBank.LEdition.Domain.Tests.csproj`. Start the desktop application with `dotnet run --project src/CyberBank.LEdition.Desktop/CyberBank.LEdition.Desktop.csproj`.

## Status and verification

Windows Alpha 0.1.1 documented.

12 domain tests passed locally. Desktop acceptance tests and release installers were not run.

## Source and evidence

- [src](src)
- [LocalWorkflowAcceptanceTests.cs](tests/CyberBank.LEdition.Desktop.Tests/LocalWorkflowAcceptanceTests.cs)
- [Windows-Alpha-0.1.1-Login-Persistence.md](docs/release/Windows-Alpha-0.1.1-Login-Persistence.md)

## Solution Layout

| Project | Responsibility |
|---|---|
| `Domain` | Engagement, declared scope, evidence, finding, report, and audit invariants. |
| `Application` | Local workflow commands and policy gates. |
| `Infrastructure` | Isolated project folders, hashing, import metadata, JSON persistence, and local reports. |
| `Desktop` | Avalonia Workstation for local project review. |
| `Cli` | Restricted local project and evidence commands only. |

Run the domain test contract through `test-domain.cmd`. Windows command wrappers set the required environment variables used by the installed .NET toolchain.

## Author

Ahmed Mohamed Elsayed Elreefy. Engineering work includes requirements, technical decisions, AI-assisted implementation, debugging and acceptance review.
