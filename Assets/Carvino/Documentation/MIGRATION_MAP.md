# Safe source migration map

The current v0.01 source remains in `Assets/Carvino/Runtime` so the playable
build is not disrupted. Move a file only as part of a tested feature change.

| Current file | Future owner/location |
| --- | --- |
| `CarvinoCatalog.cs` | `Runtime/Core/Content/CarvinoCatalog.cs` |
| `GarageSession.cs` | `Runtime/Core/Profile/GarageSession.cs` |
| `GarageController.cs` | `Runtime/UI/Garage/GarageController.cs` |
| `GarageInspectionController.cs` | `Runtime/UI/Garage/GarageInspectionController.cs` |
| `MainMenuController.cs` | `Runtime/UI/Menu/MainMenuController.cs` |
| `PrototypeSceneBuilder.cs` | `Editor/Integration/PrototypeSceneBuilder.cs` |

New features must use the ownership folders in `WORKER_MANIFEST.json` from the
start. This makes the project progressively cleaner without a risky all-at-once
rewrite.
