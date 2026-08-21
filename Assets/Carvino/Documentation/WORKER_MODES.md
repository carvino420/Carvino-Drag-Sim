# Optimized worker types

Carvino does not need twenty agents running at once. That burns time, creates
merge conflicts and makes debugging harder. Use the smallest group that can
finish the current feature safely.

## Recommended default: four active lanes

| Worker type | Best work | Normal timing | What it returns |
| --- | --- | --- | --- |
| Director/Integrator | scope, contracts, reviews, merges | always active | one approved build path |
| Systems specialist | engine, tuning, physics, save/state | one feature at a time | code plus data and test notes |
| Player-experience specialist | menu, garage, HUD, controls, audio feedback | one feature at a time | player-facing flow plus accessibility check |
| QA/Build specialist | compilation, regression routes, performance checks | after each merge | evidence, not guesses |

This is the right v0.01 team. It keeps the number of simultaneous code edits
low while still allowing progress in parallel.

## Specialists that activate only when needed

| Worker | Activate for | Keep inactive when |
| --- | --- | --- |
| Art/3D | a licensed/original model, track prop, material, LOD or collision task is ready | there is no approved source asset or reference yet |
| Audio | a stable telemetry value exists to drive a new sound | engine inputs are still changing daily |
| Economy/Content | enough cars, parts and races exist to balance | the core garage/race loop is changing |
| AI | race flow and physics are stable enough for fair opponents | launch/traction calculations are still being rewritten |
| Optimization | a measured performance or memory issue exists | before profiling shows a problem |

## Best model/worker behavior by job

## Recommended AI model by job

| Role | Recommended model | Why |
| --- | --- | --- |
| Game Director / integration | GPT-5.6 Sol | strongest tradeoff analysis and safest cross-system changes |
| Engine and physics | GPT-5.6 Sol | handles interacting math, state and failure edge cases best |
| UI, menu and garage | GPT-5.6 Terra | strong implementation quality with faster iteration |
| Art/content planning | GPT-5.6 Terra | good visual/content judgment; use image or Blender tools for the assets themselves |
| QA, build-log review, data cleanup | GPT-5.6 Luna | fast and cost-efficient for repeatable evidence-based checks |

Use the higher-reasoning Sol setting for architecture or difficult physics only.
Use Terra for most ordinary feature work. Use Luna for checks that have a clear
pass/fail result. The Director can promote a task to Sol when an agent finds an
ambiguous design decision, rather than paying that cost for every small task.

- **Architecture and integration:** deliberate, conservative; reads broadly but
  writes narrowly. It does not invent replacement systems.
- **Math-heavy vehicle work:** focused specialist; one authoritative simulation,
  data-driven inputs, deterministic tests.
- **UI and content work:** visual, iterative specialist; changes only its screen
  or asset area and confirms navigation still works.
- **QA:** skeptical, read-only by default; reports logs and repeatable steps,
  never calls a feature tested without evidence.
- **Art:** asset-first; checks license, scale, LODs, materials and collision
  before it reaches the playable build.

## Work packet format

Every worker receives a small packet:

1. One player-facing outcome.
2. Exact files it owns.
3. Inputs it may read.
4. Outputs/contracts it must preserve.
5. One build or regression check.

The Director merges only completed packets. Large requests, such as “add a
turbo kit,” are divided into these packets instead of given to one worker as an
unbounded rewrite.
