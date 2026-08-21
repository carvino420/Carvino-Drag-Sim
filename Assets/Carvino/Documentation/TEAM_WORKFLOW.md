# Carvino eight-worker workflow

This is a working development boundary, not eight copies of the game.
Every worker owns a narrow area; the Game Director is the only role allowed to
combine finished work into a release candidate.

| Worker | Owns | Must not change |
| --- | --- | --- |
| Game Director | architecture, integration, release approval | specialist implementations without review |
| Core Engineer | save, settings, input, game state | engine calculations, art assets |
| Vehicle Engineer | engine, drivetrain, turbo, fuel, damage | UI, audio, art |
| Physics Engineer | tires, contact patches, suspension, surfaces | engine tuning logic, UI |
| UI/Tuning Engineer | menus, garage, HUD, dyno, tuning controls | engine simulation and physics equations |
| Audio Engineer | audio assets and telemetry-to-sound playback | engine-state mutation |
| Art/Content | models, materials, tracks, prefabs, LOD/collision | gameplay code |
| QA/Integration | tests, reproducible defects, build validation | feature behavior except test-only code |

## Director checklist for every feature

1. Define the player-facing result and the systems involved.
2. Give each worker a small output with a named contract.
3. Merge into an integration scene only after each output compiles.
4. Run a Windows development build and a short race/garage regression pass.
5. Update the manifest, changelog and known-issues list.

## Example: turbo kit

- Vehicle Engineer adds airflow, spool, wastegate and component limits.
- UI/Tuning Engineer adds boost target and boost-by-gear controls.
- Audio Engineer reads RPM, throttle and boost telemetry to drive spool/BOV sound.
- Art/Content creates an original turbo/intercooler prefab and material.
- Physics Engineer confirms the new wheel torque does not bypass traction.
- QA tests a stock car, a safe turbo tune and an intentionally unsafe tune.

No role duplicates the engine model. Dyno, racing, AI and audio consume the
same authoritative state, with audio strictly read-only.
