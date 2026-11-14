# CYBER BANK L EDITION — Windows Alpha 0.1.0

**Release date:** 2026-08-24  
**Product:** CYBER BANK L EDITION — authorized-assessment workstation  
**Desktop stack:** .NET 10, C#, Avalonia 12, xUnit  
**Installation model:** self-contained `win-x64`, lowest-privilege local install

## Release Scope

Alpha 0.1.0 is a native Windows desktop program for documenting explicitly authorized assessment work locally. The released vertical slice begins with an authorization gate and declared scope, then supports local project records, asset labels, non-executing evidence import, assessor-authored finding drafts, local report snapshots, activity history, and a restricted local CLI.

| Area | Delivered behavior |
|---|---|
| Engagement gate | Requires a written authorization reference and at least one declared in-scope entry. In-scope and out-of-scope records remain separate. |
| Asset Register | Adds an assessor-entered label only when it links to an exact declared in-scope boundary. URL- and IP-style labels are rejected. |
| Evidence | The user explicitly selects a local artifact. The program copies bytes to the local project, calculates SHA-256, stores an `.evidence` payload, and appends local provenance. No content is executed. |
| Findings | Assessor-authored title, severity, impact, remediation, draft review state, and metadata-only evidence references. |
| Reports | Local JSON and Markdown snapshots include authorization, scope, declared assets, evidence metadata, findings, remediation, audit entries, and a JSON SHA-256. |
| Workstation | Native Engagement Library, Scope Map, Asset Register, Finding Board, Context / Evidence Rail, Activity Ledger, and persistent local-only posture. |
| CLI | `project create`, `project validate`, `evidence import`, and `report export` only. It rejects targets, URLs, IPs, credentials, payloads, shell commands, scanner options, and remote file/output paths. |

## Safety Boundary

> L EDITION is a local documentation and evidence workstation for work authorized outside the program. It does **not** scan, exploit, probe, brute-force, replay credentials, store secrets, launch tools, execute imported material, upload data, or deliver reports externally.

The CLI additionally requires `--name`, `--authorization`, and `--scope-label` before it accepts `project create`; a bare project path cannot create an engagement.

## Verification Evidence

| Verification | Result |
|---|---|
| Final xUnit regression | **42 passed / 0 failed**: Domain 12, Application 1, Infrastructure 5, CLI 17, Desktop 7. |
| Release Desktop build | Succeeded with **0 warnings** and **0 errors**. |
| Self-contained publish | Completed for `win-x64`. |
| Installer compiler | Inno Setup 6.7.3 completed successfully. |
| Silent installation | Completed under `%LocalAppData%\Programs\CyberBank\L Edition`. |
| Installed launch smoke | `CyberBank.LEdition.Desktop.exe` launched successfully after the final silent install; observed process ID `17048`. |
| Visual review | Native-window review completed at `1494 × 958`; Context / Evidence Rail, local-only status, Globe-L, engagement gate, and installer-launched shell are visible. |

## Installer Artifact

| Property | Value |
|---|---|
| File | `artifacts\installer\CyberBank-L-Edition-Alpha-Setup-win-x64.exe` |
| Size | `51,427,466` bytes |
| SHA-256 | `F0BECC228B889F62FE4AE89DE797E6C0788CB07241FBE27B503173845A5F0CCC` |
| Installed executable | `%LocalAppData%\Programs\CyberBank\L Edition\CyberBank.LEdition.Desktop.exe` |
| Installer privileges | `lowest` — no administrator installation required. |

## Review Artifacts

| Artifact | Purpose |
|---|---|
| `.test-artifacts/final-regression-release.log` | Final xUnit evidence. |
| `.test-artifacts/publish-release-final.log` | Self-contained publish output. |
| `.test-artifacts/installed-window-alpha-010-final.png` | Visual smoke capture of the installer-launched application. |
| `docs/release/Alpha-0.1-Visual-Review.md` | Workstation visual review and resolution history. |
| `branding/globe-l-scope-prototype.svg` | Original pre-integration Globe-L vector prototype. |

## Known Alpha Boundaries

The engagement manifest and activity file persist locally and can be reopened. Imported evidence bytes and SHA-256 provenance persist inside the project folder. Asset and finding records are currently maintained in the active local Workstation session and are included in any report snapshot exported from that session; persistence of full asset/finding collections across an application restart belongs to the next storage-slice milestone. This is a product maturity limitation, not a permission or network gap.
