import bpy
import math
import os
import re
from mathutils import Matrix, Vector

MASTER = r"P:\chatgpt projects\Carvino Drag Sim\Assets\Carvino\Art\Models\Cinematic\Hatch93\CarvinoHatch_93_CinematicLOD0.blend"
BASE_SOURCE = r"P:\chatgpt projects\Carvino Drag Sim\Assets\Carvino\Art\Models\CarvinoHatch_93.blend"
OUT_DIR = r"C:\Users\Xxroa\Documents\Carvino drag sim\hatch-playable-stage"
BLENDER_OUT = os.path.join(OUT_DIR, "CarvinoHatch_93_Playable_LODs.blend")
COMBINED_OUT = os.path.join(OUT_DIR, "CarvinoHatch_93_Playable.fbx")
PREVIEW_OUT = os.path.join(OUT_DIR, "CarvinoHatch_93_PlayableLOD0_Preview.png")


def clean_name(value):
    return re.sub(r"[^A-Za-z0-9_]+", "_", value).strip("_")[:44] or "Material"


def triangles(obj):
    if obj.type != "MESH":
        return 0
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def subtree_triangles(root):
    return sum(triangles(obj) for obj in root.children_recursive if obj.type == "MESH")


def delete_tree(obj):
    for child in list(obj.children):
        delete_tree(child)
    bpy.data.objects.remove(obj, do_unlink=True)


def parent(obj, owner):
    obj.parent = owner
    return obj


def smooth(obj):
    if obj.type == "MESH":
        for polygon in obj.data.polygons:
            polygon.use_smooth = True
    return obj


def apply_modifier(obj, modifier):
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)


def bevel(obj, width, segments=2):
    modifier = obj.modifiers.new("Playable edge radius", "BEVEL")
    modifier.width = width
    modifier.segments = segments
    modifier.limit_method = "ANGLE"
    apply_modifier(obj, modifier)
    return smooth(obj)


def mat(name, color, metallic=0.0, roughness=0.4):
    found = bpy.data.materials.get(name)
    if found:
        return found
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if "Coat Weight" in bsdf.inputs:
        bsdf.inputs["Coat Weight"].default_value = 0.34 if metallic > 0.15 else 0.08
    return material


def cube(name, location, dimensions, material, owner, bevel_width=0.0, bevel_segments=2, rotation=(0, 0, 0)):
    bpy.ops.mesh.primitive_cube_add(location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    parent(obj, owner)
    if bevel_width:
        bevel(obj, bevel_width, bevel_segments)
    return obj


def cylinder(name, location, radius, depth, material, owner, vertices, rotation=(0, math.pi / 2, 0), bevel_width=0.0):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    parent(obj, owner)
    if bevel_width:
        bevel(obj, bevel_width, min(2, max(1, vertices // 32)))
    else:
        smooth(obj)
    return obj


def torus(name, location, major_radius, minor_radius, material, owner, major_segments, minor_segments):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major_radius,
        minor_radius=minor_radius,
        major_segments=major_segments,
        minor_segments=minor_segments,
        location=location,
        rotation=(0, math.pi / 2, 0),
    )
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    return parent(smooth(obj), owner)


def wheel_materials():
    return {
        "rubber": mat("CARVINO_Hatch_Mechanical_4K__TireRubber", (0.008, 0.010, 0.012, 1), 0.0, 0.78),
        "sidewall": mat("CARVINO_Hatch_Mechanical_4K__Sidewall", (0.014, 0.016, 0.018, 1), 0.0, 0.62),
        "alloy": mat("CARVINO_Hatch_Mechanical_4K__MachinedAlloy", (0.28, 0.31, 0.34, 1), 0.92, 0.18),
        "dark": mat("CARVINO_Hatch_Mechanical_4K__DarkAlloy", (0.026, 0.031, 0.037, 1), 0.78, 0.28),
        "rotor": mat("CARVINO_Hatch_Mechanical_4K__BrakeRotor", (0.23, 0.24, 0.25, 1), 0.87, 0.30),
        "caliper": mat("CARVINO_Hatch_Mechanical_4K__Caliper", (0.58, 0.022, 0.015, 1), 0.70, 0.22),
    }


def add_playable_wheel(name, x, z, owner, lod):
    quality = {
        0: dict(tire=(112, 24), side=(72, 8), lip=(96, 12), inner=(64, 8), barrel=96, rotor=(80, 10), hub=64, spokes=10, tread=(48, 3)),
        1: dict(tire=(64, 16), side=(40, 6), lip=(48, 8), inner=(36, 6), barrel=48, rotor=(40, 6), hub=40, spokes=8, tread=(24, 2)),
        2: dict(tire=(40, 12), side=(28, 6), lip=(32, 6), inner=(24, 5), barrel=32, rotor=(28, 6), hub=28, spokes=6, tread=(16, 1)),
    }[lod]
    materials = wheel_materials()
    wheel = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(wheel)
    wheel.location = (x, 0.42, z)
    wheel["wheel_role"] = name
    wheel["lod"] = lod
    parent(wheel, owner)
    outside = 1 if x > 0 else -1

    torus(name + "_Tire", (0, 0, 0), 0.258, 0.074, materials["rubber"], wheel, *quality["tire"])
    torus(name + "_SidewallOuter", (outside * 0.078, 0, 0), 0.258, 0.0055, materials["sidewall"], wheel, *quality["side"])
    if lod < 2:
        torus(name + "_SidewallInner", (-outside * 0.078, 0, 0), 0.258, 0.0045, materials["sidewall"], wheel, *quality["side"])
    torus(name + "_RimLip", (outside * 0.092, 0, 0), 0.205, 0.015, materials["alloy"], wheel, *quality["lip"])
    torus(name + "_RimInner", (-outside * 0.045, 0, 0), 0.180, 0.009, materials["dark"], wheel, *quality["inner"])
    cylinder(name + "_Barrel", (0, 0, 0), 0.194, 0.155, materials["dark"], wheel, quality["barrel"], bevel_width=0.003)
    face_x = outside * 0.103
    cylinder(name + "_Hub", (face_x, 0, 0), 0.052, 0.024, materials["alloy"], wheel, quality["hub"], bevel_width=0.002)

    spoke_count = quality["spokes"]
    for index in range(spoke_count):
        angle = index * math.tau / spoke_count
        spoke = cube(
            name + f"_Spoke_{index + 1:02d}",
            (face_x, math.sin(angle) * 0.105, math.cos(angle) * 0.105),
            (0.025 if lod < 2 else 0.032, 0.032, 0.222),
            materials["alloy"],
            wheel,
            0.006 if lod == 0 else 0.002 if lod == 1 else 0.0,
            2 if lod == 0 else 1,
        )
        spoke.rotation_euler.x = -angle

    rotor_x = outside * 0.074
    torus(name + "_BrakeRotor", (rotor_x, 0, 0), 0.140 if z > 0 else 0.123, 0.025, materials["rotor"], wheel, *quality["rotor"])
    cylinder(name + "_RotorHat", (rotor_x, 0, 0), 0.069, 0.018, materials["dark"], wheel, quality["hub"], bevel_width=0.002)
    cube(name + "_Caliper", (outside * 0.112, -0.120, 0.012), (0.050, 0.078, 0.135 if z > 0 else 0.108), materials["caliper"], wheel, 0.012 if lod == 0 else 0.004, 2)

    if lod < 2:
        lug_count = 4
        for index in range(lug_count):
            angle = index * math.tau / lug_count + math.pi / 4
            cylinder(name + f"_Lug_{index + 1}", (outside * 0.125, math.sin(angle) * 0.033, math.cos(angle) * 0.033), 0.007, 0.015, materials["dark"], wheel, 16 if lod == 0 else 12)

    tread_count, bands = quality["tread"]
    for index in range(tread_count):
        angle = index * math.tau / tread_count
        for band in range(bands):
            side_offset = 0 if bands == 1 else -0.042 + band * (0.084 / (bands - 1))
            block = cube(
                name + f"_Tread_{index:02d}_{band}",
                (side_offset, math.sin(angle) * 0.329, math.cos(angle) * 0.329),
                (0.026 if lod == 0 else 0.036, 0.060, 0.014),
                materials["rubber"],
                wheel,
                0.002 if lod == 0 else 0.0,
                1,
            )
            block.rotation_euler.x = -angle + (0.14 if (index + band) % 2 else -0.14)
    return wheel


def restore_closed_body(root):
    old_body = bpy.data.objects.get("Body_Shell")
    paint = old_body.data.materials[0] if old_body and old_body.data.materials else None
    if old_body:
        delete_tree(old_body)
    with bpy.data.libraries.load(BASE_SOURCE, link=False) as (data_from, data_to):
        if "Body_Shell" not in data_from.objects:
            raise RuntimeError("Closed Body_Shell missing from base source")
        data_to.objects = ["Body_Shell"]
    body = data_to.objects[0]
    bpy.context.collection.objects.link(body)
    body.name = "Body_Shell"
    body.parent = root
    if paint:
        body.data.materials.clear()
        body.data.materials.append(paint)
    modifier = body.modifiers.new("Playable surface continuity", "SUBSURF")
    modifier.subdivision_type = "CATMULL_CLARK"
    modifier.levels = 1
    modifier.render_levels = 1
    apply_modifier(body, modifier)
    body["configuration"] = "Closed factory-style hood surface"


def close_hood(root):
    hinge = bpy.data.objects.get("Hood_Open_Hinge")
    if hinge:
        delete_tree(hinge)


def ancestry_names(obj):
    values = [obj.name]
    current = obj.parent
    while current:
        values.append(current.name)
        current = current.parent
    return " ".join(values).lower()


def group_for(obj):
    value = ancestry_names(obj)
    if any(token in value for token in ("glass", "lamp", "lens", "reflector", "bulb", "amber", "gasket")):
        return "GlassLights"
    if any(token in value for token in ("cockpit", "interior", "seat", "dash", "steering", "gauge", "shifter", "console", "stitch", "vent")):
        return "Interior"
    if any(token in value for token in ("wheel", "tire", "rim", "brake", "rotor", "caliper", "lug", "exhaust", "hub", "spoke", "mechanical")):
        return "Mechanical"
    return "Body"


def normalize_material_groups(root):
    cache = {}
    for obj in root.children_recursive:
        if obj.type != "MESH":
            continue
        group = group_for(obj)
        for index, material in enumerate(list(obj.data.materials)):
            if material is None:
                continue
            if material.name.startswith("CARVINO_Hatch_"):
                continue
            key = (material.name, group)
            if key not in cache:
                copy = material.copy()
                copy.name = f"CARVINO_Hatch_{group}_4K__{clean_name(material.name)}"
                cache[key] = copy
            obj.data.materials[index] = cache[key]


def decimate(obj, ratio):
    if obj is None or obj.type != "MESH" or triangles(obj) < 500:
        return
    modifier = obj.modifiers.new("Playable LOD reduction", "DECIMATE")
    modifier.decimate_type = "COLLAPSE"
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True
    apply_modifier(obj, modifier)


def strip_for_lod(root, lod):
    for wheel_name in ("CIN_Wheel_FL", "CIN_Wheel_FR", "CIN_Wheel_RL", "CIN_Wheel_RR"):
        wheel = bpy.data.objects.get(wheel_name)
        if wheel:
            delete_tree(wheel)
    bay = bpy.data.objects.get("Cinematic_EngineBay")
    if bay:
        delete_tree(bay)
    restore_closed_body(root)
    close_hood(root)

    if lod >= 1:
        remove_tokens = ("Reflector", "Bulb", "Stitch_", "Gauge_Needle")
        for obj in list(root.children_recursive):
            if any(token in obj.name for token in remove_tokens):
                delete_tree(obj)
        # Preserve the authored hero shell exactly. The wheel/tread, cockpit, and
        # lamp-internal reductions already meet the target budget, while shell
        # decimation can move boundary vertices and change camera framing.

    if lod >= 2:
        cockpit = bpy.data.objects.get("Cinematic_Cockpit")
        if cockpit:
            delete_tree(cockpit)
        remove_tokens = (
            "Headlamp_", "Door_Handle", "Door_Gap", "Molding", "Rocker_Seam",
            "Window_Frame", "Arch_Trim", "Hood_Shut", "Hood_Center_Crease",
            "Hatch_Shut", "Front_Seat", "Front_Headrest", "Dashboard",
            "Interior_Floor", "Steering_Wheel", "License_Recess",
        )
        for obj in list(root.children_recursive):
            if any(token in obj.name for token in remove_tokens):
                delete_tree(obj)
        # LOD2 keeps the same outer silhouette; small trim and hidden detail are
        # removed instead of collapsing the body surface.
        cabin_material = mat(
            "CARVINO_Hatch_Interior_4K__DistanceCabin",
            (0.008, 0.011, 0.015, 1),
            0.0,
            0.78,
        )
        cube(
            "LOD2_CabinSilhouette",
            (0.0, 0.63, -0.12),
            (1.28, 0.40, 1.42),
            cabin_material,
            root,
            0.07,
            2,
        )

    for name, x, z in (
        ("Wheel_FL", -0.86, 1.22),
        ("Wheel_FR", 0.86, 1.22),
        ("Wheel_RL", -0.86, -1.25),
        ("Wheel_RR", 0.86, -1.25),
    ):
        add_playable_wheel(name, x, z, root, lod)

    normalize_material_groups(root)
    root["asset_origin"] = "Original Carvino project geometry"
    root["license"] = "Original commercial-use asset; no external mesh, logo, or badge"
    root["configuration"] = "Closed hood playable PC vehicle"
    root["lod_level"] = lod
    root["texture_layout"] = "Body, Interior, Mechanical, GlassLights; four 4096-ready groups"


def select_tree(root):
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for obj in root.children_recursive:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root


def bounds(root):
    corners = []
    for obj in root.children_recursive:
        if obj.type == "MESH":
            corners.extend(obj.matrix_world @ Vector(corner) for corner in obj.bound_box)
    lo = Vector((min(v.x for v in corners), min(v.y for v in corners), min(v.z for v in corners)))
    hi = Vector((max(v.x for v in corners), max(v.y for v in corners), max(v.z for v in corners)))
    return hi - lo


def export_level(lod):
    bpy.ops.wm.open_mainfile(filepath=MASTER)
    root = bpy.data.objects.get("CarvinoHatch_93_CinematicLOD0")
    if not root:
        raise RuntimeError("Cinematic hatch root missing")
    root.name = f"CarvinoHatch_93_PlayableLOD{lod}"
    strip_for_lod(root, lod)
    total = subtree_triangles(root)
    size = bounds(root)
    root["triangle_count"] = total
    root["dimensions_m"] = [round(v, 4) for v in size]
    print(f"CARVINO_PLAYABLE_LOD{lod} triangles={total} dims=({size.x:.4f},{size.y:.4f},{size.z:.4f})")
    targets = ((120000, 180000), (45000, 80000), (20000, 40000))[lod]
    if not targets[0] <= total <= targets[1]:
        raise RuntimeError(f"LOD{lod} triangles {total} outside target {targets}")
    blend = os.path.join(OUT_DIR, f"CarvinoHatch_93_PlayableLOD{lod}.blend")
    fbx = os.path.join(OUT_DIR, f"CarvinoHatch_93_PlayableLOD{lod}.fbx")
    select_tree(root)
    bpy.ops.wm.save_as_mainfile(filepath=blend, compress=True)
    bpy.ops.export_scene.fbx(
        filepath=fbx,
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        use_mesh_modifiers=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
        embed_textures=False,
    )
    return blend, fbx, total


def append_fbx(path, lod, combined_root):
    before = set(bpy.data.objects)
    bpy.ops.import_scene.fbx(filepath=path, use_custom_normals=True, use_anim=False)
    imported = list(set(bpy.data.objects) - before)
    roots = [obj for obj in imported if obj.parent is None]
    if len(roots) != 1:
        raise RuntimeError(f"LOD{lod} import expected one root, found {[o.name for o in roots]}")
    lod_root = roots[0]
    lod_root.name = f"LOD{lod}"
    lod_root.parent = combined_root
    for obj in lod_root.children_recursive:
        if lod > 0 and obj.name.startswith("Wheel_"):
            obj.name = f"LOD{lod}_{obj.name}"
    return lod_root


def look_at(obj, target):
    forward = (target - obj.location).normalized()
    world_up = Vector((0.0, 1.0, 0.0))
    right = forward.cross(world_up).normalized()
    corrected_up = right.cross(forward).normalized()
    rotation = Matrix((right, corrected_up, -forward)).transposed().to_4x4()
    obj.matrix_world = Matrix.Translation(obj.location) @ rotation


def make_preview(lod0_blend):
    bpy.ops.wm.open_mainfile(filepath=lod0_blend)
    root = bpy.data.objects.get("CarvinoHatch_93_PlayableLOD0")
    floor_mat = mat("PREVIEW_Floor", (0.018, 0.023, 0.030, 1), 0.1, 0.32)
    bpy.ops.mesh.primitive_plane_add(size=18, location=(0, 0.085, 0), rotation=(math.pi / 2, 0, 0))
    bpy.context.object.data.materials.append(floor_mat)
    for index, (location, energy, size, color) in enumerate((
        ((4.4, 5.0, 5.3), 1350, 4.0, (0.66, 0.78, 1.0)),
        ((-3.6, 3.1, 2.0), 1050, 3.0, (1.0, 0.30, 0.16)),
        ((0.2, 5.0, -4.4), 1200, 3.2, (0.32, 0.48, 1.0)),
    )):
        data = bpy.data.lights.new(f"PREVIEW_Light_{index}", "AREA")
        data.energy = energy
        data.shape = "DISK"
        data.size = size
        data.color = color
        light = bpy.data.objects.new(data.name, data)
        bpy.context.collection.objects.link(light)
        light.location = location
        look_at(light, Vector((0, 0.65, 0.25)))
    cam_data = bpy.data.cameras.new("PREVIEW_Camera")
    cam = bpy.data.objects.new("PREVIEW_Camera", cam_data)
    bpy.context.collection.objects.link(cam)
    cam.location = (5.4, 2.6, 6.2)
    cam_data.lens = 58
    look_at(cam, Vector((0, 0.67, 0.25)))
    scene = bpy.context.scene
    scene.camera = cam
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1920
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = PREVIEW_OUT
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.world.use_nodes = True
    bg = scene.world.node_tree.nodes.get("Background")
    bg.inputs["Color"].default_value = (0.006, 0.010, 0.018, 1)
    bg.inputs["Strength"].default_value = 0.32
    bpy.ops.render.render(write_still=True)


def combine(levels):
    bpy.ops.wm.read_factory_settings(use_empty=True)
    root = bpy.data.objects.new("CarvinoHatch_93", None)
    bpy.context.collection.objects.link(root)
    root["asset_origin"] = "Original Carvino project geometry"
    root["license"] = "Original commercial-use asset"
    root["configuration"] = "Closed hood playable LOD set"
    root["lod_thresholds"] = [0.58, 0.24, 0.04]
    for lod, (_, fbx, _) in enumerate(levels):
        append_fbx(fbx, lod, root)
    select_tree(root)
    bpy.ops.wm.save_as_mainfile(filepath=BLENDER_OUT, compress=True)
    bpy.ops.export_scene.fbx(
        filepath=COMBINED_OUT,
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        use_mesh_modifiers=True,
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_UNITS",
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="AUTO",
        embed_textures=False,
    )
    print("CARVINO_PLAYABLE_COMBINED", COMBINED_OUT)


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    levels = [export_level(lod) for lod in range(3)]
    make_preview(levels[0][0])
    combine(levels)
    print("CARVINO_PLAYABLE_BLEND", BLENDER_OUT)
    print("CARVINO_PLAYABLE_PREVIEW", PREVIEW_OUT)


if __name__ == "__main__":
    main()
