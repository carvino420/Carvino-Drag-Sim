# Current integration sprint

## Goal

Make the v0.01 desktop vertical slice more reliable and easier to play without
breaking the garage-to-race loop.

## Active work lanes

| Lane | Owner | Deliverable | Merge requirement |
| --- | --- | --- | --- |
| Vehicle/Engine | Vehicle Engineer | One data-driven simulation or traction improvement | compile plus a safe default tune |
| UI/Tuning | UI/Tuning Engineer | One player-facing menu, garage or tuning-flow improvement | keyboard/controller route remains usable |
| QA/Integration | QA/Integration | Fresh Windows build report | no build errors; log attached |
| Integration | Game Director | Review, merge and release note | no ownership conflicts |

## Non-negotiable regression route

1. Launch to main menu.
2. Enter garage and select each starter vehicle.
3. Change a part or tune setting.
4. Enter the drag strip, stage, launch and reach results.
5. Return to garage, close and relaunch to verify save persistence.

## Deferred this sprint

- External pickup import: wait for a downloadable, license-verified source file.
- New high-resolution textures: wait until the vehicle asset and target platform
  budget are locked.
- Multiplayer/backend: after the offline core loop is stable.
