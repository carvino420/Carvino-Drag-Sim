# Asset source register

Record an entry here before importing an external asset into the playable game.

| Asset ID | Asset | Source/author | License | Commercial use verified | PC tier | Mobile tier | Status |
| --- | --- | --- | --- | --- | --- | --- | --- |
| carvino-pc-procedural-pbr-material-library-01 | Five original tileable PBR-style surface sets | Carvino project, self-authored deterministic procedural generator | Original | Yes | five 2048px albedo / normal / packed-mask sets; BC7 Standalone, mipmapped, aniso 8 | exclude from prototype or generate 1024px derivatives later | Imported; ready for scene/material integration |
| carvino-pc-ultra-track-atmosphere-01 | Original procedural PC Ultra+ lighting, lane treatment, and atmosphere | `Assets/Carvino/Editor/UltraTrackAtmosphereBuilder.cs` | Original | Yes | 10 low-poly lighting towers, visual-only lane surfaces, 21 soft-shadow lights, one directional key | omit towers, lane accents, and added lights | In game |
| carvino-hatch-93 | Original 1993-era compact three-door hatch | Carvino project | Original | Yes | 24,604-triangle LOD0 + Ultra+ procedural PBR v2 | future reduced LOD required | In game - PC material pass 2 |
| carvino-pickup-91 | Original early-1990s compact pickup | `Assets/Carvino/Art/Models/CarvinoPickup_91.blend` | Original | Yes | 22,568-triangle LOD0 + Ultra+ procedural PBR v2 | future reduced LOD required | In game - PC material pass 2 |
| carvino-trackside-scoreboards-01 | Original procedural finish-line scoreboards | `Assets/Carvino/Editor/TracksideScoreboardBuilder.cs` | Original | Yes | primitive geometry, no textures | primitive geometry, no textures | In game |
| carvino-pc-garage-bay-dressing-01 | Original garage workbench, locker, lighting, and bay markers | `Assets/Carvino/Editor/GarageBayDressingBuilder.cs` | Original | Yes | 30 static primitives, no textures | can be omitted or merged into lower-detail garage tier | In game |
| carvino-starter-pavilion-01 | Original procedural starter-side timing pavilion | `Assets/Carvino/Editor/TracksideStarterPavilionBuilder.cs` | Original | Yes | 24 static primitives, no textures | can be omitted from mobile or merged into a low-detail static strip pass | In game |
| carvino-pc-prepped-asphalt-01 | Original prepped drag-strip asphalt material | `Assets/Carvino/Art/Textures/carvino_pc_prepped_asphalt_01.png` | Original | Yes | 2048px, mipmapped, aniso 8, high-quality standalone compression | downscale to 1024px / lower compression tier | In game |
| carvino-pc-garage-presentation-01 | Original garage presentation lighting and wall detail | `Assets/Carvino/Editor/GaragePresentationBuilder.cs` | Original | Yes | 19 static primitives and 3 no-shadow spot lights | omit or reduce to one light and no wall detail | In game |
| carvino-race-presentation-01 | Original procedural start/finish framing and distance beacons | `Assets/Carvino/Editor/TracksideRacePresentationBuilder.cs` | Original | Yes | 42 static primitives, no textures | omit the finish arch and reduce distance beacons | In game |

## carvino-pc-procedural-pbr-material-library-01 review

- **Source / author:** Original Carvino project artwork generated deterministically from a self-authored local C# texture generator. No web photo, scan, material pack, logo, trademark, game extraction, or third-party source texture was used. The Unity-side import policy is `Assets/Carvino/Editor/PbrTextureImportRules.cs`.
- **Rights / branding:** Carvino owns this project-authored library and it is cleared for commercial use. All surfaces are generic/unbranded and contain no badge, wordmark, product label, venue identity, or recognizable source photograph.
- **Contents / source resolution:** Five seamless 2048 × 2048 RGBA PNG material sets (15 PNGs total): `rubbered_drag_asphalt`, `garage_concrete`, `painted_metal`, `brushed_steel`, and `bedliner`. Each set has `_albedo` (sRGB), `_normal` (linear tangent-space normal), and `_mask` (linear packed R=metallic, G=ambient occlusion, B=perceptual roughness, A=normalized height). Total PNG source footprint is 37.63 MiB before Unity import.
- **PC import / runtime budget:** Unity imports all fifteen assets with repeat wrap, trilinear filtering, mipmaps, streaming mipmaps, anisotropic level 8, 2048px cap, and Standalone BC7 High Quality compression. A fully resident set is approximately 16 MB in BC7 after the mip chain; the complete five-set PC library is approximately 80 MB when every map is resident. PC Ultra+ should stream only material sets that the active garage/strip scene needs.
- **Mobile / lower tier:** These are authoring-quality PC assets. The mobile prototype should omit the library or use deliberately generated 1024px derivatives with lower-quality platform compression; no mobile derivative is registered as shipped art yet.
- **Collision / use:** Texture-only assets: no colliders, meshes, scenes, materials, prefabs, runtime simulation, or legacy texture files were changed in this pass. Future materials can assign the sets to original dragway lane meshes, garage props, original vehicle metal, and pickup bed surfaces.
- **Review:** 2026-08-21, Carvino PC Material Library worker. PNG signatures verified for all 15 files; Unity batch import succeeded with exit code 0 and confirmed normal maps, non-sRGB normal/mask data, sRGB albedo, repeat wrap, mipmaps, aniso 8, and Standalone BC7 settings.

## carvino-pickup-91 review

- **Source / author:** Original Carvino mesh authored in Blender 5.2 at `Assets/Carvino/Art/Models/CarvinoPickup_91.blend`; Unity consumes the matching `CarvinoPickup_91.fbx`.
- **Rights / branding:** Original geometry and generated materials. No Chevrolet badge, wordmark, logo, copied mesh, external texture, or protected game asset is present. The proportions reference the broad compact-pickup design language of the early 1990s while retaining original body, fascia, bed, trim, and wheel geometry.
- **Geometry:** 22,568 triangles in the PC LOD0 source. The cab, lower body, hood/fenders, narrow flare bed, tailgate, bumpers, glass, lamps, interior, and underbody details are separate named objects. Four independent `Wheel_FL`, `Wheel_FR`, `Wheel_RL`, and `Wheel_RR` roots are retained for later suspension and wheel animation. No reduced LOD is included in this pass.
- **Textures / materials:** No external or embedded bitmap textures. Ultra+ v2 uses original procedural Blender node materials: layered metallic-blue paint with clearcoat and microflake response, tinted transmissive glass, detailed lamp-lens surfaces, drag-radial grain, coarse bed liner, interior grain, brushed bumpers, and machined/dark wheel metals. FBX-compatible base PBR values are retained for Unity import. Studio review render: `Assets/Carvino/Art/Previews/CarvinoPickup_91_UltraPlus_MaterialPreview.png` (1600 × 900 PNG).
- **Collision / import:** Visual mesh only; gameplay keeps its existing simplified vehicle physics/collider authority. FBX exports Y-up with a stable root, approximately 2.65 m wide including mirrors, 2.09 m high including tires, and 5.51 m long including bumpers.
- **Review:** 2026-08-21, Carvino PC Ultra+ Vehicle Materials worker. Material-only geometry lock preserved 120 mesh objects, 11,514 vertices, 11,097 polygons, and 22,568 evaluated triangles; Blender source and exported FBX retain the original names and upright axis convention.

## carvino-pc-garage-presentation-01 review

- **Source / author:** Original Carvino procedural geometry and lighting, created in `GaragePresentationBuilder.cs`.
- **Rights / branding:** No external mesh, image, logo, badge, signage, text, or texture used; suitable for commercial distribution.
- **Geometry:** 19 low-poly Unity primitive renderers: a rear wall treatment, trim, emissive strips, and an inset service-bay plate. Colliders are removed because every prop is visual-only.
- **Lighting / tiers:** Three dynamic spot lights with shadows disabled for clean vehicle presentation at a minimal PC cost. The mobile tier can omit the complete pass or retain a single non-shadow fill light.
- **Unity setup:** Baked into the Garage scene via **Carvino → Art → Add PC Garage Presentation Pass**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino PC Visual-Quality worker.

## carvino-race-presentation-01 review

- **Source / author:** Original Carvino procedural geometry created in `TracksideRacePresentationBuilder.cs`.
- **Rights / branding:** No external mesh, image, badge, logo, venue mark, signage, text, or source texture used; suitable for commercial distribution.
- **Geometry:** A visual-only starting truss, starter pedestals/beacons, finish arch, and paired distance beacons. Approximately 42 low-poly Unity primitive renderers; colliders are removed, preserving race driving and timing behavior.
- **Textures / tiers:** None. Flat metal/concrete and emission materials only. PC uses the complete pass; mobile may remove the finish arch and retain only a reduced set of five distance blocks.
- **Unity setup:** Baked into the QuarterMilePrototype scene through **Carvino → Art → Add Race Presentation Pass**. The editor helper is excluded from player builds. It does not rebuild generated scenes or modify runtime code.
- **Review:** 2026-08-21, Carvino Race Presentation worker.

## carvino-pc-prepped-asphalt-01 review

- **Source / author:** Original Carvino texture created for this project; no external photo, logo, or copied track material was used.
- **Rights / branding:** Commercially usable original asset. No manufacturer or venue marks are present.
- **Resolution / tiers:** 2048 × 2048 PNG source for the PC prototype. Unity import uses repeat wrapping, mipmaps, anisotropic filtering level 8, and standalone compression quality 100. Future Ultra can use a new true 4K source; Performance/mobile uses a 1024px derivative.
- **Unity setup:** Assigned by `PrototypeSceneBuilder.cs` to the quarter-mile asphalt when explicitly rebuilding generated scenes.
- **Review:** 2026-08-21, Carvino PC Texture/Material Artist.

## carvino-starter-pavilion-01 review

- **Source / author:** Original Carvino procedural geometry created in `TracksideStarterPavilionBuilder.cs`.
- **Rights / branding:** No external asset, badge, logo, copied signage, text, source mesh, or texture used; suitable for commercial distribution.
- **Geometry:** One small trackside pavilion at the starter end with simple room, roof, windows, stairs, rails, and lights. 24 low-poly Unity primitive renderers; colliders are removed because all parts are visual-only.
- **Textures / tiers:** None. PC uses flat steel, glass-color, concrete, and emissive materials. The mobile tier can omit it or merge it into a simplified static strip dressing pass.
- **Unity setup:** Baked into the quarter-mile scene via **Carvino → Art → Add Original Starter Pavilion**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino PC Environment/Level Artist.

## carvino-pc-garage-bay-dressing-01 review

- **Source / author:** Original Carvino procedural geometry, created in `GarageBayDressingBuilder.cs`.
- **Rights / branding:** No external asset, logo, manufacturer mark, text, source mesh, or texture used; suitable for commercial distribution.
- **Geometry:** One workbench, one tool locker, three overhead fixtures, and floor bay markers. Approximately 30 low-poly Unity primitive renderers; colliders are removed because all props are visual-only.
- **Textures / tiers:** None. PC uses flat metal and emissive materials; the mobile tier can omit the dressing or batch it into a simplified static garage pass.
- **Unity setup:** Baked into the Garage scene via **Carvino → Art → Add PC Garage Bay Dressing**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino PC Art/Content worker.

## carvino-trackside-scoreboards-01 review

- **Source / author:** Original Carvino procedural geometry, created in `TracksideScoreboardBuilder.cs`.
- **Rights / branding:** No external asset, logo, manufacturer mark, text, or source texture used; safe for commercial distribution.
- **Geometry:** Two scoreboards; 17 Unity primitive renderers per board, constructed once into the quarter-mile scene. Colliders are removed because the props are visual-only.
- **Textures / tiers:** None. Uses small flat-color and emission materials, so no texture memory or compressed texture footprint on PC or mobile.
- **Unity setup:** Baked as ordinary static scene primitives through **Carvino → Art → Add Trackside Scoreboards**. The editor helper is excluded from player builds.
- **Review:** 2026-08-21, Carvino Art/Content worker.

## carvino-hatch-93 review

- **Source / author:** Original Carvino geometry created in Blender at `Assets/Carvino/Art/Models/CarvinoHatch_93.blend`, with the Unity-ready interchange copy at `Assets/Carvino/Art/Models/CarvinoHatch_93.fbx`.
- **Rights / branding:** Commercial-use original project asset. It uses broad early-1990s compact three-door proportions as design-era inspiration only. No Honda mesh, scan, blueprint, badge, wordmark, logo, photo texture, or proprietary source asset was used or included.
- **Geometry:** 90 named mesh objects, 12,470 source vertices and 24,604 source triangles. The body shell has true wheel-arch openings; wheels, tires, rims, brake hardware, glass, lights, trim, interior, and body details remain separate named objects. Current LOD0 only; a reduced PC LOD1 and mobile LOD2 remain future work.
- **Textures / materials:** No external bitmap textures. Ultra+ v2 uses original procedural Blender node materials: layered metallic-teal paint with clearcoat and microflake response, tinted transmissive glass, detailed lamp-lens surfaces, street-rubber grain, interior/trim grain, machined alloy, brake rotors, and wheel hardware. FBX-compatible base PBR values are retained for Unity import. Studio review render: `Assets/Carvino/Art/Previews/CarvinoHatch_93_UltraPlus_MaterialPreview.png` (1600 × 900 PNG).
- **Collision / import:** No mesh colliders are generated by the FBX importer. Unity import retains metric units, one-meter scale, Y-up / forward-axis conversion, readable vertex colors, and separate hierarchy objects. Runtime vehicle collision remains under the game's existing simplified physics authority.
- **Review:** 2026-08-21, Carvino PC Ultra+ Vehicle Materials worker. Material-only geometry lock preserved 90 mesh objects, 12,470 vertices, 11,707 polygons, and 24,604 evaluated triangles; Blender source and exported FBX retain the original names and upright axis convention.

## carvino-pc-ultra-track-atmosphere-01 review

- **Source / author:** Original Carvino procedural scene dressing and lighting created in `UltraTrackAtmosphereBuilder.cs`.
- **Rights / branding:** No external mesh, photo, logo, signage, mark, texture, copied venue layout, or protected game material is used. The pass uses only Unity primitive geometry and project-authored material values.
- **Geometry:** Ten visual-only low-poly light towers (five per side), a pair of thin prepared-lane overlays, launch-box overlays, and three emissive guide strips. Colliders are removed from all new primitive props, preserving existing driving and timing authority.
- **Textures / tiers:** No external bitmap texture is introduced. PC Ultra+ uses project-native materials plus soft-shadow spot lights and one cool directional key. Performance/mobile should omit this complete root object or retain only the non-light lane overlays.
- **Unity setup:** Baked into `QuarterMilePrototype.unity` through **Carvino → Art → Add PC Ultra Track Atmosphere**. It does not rebuild the scene, touch vehicle assets, or change runtime scripts.
- **Review:** 2026-08-21, Carvino PC Ultra Environment worker.

## Required fields for each new entry

- Exact source URL or original-project location
- Author/rights holder and license
- Whether badges, branding or other protected marks were removed
- Source mesh triangle count and LOD counts
- Texture resolutions and compressed size by quality tier
- Collision setup and Unity import settings
- Review date and reviewer

No entry means no import into the release build.
