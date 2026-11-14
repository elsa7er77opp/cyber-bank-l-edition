# CYBER BANK L EDITION — Material Desktop Template Selection

## Selected Template Pattern

The source reference was reviewed for hierarchy and component discipline, not for artwork, sample data, or direct replication. The chosen direction is its **classic three-column desktop software layout**: a compact application bar, expandable navigation tree, tabbed work area, and resizable contextual inspector. This pattern is suited to L EDITION because local assessment work has a stable project context, an active work surface, and detail/provenance that should be visible without becoming a fake analytics dashboard.

> Source finding: the referenced desktop system describes an application bar, navigation tree, compact tabs, status cards, text-edit cards, and a classic three-column layout designed for complex desktop software. [1]

## L EDITION Adaptation

| Template pattern | L EDITION adaptation | Explicit exclusion |
|---|---|---|
| Application bar | Project title, local-only state, search within local records, theme switch, export snapshot. | No cloud account, user tracking, or remote connection status. |
| Expandable navigation tree | Engagement, Scope, Materials, Evidence, Findings, Reports, Activity. | No threat monitor, network, target, attack, or scanner route. |
| Compact tabs | `Overview`, `Boundary`, `Materials`, `Findings`, `Delivery` in the center workspace. | No dashboard metrics or operational target control. |
| Status cards | Authorization ready/blocked, scope count, local evidence count, finding draft count. | No made-up security score, threats, IPs, traffic graph, or mitigated events. |
| Context inspector | Selected evidence metadata, SHA-256, linked asset/finding, report readiness. | No remote data, payloads, credentials, execution controls, or uploads. |
| Activity panel | Actual local project events from the append-only ledger. | No synthetic system logs. |

## Theme System

The application exposes one workflow in two first-class themes. The first launch defaults to **Light**. Theme is an interface preference only and cannot change permission, scope, file-import, report-export, CLI, or data behavior.

| Token role | Light theme — White / Lime | Dark theme — Black / Blue |
|---|---|---|
| Canvas | `#F7F9FC` | `#0B1020` |
| Chrome | `#FFFFFF` | `#111A2E` |
| Raised panel | `#FFFFFF` | `#151F35` |
| Primary ink | `#162033` | `#E7F0FF` |
| Secondary ink | `#5E6C80` | `#9FB2CE` |
| Divider | `#DDE4EE` | `#273652` |
| Primary action | `#9BEA22` | `#4EA4FF` |
| Action ink | `#102000` | `#071525` |
| Success/state | `#56A800` | `#66C2FF` |
| Focus ring | `#65B30B` | `#78B8FF` |

The lime is reserved for decisive local actions and scope-ready state in Light mode. It is not a broad background color. Dark mode uses a deliberate ink-black/blue system, never a palette inversion or gray-only fallback.

## Information Architecture

```text
Application bar
├── local case name / local-only state / local search / Light-Dark switch / export
Navigation tree
├── Engagement
├── Boundary
├── Materials
├── Evidence
├── Findings
├── Reports
└── Activity
Workspace tabs
├── Overview
├── Boundary
├── Materials
├── Findings
└── Delivery
Context inspector
└── selected local record metadata and readiness
```

## Acceptance Criteria

The result should look like a serious Material-inspired desktop application with a coherent 8px rhythm, a clearly active navigation item, compact tabs, real status cards, and an inspector whose contents come from L EDITION state. It must look materially different from the rejected dark screenshot: no giant logo card, no synthetic security graphs, no fake threat table, no IP addresses, and no terminal-style event fiction.

## Reference

[1] [Prototypr — Desktop design templates: Material UI for dashboards, software and web tools](https://blog.prototypr.io/desktop-design-templates-material-ui-for-dashboards-software-and-web-tools-e6b77f38e63e)
