# PC content roadmap: toward a real 20 GB edition

The 20 GB target applies to the future Windows/Steam edition. It must come from
real playable content and high-quality source assets, not duplicated files or
empty padding. Android receives a separate, much smaller content tier.

## Content budget (installed size, target)

| Area | PC target | What earns the space |
| --- | ---: | --- |
| Vehicles and upgrade parts | 6 GB | 10–20 original/licensed vehicles, engine bays, interiors, damage parts, four LODs and 2K/4K PBR materials |
| Tracks and garage worlds | 5 GB | drag strips, street districts, night/day variants, props, crowds and collision/LOD data |
| Audio | 2 GB | layered engine families, induction/exhaust variations, turbo, tires, UI, ambience and music |
| Customization | 2 GB | wheels, paint, decals, body/engine parts and material variations |
| Effects/cinematics/UI | 1 GB | tire smoke, weather, sparks, intro/outro and polished menus |
| Shared systems/content data | 1 GB | races, AI profiles, part data, save migrations and localization reserve |
| Headroom/DLC-ready content | 3 GB | future car and track packs, only when each is shippable content |

## Quality tiers

| Tier | Platform | Typical texture cap | Vehicle LODs |
| --- | --- | --- | --- |
| PC Ultra | Steam desktop | 4K hero car, 2K supporting assets | 4 |
| PC High | Steam desktop | 2K | 3 |
| PC Performance | lower-spec PC | 1K–2K | 3 |
| Mobile | Android | 512–1K | 2 |

The same original source model may feed all tiers. The game must not ship PC
Ultra textures to Android by default.

## First PC-content milestones

1. Establish an asset registry with author, license, source, LOD and texture
   budget for every imported item.
2. Finish two presentable alpha vehicles with damage-ready parts and PC/mobile
   material tiers.
3. Turn the current drag strip and garage into reusable environment packs with
   original props, lighting and optimized materials.
4. Add a content-pack loader only after the offline garage/race loop is stable.
5. Use Git LFS or a dedicated art store for large binary source assets; ordinary
   Git remains for code, scenes, data and documentation.

## Rule for external content

Every asset requires a source record showing commercial-use permission before
it can enter `Assets/Carvino/Art`. Reference images and proprietary game files
never become shipped assets.
