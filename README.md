# CYBER BANK L EDITION

CYBER BANK L EDITION is a local Windows desktop workstation for organizing explicitly authorized assessment engagements, declared scope, imported local evidence, assessor-authored findings, remediation notes, local reports, and history.

It is deliberately not a scanner, exploit runner, payload store, credential manager, target discovery tool, browser automation tool, or network service. It does not launch imported artifacts, contact a target, send a report, or upload project data.

## Solution Layout

| Project | Responsibility |
|---|---|
| `Domain` | Engagement, declared scope, evidence, finding, report, and audit invariants. |
| `Application` | Local workflow commands and policy gates. |
| `Infrastructure` | Isolated project folders, hashing, import metadata, JSON persistence, and local reports. |
| `Desktop` | Avalonia Workstation for local project review. |
| `Cli` | Restricted local project and evidence commands only. |

Run the domain test contract through `test-domain.cmd`. Windows command wrappers set the required environment variables used by the installed .NET toolchain.
