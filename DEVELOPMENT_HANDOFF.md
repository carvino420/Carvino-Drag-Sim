# Carvino Drag Sim — Development Handoff

## Current playable build

- Windows executable: `P:\chatgpt projects\Carvino Drag Sim\Builds\Windows\Carvino Drag Sim.exe`
- Unity project: `P:\chatgpt projects\Carvino Drag Sim`
- Unity version: `6000.5.9f1`
- Last verified build: **2026-08-20**, `Carvino pre-build validation passed` and `Build Successful`.

## Current core loop

1. Main Menu provides Career, Free Play, Garage, Dyno & Tune, Profile, Controls, Settings, and Exit.
2. Profile stores a local driver name, rank, wallet, record, current vehicle, engine, and part count.
3. Career gates events by wins; Free Play exposes every race event.
4. Garage buys/installs parts, swaps used/new engines, and repairs engine health.
5. Dyno and racing use the same `DragSimulation` engine model.
6. Races support burnout, staging, tree, AI, shifting, payouts, 1/8-mile or 1/4-mile passes, and save records.

## Content currently present

- Vehicles: original 1993-style three-door hatch and 1991-style compact pickup.
- Hatch engines: D16, B20, K20, K24.
- Pickup engines: V6, I6, small V8, LS 5.3, big block.
- Parts: intake, exhaust, ECU, drag slicks, weight reduction, turbo kit, injectors, fuel pump, clutch, gearset, limited-slip, and axles.
- Tuning: fuel type, AFR/lambda, timing, launch/shift RPM, tire pressure, boost-by-gear, ride height, rebound, anti-squat.
- Failure feedback: knock, fuel-limit/lean, turbo overspeed, overheating, misfire, and failure wear.

## Asset status

Original Blender source and FBX exports:

- `Assets/Carvino/Art/Models/CarvinoHatch_93.blend`
- `Assets/Carvino/Art/Models/CarvinoHatch_93.fbx`
- `Assets/Carvino/Art/Models/CarvinoPickup_91.blend`
- `Assets/Carvino/Art/Models/CarvinoPickup_91.fbx`

`PrototypeSceneBuilder.cs` loads the FBX models for every generated scene, while procedural vehicles remain as a fallback if an asset is missing. The models are deliberately original and unbranded; reference photos were used only for broad era/proportion inspiration.

## Key source locations

- `Assets/Carvino/Runtime/DragSimulation.cs` — shared engine/race/dyno simulation.
- `Assets/Carvino/Runtime/GarageSession.cs` — local save/economy/build persistence.
- `Assets/Carvino/Runtime/MainMenuController.cs` — game hub.
- `Assets/Carvino/Runtime/CareerController.cs` and `CareerProgress.cs` — career flow.
- `Assets/Carvino/Runtime/ProfileController.cs` and `PlayerProfile.cs` — local profile.
- `Assets/Carvino/Runtime/DynoController.cs` — tuning/dyno UI.
- `Assets/Carvino/Runtime/PrototypeRaceController.cs` — race flow.
- `Assets/Carvino/Runtime/TireContactModel.cs` — tire contact patches/deflection.
- `Assets/Carvino/Editor/PrototypeSceneBuilder.cs` — scenes, procedural environment, model placement.
- `Assets/Carvino/Editor/WindowsBuild.cs` — Windows build entry point.

## Known limitations / next priorities

1. Hand-test the compiled executable for final camera framing, menu layout, controller behavior, and vehicle scale after replacing the old procedural meshes.
2. Refine the Blender vehicles: wheel arches, lower body shape, interior/engine bay, separate upgrade visuals, LODs, and colliders.
3. Add an onboarding/tutorial flow for first-time players.
4. Add a clean garage-to-car inspection flow with visual part installation.
5. Add mobile UI/input and verify an Android build.
6. Do not add online currency, purchases, or leaderboards without an authoritative backend.

## Build command

Use Unity batch mode with `Carvino.Editor.WindowsBuild.BuildDevelopment`. It regenerates scenes, runs `CarvinoBuildValidation`, then writes the executable to `Builds/Windows`.

## Legal boundary

Do not copy code, assets, models, textures, audio, branding, or game data from BeamNG, Forza, EV3, No Limit, Hondata/KManager, or Speed Dreams. Speed Dreams can be studied for concepts and architecture, but its GPL licensing means copied code would require GPL obligations for Carvino; keep Carvino implementation original.
