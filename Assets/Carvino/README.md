# Carvino Drag Sim v0.01 foundation

Open `Assets/Carvino/Scenes/Garage.unity` after running
`Carvino > Build v0.01 Prototype Scene` once in the Unity editor.

Prototype controls:

- `1` / `2`: choose hatch or pickup
- `E`: cycle compatible engines
- `U`: toggle starter upgrades
- `Space`: launch
- `R`: reset the pass

The garage has a dyno entry point. Dyno settings persist and affect peak power and
the displayed race tune.

All vehicle names, brands, values, code, and geometry in this folder are original
prototype content. Reference mods are not included.

## Development ownership

The project uses an eight-worker structure so specialists do not overwrite each
other's systems. Start with
`Documentation/TEAM_WORKFLOW.md`, then consult
`Documentation/WORKER_MANIFEST.json` before changing a feature. The shared
simulation boundaries are defined in `Documentation/SYSTEM_CONTRACTS.md`.
