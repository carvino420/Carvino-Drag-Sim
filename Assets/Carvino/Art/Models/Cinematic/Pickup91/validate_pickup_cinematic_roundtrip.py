import bpy
import os
from mathutils import Vector


FBX_PATH = os.path.join(os.path.dirname(os.path.abspath(__file__)), "CarvinoPickup_91_CinematicLOD0.fbx")
EXPECTED_WHEELS = {"CIN_Wheel_FL", "CIN_Wheel_FR", "CIN_Wheel_RL", "CIN_Wheel_RR"}
EXPECTED_PREFIXES = {
    "CIN_Pickup_Body_4K__",
    "CIN_Pickup_Interior_4K__",
    "CIN_Pickup_Mechanical_4K__",
    "CIN_Pickup_GlassLights_4K__",
}


def fail(message):
    raise RuntimeError("CARVINO_PICKUP_ROUNDTRIP_FAILED: " + message)


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=FBX_PATH, use_anim=False)
objects = list(bpy.context.scene.objects)
meshes = [obj for obj in objects if obj.type == "MESH"]
if not meshes:
    fail("FBX contains no mesh objects")

triangles = 0
vertices = 0
corners = []
missing_uv = []
materials = set()
for obj in meshes:
    obj.data.calc_loop_triangles()
    triangles += len(obj.data.loop_triangles)
    vertices += len(obj.data.vertices)
    if len(obj.data.polygons) and len(obj.data.uv_layers) == 0:
        missing_uv.append(obj.name)
    materials.update(slot.material.name for slot in obj.material_slots if slot.material)
    corners.extend(obj.matrix_world @ Vector(point) for point in obj.bound_box)

if triangles < 250000 or triangles > 320000:
    fail(f"triangle count {triangles} is outside 250000-320000")
wheel_names = {obj.name for obj in objects if obj.name in EXPECTED_WHEELS}
if wheel_names != EXPECTED_WHEELS:
    fail(f"wheel roots mismatch: {sorted(wheel_names)}")
if missing_uv:
    fail(f"meshes missing UV layers: {missing_uv[:10]}")
missing_prefixes = [prefix for prefix in EXPECTED_PREFIXES if not any(name.startswith(prefix) for name in materials)]
if missing_prefixes:
    fail(f"missing 4K material groups: {missing_prefixes}")

minimum = Vector((min(v.x for v in corners), min(v.y for v in corners), min(v.z for v in corners)))
maximum = Vector((max(v.x for v in corners), max(v.y for v in corners), max(v.z for v in corners)))
dimensions = maximum - minimum
print(
    "CARVINO_PICKUP_ROUNDTRIP_OK",
    f"meshes={len(meshes)}",
    f"vertices={vertices}",
    f"triangles={triangles}",
    f"dimensions=({dimensions.x:.4f},{dimensions.y:.4f},{dimensions.z:.4f})",
    f"wheel_roots={','.join(sorted(wheel_names))}",
    f"materials={len(materials)}",
    "uv_coverage=100%",
)
