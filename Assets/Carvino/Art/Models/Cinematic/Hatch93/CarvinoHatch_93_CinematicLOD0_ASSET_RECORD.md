# Carvino Hatch 93 — Cinematic LOD0 Asset Record

## Ownership and rights

- Asset: `CarvinoHatch_93_CinematicLOD0`
- Authoring project: Carvino Drag Sim
- Creation method: original procedural modeling and refinement in Blender 5.2 LTS
- Source basis: the project's original `CarvinoHatch_93` placeholder proportions
- Rights: original commercial-use project asset
- External meshes, textures, logos, badges, and trademarked model identifiers: none
- Distribution note: safe to ship as an original unbadged fictional early-1990s compact three-door hatch, subject to the project's normal final legal review

## Geometry record

- Mesh objects: 1,379
- Vertices: 162,388
- Triangles: 318,684
- Asset-space bounds: 2.1643 m wide × 1.6632 m high × 4.2820 m long
- Intended role: cinematic/showroom/garage inspection master; not the default on-track mesh
- Wheel structure: four independent named wheel roots (`CIN_Wheel_FL`, `FR`, `RL`, `RR`)
- Detailed systems: modeled street/drag tread, forged wheels, vented/drilled rotors, calipers, lamp reflectors/lens ribs, open hood, engine-bay aperture, transverse four-cylinder, radiator, intake, header, battery, hoses, cockpit seats/stitching, gauges, vents, shifter, panel seams and hardware

## 4K-ready material layout

The FBX and Blender source group materials under four deterministic texture-set prefixes:

1. `CIN_Hatch_Body_4K__*`
2. `CIN_Hatch_Interior_4K__*`
3. `CIN_Hatch_Mechanical_4K__*`
4. `CIN_Hatch_GlassLights_4K__*`

Every exported mesh has a UV layer. Current appearance uses original procedural materials only. The four groups are prepared for later 4096×4096 Base Color, Normal, Metallic/Roughness/AO, and optional Emissive texture authoring.

## Export and validation

- Blender source: `CarvinoHatch_93_CinematicLOD0.blend`
- Unity-friendly interchange: `CarvinoHatch_93_CinematicLOD0.fbx`
- Preview: `CarvinoHatch_93_CinematicLOD0_Preview.png`
- FBX axes: forward `-Z`, up `Y`; metric unit scale
- Animation/bones: none
- Embedded external textures: none
- Preview environment: excluded from the FBX hierarchy
- Blender and round-trip FBX validation must both report 250,000–320,000 triangles, four wheel roots, complete UV coverage, and all four texture-set groups.

## Safe playable replacement path

Do not replace `Assets/Carvino/Art/Models/CarvinoHatch_93.fbx` directly with this open-hood master. Derive two closed-hood exports from this source:

- `CarvinoHatch_93_PlayableLOD0.fbx`: 120,000–180,000 triangles, closed hood, retained exterior/interior/brakes, simplified tire tread, engine bay disabled outside inspection mode.
- `CarvinoHatch_93_PlayableLOD1.fbx`: 45,000–80,000 triangles, baked tread/fasteners/lamp ribs, simplified cockpit and brakes.

Integrate those through a Unity `LODGroup`, preserve the existing gameplay root/pivot/wheel names, validate wheel rotation and camera framing, then switch the garage/race prefab reference. Keep the current placeholder as rollback until a Windows build and garage/race visual test both pass.
