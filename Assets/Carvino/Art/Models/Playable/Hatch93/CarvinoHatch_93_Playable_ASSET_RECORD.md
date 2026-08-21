# Carvino Hatch 93 — Playable LOD Asset Record

## Ownership and safe-use record

- Asset: `CarvinoHatch_93_Playable`
- Author/rights holder: Carvino Drag Sim project
- Method: original procedural/modeling work derived only from the project's original cinematic hatch master and original closed body shell
- Rights: original commercial-use project asset
- External meshes, scans, photos, logos, badges, wordmarks, or proprietary game assets: none
- Design: fictional, unbadged early-1990s compact three-door hatch; final commercial release remains subject to normal legal review

## Playable geometry

- LOD0: 148,128 triangles; full exterior silhouette, glass, panel detail, cockpit, lamp internals, multi-piece wheels, brakes, and modeled tread
- LOD1: 60,572 triangles; same body silhouette, simplified lamp/cockpit hardware and reduced wheel/tread density
- LOD2: 31,028 triangles; same body silhouette, distance cabin, exterior lighting forms, and reduced wheel/brake geometry
- Bounds: approximately 2.1643 m wide × 1.2787 m high × 4.2820 m long
- Root: `CarvinoHatch_93`, origin `(0,0,0)`, identity rotation/scale
- LOD0 wheel roots: `Wheel_FL`, `Wheel_FR`, `Wheel_RL`, `Wheel_RR`
- Wheel centers: X ±0.86 m; Y 0.42 m; front Z 1.22 m; rear Z -1.25 m
- Configuration: closed continuous hood/body surface; engine-bay geometry excluded from the default playable car

## Four 4K-ready material groups

All exported meshes have UV channels and are assigned beneath these deterministic groups:

1. `CARVINO_Hatch_Body_4K__*`
2. `CARVINO_Hatch_Interior_4K__*`
3. `CARVINO_Hatch_Mechanical_4K__*`
4. `CARVINO_Hatch_GlassLights_4K__*`

The material slots are 4K-ready; this asset does not duplicate bitmap textures. Existing original Carvino 4K source libraries can be mapped in Unity as the dedicated vehicle material-authoring pass continues.

## Unity integration

- Staged combined FBX: `Assets/Carvino/Art/Models/Playable/Hatch93/CarvinoHatch_93_Playable.fbx`
- Reproducible Blender source: `CarvinoHatch_93_Playable_LODs.blend`
- Individual review exports: `CarvinoHatch_93_PlayableLOD0/1/2.fbx`
- Import behavior: `HatchPlayableModelPostprocessor.cs` adds a three-level `LODGroup` at 58%, 24%, and 4% relative screen height.
- Gameplay collision and straight-line physics remain under existing Carvino runtime authority; no mesh collider is added.
- The legacy gameplay FBX must remain available until staged Unity import, hierarchy, camera framing, and Windows build checks pass.

## Validation

- Blender 5.2 round-trip FBX: passed
- Triangle budgets: passed at 148,128 / 60,572 / 31,028
- UV coverage: passed, no mesh missing a UV layer
- Wheel names/pivots: passed on all individual exports; LOD0 gameplay names preserved in the combined export
- Four material groups: passed
- Studio preview: `CarvinoHatch_93_PlayableLOD0_Preview.png`
- Review date: 2026-08-21
