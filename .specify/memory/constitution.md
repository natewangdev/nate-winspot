<!--
Sync Impact Report
- Version change: 1.3.1 → 1.3.2 (PATCH: Technology Constraints capture hotkey F12 → fixed PrtSc)
- Modified principles: Technology Constraints (capture hotkey wording)
- Added: none
- Removed: none
- Follow-up TODOs: none
-->
# WinSpot Constitution

## Core Principles

### I. Spec-Driven Delivery
Every feature MUST begin as a Spec Kit artifact (`spec.md` → `plan.md` → `tasks.md`)
before application code is written. Implementation MUST map to tasks and remain
traceable to user stories. Rationale: keeps scope explicit and prevents
undocumented feature creep in a desktop Win32 tooling product.

### II. Non-Invasive Window Introspection
The tool MUST observe target windows only through documented Win32 public APIs
(for example window metrics, client coordinates, pixel sampling, and client-area
capture). It MUST NOT inject code into target processes, scrape private memory,
or automate UI by sending input unless a future constitution amendment explicitly
allows it. Rationale: trust, safety, and predictable behavior across apps.

### III. Accuracy Over Cleverness
Displayed handle, title, client size, relative coordinates, and sampled colors
MUST match system-reported values within documented tolerances. High-DPI and
multi-monitor coordinate conversion MUST be handled correctly. When accuracy
cannot be guaranteed (elevated target, protected desktop, capture failure), the
UI MUST surface a clear error instead of silent wrong data.

### IV. User-Copyable Insights
Any value the user is expected to reuse (HWND, title, coordinates, colors, region
rectangles, save paths) MUST be selectable or one-click copyable in a stable
text format. Rationale: WinSpot is a measurement/assistant tool; copy friction
defeats the product purpose.

### V. Simplicity & Single-Binary Delivery
Prefer the smallest design that satisfies the current spec (YAGNI). The shipping
artifact MUST be a self-contained single-file Windows executable for the
supported RID. New dependencies require justification against this principle.
Configuration MUST persist locally without requiring a cloud service.

### VI. Bilingual Spec Companion (Chinese Viewing Copy)
Every feature requirements document that Spec Kit consumes as `spec.md` MUST
have a matching Chinese companion file named `spec.zh-CN.md` in the same feature
directory. Whenever `spec.md` is created or updated, `spec.zh-CN.md` MUST be
updated in the same change so content stays information-equivalent (same
stories, requirements, success criteria, and assumptions).

The Chinese companion is **for human viewing only**. It MUST NOT be the input
to Spec Kit skills, scripts, or gates (`/speckit-specify`, `/speckit-plan`,
`/speckit-tasks`, `/speckit-implement`, `/speckit-analyze`, `/speckit-converge`,
checklists that feed those steps, etc.). Spec Kit workflow artifacts and path
resolution MUST continue to use English `spec.md` (and other English Spec Kit
files) exclusively. Rationale: bilingual readability for Chinese readers without
perturbing Spec-Driven automation.

### VII. Bilingual README with Cross-Links
The repository MUST maintain both `README.md` (English, primary) and
`README.zh-CN.md` (Simplified Chinese). Each MUST include a clearly visible
markdown link to switch to the other language near the top of the document.
Whenever product behavior, setup, configuration, or Spec Kit usage described in
README changes, **both** README files MUST be updated together to remain
information-equivalent. After every requirements/spec change, authors and agents
MUST explicitly consider whether README content needs an update and either
update both languages or record why no README change is required. Rationale:
discoverability for Chinese and English readers and docs that do not drift from
the product.

### VIII. Product SemVer & Automated Releases
The **product** version (shipped WinSpot binary and GitHub Release) MUST follow
[Semantic Versioning](https://semver.org/) and MUST be distinct from this
constitution’s own version line.

Mandatory rules:

1. Git tags MUST use the form `vX.Y.Z` (for example `v0.1.0`, `v1.2.3`). Optional
   pre-release suffixes MAY use `vX.Y.Z-beta.N` (or equivalent SemVer pre-release).
2. Releases MUST be **tag-driven**: pushing a `v*` tag is the sole trigger for
   CI to publish. Merges to `main` MUST NOT create tags or GitHub Releases.
3. The git tag is the source of truth for the product version. CI MUST strip the
   leading `v` and inject that version into the published `WinSpot.exe` metadata
   (tag `v1.2.3` ↔ product version `1.2.3`; informational version MAY retain a
   pre-release suffix when present on the tag).
4. A GitHub Release MUST be created for the pushed tag and MUST attach the
   published single-file `WinSpot.exe`.
5. Do not reuse a tag / version that already exists as a GitHub Release.

Rationale: explicit, intentional releases (same model as nate-game-engine);
predictable artifacts that match what was tagged.

## Technology Constraints

- Stack MUST be C# and WPF on a current LTS .NET (8+).
- Target platform MUST be Windows 10/11 x64.
- Publish MUST produce a single-file, self-contained `win-x64` executable.
- Global hotkeys (fixed PrtSc / Print Screen for client capture in v1) MUST degrade gracefully when
  registration fails (conflict messaging; no crash).
- Settings (save directory, image format, JPEG quality, filename template) MUST
  persist under a per-user local config location.
- Release packaging MUST inject or set the product version from the Git tag so
  published `WinSpot.exe` metadata matches `vX.Y.Z`.

## Development Workflow

- Follow Spec Kit skills in order for new work: constitution (once) → specify →
  plan → tasks → implement → converge.
- Optional quality gates (`clarify`, `checklist`, `analyze`) SHOULD run before
  large implementation pushes.
- Each user story SHOULD remain independently demoable after its phase completes.
- Out-of-scope items listed in the active feature spec MUST NOT be implemented
  without an amended spec.
- After creating or editing English `spec.md`, update `spec.zh-CN.md` in the same
  change (Principle VI). Do not point Spec Kit at the Chinese file.
- After creating or editing either README language, update the other language and
  keep cross-links working (Principle VII). After spec/requirements changes,
  decide and act on README impact before closing the change.
- Prefer Conventional Commit messages on work merged to `main` for readable
  history and release notes (they do **not** auto-bump or auto-release).
- To ship: after the desired commit is on the remote (typically `main`), create
  and push a tag `vX.Y.Z` (for example `git tag v0.1.0 && git push origin v0.1.0`).

## Governance

This constitution supersedes informal coding habits when they conflict.
Amendments require: (1) documented rationale in the Sync Impact Report comment,
(2) semantic version bump (MAJOR for incompatible principle changes, MINOR for
new principles/sections, PATCH for clarifications), (3) update of
`LAST_AMENDED_DATE`. PRs and agent runs MUST verify compliance with Core
Principles and Technology Constraints before merging or claiming completion.
Complexity beyond a single WPF app + Win32 interop layer MUST be justified in
the feature plan Complexity Tracking table.
Product SemVer tags (`vX.Y.Z`) MUST NOT be conflated with constitution versions.

**Version**: 1.3.2 | **Ratified**: 2026-09-27 | **Last Amended**: 2026-09-30
