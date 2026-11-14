# Login and Theme Integration Review

## Local Login Gate

The desktop bootstrap now opens `LoginWindow` before `MainWindow`. First use creates a local operator profile with a display name and a 12-character-or-longer passphrase. The profile keeps a random salt and PBKDF2-SHA512 derived hash; no plaintext passphrase is written to `operator-profile.json`. Later use exposes only the unlock route. The profile stays in the user’s local application-data path and has no remote account, OAuth, cloud sync, or upload dependency.

The visual Login gate has a deliberately enlarged, isolated Globe-L mark on the identity side and a compact transparent mark in the application chrome. The rejected screenshot-card treatment is absent.

## Theme Verification

The same Login layout was captured in both live theme variants. Light is white with lime used for primary actions and state; Dark is ink black with deliberate blue primary action and state. Both variants use the same local-access content and do not change the authorization or evidence workflow.

| Evidence | Result |
|---|---|
| `login-light-column-fix.png` | Full local-profile form visible with enlarged isolated Globe-L and white/lime design. |
| `login-dark.png` | Same form visible with black/blue Dark theme after actual UI Automation toggle. |
| `LocalLoginServiceTests` | Profile creation stores no plaintext passphrase, correct unlock succeeds, incorrect unlock fails. |
| Desktop regression | 9 passing Desktop tests after login and theme integration. |

## Remaining Work

The Workstation must receive the same width budget correction as the Login window so the Context Inspector fits at the reviewed narrow native viewport. A future design pass should add keyboard-accessible focus narration to the navigation tree and run a full user-flow capture from local profile creation through an empty Workstation state.
