# Carvino Pickup 91 — Cinematic LOD0 Asset Record

## Ownership and rights

- Asset: `CarvinoPickup_91_CinematicLOD0`
- Authoring project: Carvino Drag Sim
- Creation method: original procedural modeling and refinement in Blender 5.2 LTS
- Source basis: the project's original unbadged `CarvinoPickup_91` placeholder proportions
- Design direction: fictional early-1990s compact pickup silhouette; no manufacturer badge or protected model identifier is present on the asset
- Rights: original commercial-use project asset
- External meshes, textures, logos, badges, scans, and game assets: none
- Distribution note: safe to ship as an original unbadged compact pickup subject to the project's normal final legal review

## Geometry record

- Mesh objects: 1,043
- Vertices: 155,076
- Triangles: 300,344
- Asset-space bounds: 2.6000 m wide × 2.6789 m high with open hood × 5.5480 m long
- Asset-space closed-body height excluding the open hood: approximately 2.08 m
- Intended role: cinematic/showroom/garage inspection master; not the default on-track mesh
- Wheel structure: four independent named roots (`CIN_Wheel_FL`, `CIN_Wheel_FR`, `CIN_Wheel_RL`, `CIN_Wheel_RR`)
- Detailed systems: 336 modeled tread blocks per tire, forged wheels, drilled/vented rotors, calipers, studs/lugs, open braced hood, recessed engine-bay aperture, longitudinal V8-style engine representation, radiator/front dress, fuel rails/injectors, headers, hoses/wiring, dual-seat cockpit, gauges/ticks/needles, pedals/shifter, corrugated bed, wheel tubs, tie-downs, tailgate hardware, and multi-element lamp optics

## 4K-ready material layout

The Blender source and FBX use four deterministic 4096-ready texture-set prefixes:

1. `CIN_Pickup_Body_4K__*`
2. `CIN_Pickup_Interior_4K__*`
3. `CIN_Pickup_Mechanical_4K__*`
4. `CIN_Pickup_GlassLights_4K__*`

All 1,043 FBX mesh objects retain UV layers. Current preview appearance uses original procedural materials only. Each prefix is ready to receive 4096×4096 Base Color, Normal, Metallic/Roughness/AO, and optional Emissive texture packages without changing mesh ownership or naming.

## Source and export paths

- Blender source: `Assets/Carvino/Art/Models/Cinematic/Pickup91/CarvinoPickup_91_CinematicLOD0.blend`
- Unity-friendly interchange: `Assets/Carvino/Art/Models/Cinematic/Pickup91/CarvinoPickup_91_CinematicLOD0.fbx`
- Preview: `Assets/Carvino/Art/Models/Cinematic/Pickup91/CarvinoPickup_91_CinematicLOD0_Preview.png`
- Reproducible generator: `Assets/Carvino/Art/Models/Cinematic/Pickup91/CarvinoPickup_91_CinematicLOD0_generator.py`
- Validation script: `Assets/Carvino/Art/Models/Cinematic/Pickup91/validate_pickup_cinematic_roundtrip.py`
- FBX axes: forward `-Z`, up `Y`; metric unit scale
- Animation/bones: none
- Embedded external textures: none
- Preview environment: excluded from the FBX hierarchy

## Validation record

Blender 5.2 LTS source generation reported:

`meshes=1043 vertices=155076 triangles=300344 dimensions=(2.6000,2.6789,5.5480)`

Fresh-scene FBX round-trip import reported:

`CARVINO_PICKUP_ROUNDTRIP_OK meshes=1043 vertices=155076 triangles=300344 dimensions=(2.6000,2.6789,5.5480) wheel_roots=CIN_Wheel_FL,CIN_Wheel_FR,CIN_Wheel_RL,CIN_Wheel_RR materials=58 uv_coverage=100%`

## Safe playable replacement path

Do not overwrite `Assets/Carvino/Art/Models/CarvinoPickup_91.fbx` with this open-hood master. Derive and validate these closed-hood assets first:

- `CarvinoPickup_91_PlayableLOD0.fbx`: 120,000–180,000 triangles, closed hood, retained exterior/interior/brakes, simplified tread, optional engine bay only in inspection mode.
- `CarvinoPickup_91_PlayableLOD1.fbx`: 45,000–80,000 triangles, baked tread/fasteners/lamp ribs, simplified cockpit and brakes.
- `CarvinoPickup_91_PlayableLOD2.fbx`: 15,000–30,000 triangles for distant rivals and trackside views.

Integrate through a Unity `LODGroup`, preserve wheel roots and the existing gameplay pivot, validate wheel rotation/camera framing/colliders, and retain the current gameplay FBX as rollback until the Windows garage and race checks both pass.
