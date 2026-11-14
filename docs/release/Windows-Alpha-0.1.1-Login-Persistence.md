# CYBER BANK L EDITION — Windows Alpha 0.1.1

**Release focus:** Local Login, Light/Dark themes, restart-safe project snapshots, and the updated self-contained Windows installer.

## What Changed

| Area | Verified change |
|---|---|
| Local Login | A first-run local profile uses PBKDF2-SHA512 with a random salt and fixed-time verification. The passphrase is not stored as text and no account, cloud, or remote authentication service is used. |
| Themes | The desktop contains a white/lime Light theme and a black/blue Dark theme behind a real state toggle. Both retain the same local-only workflow. |
| Project persistence | The local manifest now stores and restores the declared engagement, assets, evidence metadata, findings, and finding-to-evidence references. Imported artifact bytes remain under the local evidence folder and are never executed during reopen. |
| Desktop workflow | On a project change, the Workstation saves the snapshot; on opening, it loads the most recently modified local project manifest when available. |
| CLI continuity | Local `evidence import` persists metadata and a following `report export` receives the restored assets/evidence/findings instead of exporting empty collections. |
| Installer | A fresh self-contained `win-x64` installer was compiled and silently installed under LocalAppData. |

## Verification

| Layer | Result |
|---|---|
| Domain | 12 passed |
| Application | 1 passed |
| Infrastructure | 6 passed |
| CLI | 17 passed |
| Desktop | 11 passed |
| Total | **47 passed, 0 failed** |
| Release build | 0 warnings, 0 errors |
| Installer size | 55,244,816 bytes |
| Installer SHA-256 | `507D808C19167CD7CFEB59BA96D29A13CD73D64466C9FAD780BC5CF25DAA9A09` |

## Installer Smoke Result

The latest installer completed a silent LocalAppData installation and launched `CyberBank.LEdition.Desktop.exe`. The test profile created for automated smoke verification was explicitly removed afterwards, so the user receives the first-run profile creation flow rather than an unknown test passphrase.

The remote Windows capture service initially starts newly launched windows in a minimized state. Restoring the installed window produces the reviewed native login screen. This is documented as a remote-session capture limitation; the remaining Workstation handoff/inspector visual gate stays open until it is reviewed in a normal visible desktop state.

## Deliberate Boundaries

L EDITION remains a local authorized-assessment record and evidence workstation. It does not scan, connect to targets, run payloads, execute evidence, brute-force credentials, store operational credentials, use remote commands, or upload reports.
