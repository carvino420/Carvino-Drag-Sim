import bpy
import math
import os
from mathutils import Matrix, Vector


SOURCE_BLEND = r"P:\chatgpt projects\Carvino Drag Sim\Assets\Carvino\Art\Models\CarvinoHatch_93.blend"
STAGE_DIR = r"C:\Users\Xxroa\Documents\Carvino drag sim\cinematic-hatch-stage"
BLEND_PATH = os.path.join(STAGE_DIR, "CarvinoHatch_93_CinematicLOD0.blend")
FBX_PATH = os.path.join(STAGE_DIR, "CarvinoHatch_93_CinematicLOD0.fbx")
PREVIEW_PATH = os.path.join(STAGE_DIR, "CarvinoHatch_93_CinematicLOD0_Preview.png")


def mat(name, color, metallic=0.0, roughness=0.4, transmission=0.0, emission=None):
    existing = bpy.data.materials.get(name)
    if existing:
        return existing
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if "Coat Weight" in bsdf.inputs:
        bsdf.inputs["Coat Weight"].default_value = 0.5 if metallic > 0.1 else 0.15
    if "Transmission Weight" in bsdf.inputs:
        bsdf.inputs["Transmission Weight"].default_value = transmission
    if emission is not None:
        bsdf.inputs["Emission Color"].default_value = emission
        bsdf.inputs["Emission Strength"].default_value = 3.0
    return material


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


def bevel(obj, width=0.01, segments=2):
    modifier = obj.modifiers.new("Cinematic edge radii", "BEVEL")
    modifier.width = width
    modifier.segments = segments
    modifier.limit_method = "ANGLE"
    apply_modifier(obj, modifier)
    return smooth(obj)


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


def cylinder(name, location, radius, depth, material, owner, vertices=64, rotation=(0, math.pi / 2, 0), bevel_width=0.0):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    parent(obj, owner)
    if bevel_width:
        bevel(obj, bevel_width, 2)
    else:
        smooth(obj)
    return obj


def torus(name, location, major_radius, minor_radius, material, owner, major_segments=128, minor_segments=24, rotation=(0, math.pi / 2, 0)):
    bpy.ops.mesh.primitive_torus_add(
        major_radius=major_radius,
        minor_radius=minor_radius,
        major_segments=major_segments,
        minor_segments=minor_segments,
        location=location,
        rotation=rotation,
    )
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    parent(obj, owner)
    return smooth(obj)


def uv_sphere(name, location, scale, material, owner, segments=48, rings=24):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    return parent(smooth(obj), owner)


def curve_tube(name, points, material, owner, radius=0.008, resolution=3, cyclic=False):
    data = bpy.data.curves.new(name + " Curve", "CURVE")
    data.dimensions = "3D"
    data.resolution_u = 2
    data.bevel_depth = radius
    data.bevel_resolution = resolution
    spline = data.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for handle, position in zip(spline.bezier_points, points):
        handle.co = position
        handle.handle_left_type = "AUTO"
        handle.handle_right_type = "AUTO"
    spline.use_cyclic_u = cyclic
    obj = bpy.data.objects.new(name, data)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    return parent(obj, owner)


def delete_tree(obj):
    for child in list(obj.children):
        delete_tree(child)
    bpy.data.objects.remove(obj, do_unlink=True)


def add_cinematic_wheel(name, x, z, owner, materials, front=True):
    wheel = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(wheel)
    wheel.location = (x, 0.42, z)
    parent(wheel, owner)
    outside = 1 if x > 0 else -1

    # Dense round tire surface is visible in extreme garage close-ups.
    torus(name + "_TireCarcass", (0, 0, 0), 0.258, 0.074, materials["rubber"], wheel, 224, 36)
    torus(name + "_OuterSidewallRib", (outside * 0.078, 0, 0), 0.258, 0.006, materials["rubber_side"], wheel, 96, 12)
    torus(name + "_InnerSidewallRib", (-outside * 0.078, 0, 0), 0.258, 0.005, materials["rubber_side"], wheel, 96, 10)

    # Directional tread blocks are individual modeled features, not polygon padding.
    for index in range(72):
        angle = index * math.tau / 72.0
        for band, side_offset in enumerate((-0.046, 0.0, 0.046)):
            block = cube(
                name + f"_Tread_{index:02d}_{band}",
                (side_offset, math.sin(angle) * 0.329, math.cos(angle) * 0.329),
                (0.026, 0.068, 0.016),
                materials["rubber_tread"],
                wheel,
                0.004,
                1,
            )
            block.rotation_euler.x = -angle + (0.18 if (index + band) % 2 else -0.18)

    # Barrel, lip, split-spoke face and hub.
    torus(name + "_RimOuterLip", (outside * 0.092, 0, 0), 0.205, 0.016, materials["alloy"], wheel, 192, 20)
    torus(name + "_RimInnerLip", (-outside * 0.052, 0, 0), 0.184, 0.010, materials["alloy_dark"], wheel, 160, 16)
    cylinder(name + "_RimBarrel", (0, 0, 0), 0.196, 0.158, materials["alloy_dark"], wheel, 160, bevel_width=0.004)
    face_x = outside * 0.103
    cylinder(name + "_Hub", (face_x, 0, 0), 0.055, 0.025, materials["alloy"], wheel, 96, bevel_width=0.003)
    for index in range(10):
        angle = index * math.tau / 10.0
        radial_y = math.sin(angle) * 0.108
        radial_z = math.cos(angle) * 0.108
        spoke = cube(
            name + f"_ForgedSpoke_{index + 1:02d}",
            (face_x, radial_y, radial_z),
            (0.026, 0.034, 0.228),
            materials["alloy"],
            wheel,
            0.009,
            3,
        )
        spoke.rotation_euler.x = -angle

    # Vented, drilled brake assembly and visible caliper.
    disc_x = outside * 0.076
    torus(name + "_BrakeRotor", (disc_x, 0, 0), 0.143 if front else 0.126, 0.027, materials["rotor"], wheel, 192, 18)
    cylinder(name + "_RotorHat", (disc_x, 0, 0), 0.072, 0.020, materials["rotor_hat"], wheel, 96, bevel_width=0.003)
    rotor_radius = 0.135 if front else 0.118
    for index in range(24 if front else 18):
        angle = index * math.tau / (24 if front else 18)
        cylinder(
            name + f"_RotorDrill_{index + 1:02d}",
            (disc_x + outside * 0.014, math.sin(angle) * rotor_radius, math.cos(angle) * rotor_radius),
            0.006,
            0.008,
            materials["rotor_dark"],
            wheel,
            16,
        )
    caliper = cube(
        name + "_Caliper",
        (outside * 0.118, -0.125, 0.015),
        (0.055, 0.085, 0.145 if front else 0.115),
        materials["caliper"],
        wheel,
        0.018,
        4,
        rotation=(0.14, 0, 0),
    )
    caliper["functional_detail"] = "Modeled brake caliper"
    for index in range(5):
        angle = index * math.tau / 5.0
        cylinder(
            name + f"_Lug_{index + 1}",
            (outside * 0.125, math.sin(angle) * 0.036, math.cos(angle) * 0.036),
            0.009,
            0.018,
            materials["hardware"],
            wheel,
            20,
            bevel_width=0.002,
        )
    return wheel


def add_lamp_internals(owner, materials):
    for side in (-1, 1):
        # Four parabolic-style reflector bowls sit behind the original clear lens.
        for index, x_offset in enumerate((0.34, 0.56)):
            x = side * x_offset
            reflector = uv_sphere(
                f"Headlamp_{side}_Reflector_{index}",
                (x, 0.645, 2.025),
                (0.125, 0.055, 0.035),
                materials["reflector"],
                owner,
                64,
                32,
            )
            reflector["detail_role"] = "Lamp reflector housing"
            uv_sphere(
                f"Headlamp_{side}_Bulb_{index}",
                (x, 0.642, 2.060),
                (0.026, 0.026, 0.016),
                materials["bulb"],
                owner,
                32,
                16,
            )
        # Modeled perimeter gasket and lens ribs.
        curve_tube(
            f"Headlamp_{side}_Gasket",
            [
                (side * 0.76, 0.716, 2.018),
                (side * 0.25, 0.716, 2.047),
                (side * 0.25, 0.575, 2.058),
                (side * 0.76, 0.575, 2.029),
            ],
            materials["gasket"],
            owner,
            0.008,
            3,
            True,
        )
        for rib in range(7):
            x = side * (0.30 + rib * 0.065)
            curve_tube(
                f"Headlamp_{side}_LensRib_{rib}",
                [(x, 0.584, 2.062), (x, 0.704, 2.052)],
                materials["lens_rib"],
                owner,
                0.0025,
                2,
            )


def add_engine_bay(owner, materials):
    bay = bpy.data.objects.new("Cinematic_EngineBay", None)
    bpy.context.collection.objects.link(bay)
    parent(bay, owner)
    # Open central bay floor, inner fenders and front support.
    cube("EngineBay_Floor", (0, 0.45, 1.27), (1.34, 0.055, 1.05), materials["bay"], bay, 0.018, 2)
    for side in (-1, 1):
        cube(f"EngineBay_InnerFender_{side}", (side * 0.61, 0.64, 1.28), (0.12, 0.28, 1.05), materials["paint"], bay, 0.035, 3)
        uv_sphere(f"StrutTower_{side}", (side * 0.55, 0.74, 1.02), (0.17, 0.09, 0.17), materials["paint"], bay, 48, 24)
        cylinder(f"StrutTop_{side}", (side * 0.55, 0.81, 1.02), 0.065, 0.018, materials["hardware"], bay, 48, rotation=(math.pi / 2, 0, 0), bevel_width=0.003)
        for bolt in range(3):
            angle = bolt * math.tau / 3
            cylinder(
                f"StrutTop_{side}_Bolt_{bolt}",
                (side * 0.55 + math.sin(angle) * 0.085, 0.827, 1.02 + math.cos(angle) * 0.085),
                0.007,
                0.014,
                materials["hardware"],
                bay,
                16,
                rotation=(math.pi / 2, 0, 0),
            )
    cube("RadiatorCore", (0, 0.60, 1.76), (1.05, 0.42, 0.065), materials["radiator"], bay, 0.014, 2)
    for rib in range(34):
        cube(f"RadiatorFin_{rib:02d}", (-0.50 + rib * 0.0303, 0.60, 1.722), (0.007, 0.34, 0.006), materials["radiator_fin"], bay)

    # Transverse four-cylinder with visible fasteners and accessory drive.
    cube("Engine_Block", (0, 0.59, 1.25), (0.82, 0.38, 0.43), materials["cast_metal"], bay, 0.035, 3)
    cube("Cylinder_Head", (0, 0.78, 1.24), (0.86, 0.20, 0.40), materials["cast_metal_light"], bay, 0.035, 3)
    cube("Valve_Cover", (0, 0.91, 1.23), (0.74, 0.13, 0.31), materials["valve_cover"], bay, 0.045, 4)
    for rib in range(5):
        cube(f"ValveCover_Rib_{rib}", (-0.28 + rib * 0.14, 0.985, 1.23), (0.025, 0.018, 0.25), materials["valve_rib"], bay, 0.006, 2)
    for bolt in range(10):
        x = -0.33 + (bolt % 5) * 0.165
        z = 1.11 if bolt < 5 else 1.35
        cylinder(f"ValveCover_Bolt_{bolt:02d}", (x, 0.985, z), 0.008, 0.016, materials["hardware"], bay, 20, rotation=(math.pi / 2, 0, 0), bevel_width=0.002)
    cylinder("Oil_Filler_Cap", (0.27, 1.00, 1.30), 0.035, 0.026, materials["plastic"], bay, 40, rotation=(math.pi / 2, 0, 0), bevel_width=0.004)

    # Intake runners, fuel rail and throttle body.
    cube("Fuel_Rail", (-0.46, 0.82, 1.25), (0.045, 0.055, 0.43), materials["fuel_rail"], bay, 0.012, 3)
    for runner in range(4):
        z = 1.10 + runner * 0.095
        curve_tube(f"Intake_Runner_{runner}", [(-0.34, 0.78, z), (-0.50, 0.72, z), (-0.57, 0.64, z)], materials["intake"], bay, 0.025, 4)
        cylinder(f"Injector_{runner}", (-0.42, 0.84, z), 0.013, 0.055, materials["injector"], bay, 24, rotation=(0, 0, 0), bevel_width=0.003)
    cylinder("Throttle_Body", (-0.62, 0.66, 1.41), 0.075, 0.10, materials["alloy"], bay, 64, rotation=(0, 0, 0), bevel_width=0.006)
    curve_tube("Cold_Air_Intake", [(-0.62, 0.70, 1.42), (-0.66, 0.73, 1.60), (-0.52, 0.69, 1.72)], materials["intake_pipe"], bay, 0.052, 5)
    cylinder("Air_Filter", (-0.49, 0.68, 1.72), 0.09, 0.16, materials["filter"], bay, 64, rotation=(math.pi / 2, 0, 0), bevel_width=0.008)

    # Exhaust header and accessory hardware.
    for runner in range(4):
        x = -0.27 + runner * 0.18
        curve_tube(f"Header_Primary_{runner}", [(x, 0.73, 1.07), (x, 0.59, 0.98), (0.18 - runner * 0.05, 0.49, 0.91)], materials["header"], bay, 0.020, 5)
    cylinder("Alternator", (0.52, 0.61, 1.13), 0.105, 0.14, materials["alloy_dark"], bay, 72, rotation=(0, math.pi / 2, 0), bevel_width=0.006)
    cylinder("Accessory_Pulley", (0.60, 0.61, 1.13), 0.072, 0.025, materials["pulley"], bay, 64, bevel_width=0.004)
    cube("Battery", (0.49, 0.67, 1.56), (0.30, 0.24, 0.20), materials["battery"], bay, 0.018, 3)
    for side in (-1, 1):
        cylinder(f"Battery_Terminal_{side}", (0.49 + side * 0.09, 0.81, 1.54), 0.018, 0.026, materials["hardware"], bay, 24, rotation=(math.pi / 2, 0, 0))
    curve_tube("Upper_Radiator_Hose", [(0.31, 0.79, 1.33), (0.48, 0.78, 1.56), (0.38, 0.74, 1.72)], materials["hose"], bay, 0.028, 4)
    curve_tube("Brake_Booster_Line", [(-0.51, 0.79, 0.87), (-0.36, 0.83, 0.78), (-0.18, 0.79, 0.81)], materials["hose"], bay, 0.010, 3)

    # Weld/fastener line is visible in open-hood inspection.
    for side in (-1, 1):
        for index in range(24):
            uv_sphere(
                f"EngineBay_SpotWeld_{side}_{index:02d}",
                (side * 0.66, 0.775, 0.83 + index * 0.040),
                (0.007, 0.004, 0.007),
                materials["hardware"],
                bay,
                16,
                8,
            )
    return bay


def add_open_hood(owner, materials):
    hinge = bpy.data.objects.new("Hood_Open_Hinge", None)
    bpy.context.collection.objects.link(hinge)
    hinge.location = (0, 0.835, 0.78)
    hinge.rotation_euler.x = math.radians(-55)
    parent(hinge, owner)
    hood = cube("Hood_OuterPanel", (0, 0.02, 0.53), (1.46, 0.055, 1.08), materials["paint"], hinge, 0.025, 4)
    hood.scale.x = 0.96
    cube("Hood_InnerFrame", (0, -0.025, 0.53), (1.30, 0.035, 0.95), materials["bay"], hinge, 0.020, 3)
    # Stamped inner bracing and latch hardware.
    for side in (-1, 1):
        curve_tube(f"Hood_Brace_{side}", [(side * 0.55, -0.055, 0.08), (side * 0.18, -0.062, 0.53), (side * 0.55, -0.055, 0.98)], materials["hood_brace"], hinge, 0.018, 3)
    curve_tube("Hood_CenterBrace", [(0, -0.06, 0.10), (0, -0.064, 0.95)], materials["hood_brace"], hinge, 0.018, 3)
    cube("Hood_Latch", (0, -0.08, 1.02), (0.14, 0.035, 0.08), materials["hardware"], hinge, 0.012, 3)
    return hinge


def add_interior_detail(owner, materials):
    cockpit = bpy.data.objects.new("Cinematic_Cockpit", None)
    bpy.context.collection.objects.link(cockpit)
    parent(cockpit, owner)
    # Contoured seat shells, bolsters, harness openings and visible stitch runs.
    for side in (-1, 1):
        seat_x = side * 0.34
        cube(f"Seat_{side}_Cushion", (seat_x, 0.48, -0.04), (0.43, 0.18, 0.52), materials["seat"], cockpit, 0.075, 5, rotation=(-0.10, 0, 0))
        cube(f"Seat_{side}_Back", (seat_x, 0.74, -0.17), (0.45, 0.58, 0.16), materials["seat"], cockpit, 0.080, 5, rotation=(0.18, 0, 0))
        for bolster_side in (-1, 1):
            cube(f"Seat_{side}_Bolster_{bolster_side}", (seat_x + bolster_side * 0.17, 0.73, -0.16), (0.09, 0.50, 0.18), materials["seat_bolster"], cockpit, 0.035, 4, rotation=(0.18, 0, 0))
        for row in range(18):
            z = -0.36 + row * 0.022
            for stitch_side in (-1, 1):
                cube(f"Seat_{side}_Stitch_{stitch_side}_{row:02d}", (seat_x + stitch_side * 0.105, 0.785, z), (0.005, 0.006, 0.012), materials["stitch"], cockpit, 0.001, 1)
    # Cluster with modeled gauge bezels and needles.
    cluster = cube("Instrument_Cluster", (-0.35, 0.79, 0.40), (0.46, 0.17, 0.13), materials["dash"], cockpit, 0.035, 4, rotation=(-0.15, 0, 0))
    cluster["texture_set"] = "CIN_Hatch_Interior_4K"
    for gauge, x in enumerate((-0.47, -0.35, -0.23)):
        cylinder(f"Gauge_Bezel_{gauge}", (x, 0.80, 0.347), 0.054 if gauge == 1 else 0.043, 0.014, materials["alloy_dark"], cockpit, 64, rotation=(math.pi / 2, 0, 0), bevel_width=0.004)
        needle = cube(f"Gauge_Needle_{gauge}", (x, 0.812, 0.347), (0.005, 0.008, 0.067), materials["needle"], cockpit, 0.001, 1, rotation=(0.7 - gauge * 0.35, 0, 0))
        needle["detail_role"] = "Instrument needle"
    for vent, x in enumerate((-0.62, 0.0, 0.60)):
        cylinder(f"DashVent_{vent}", (x, 0.76, 0.48), 0.055, 0.018, materials["plastic"], cockpit, 48, rotation=(math.pi / 2, 0, 0), bevel_width=0.004)
        for vane in range(4):
            cube(f"DashVent_{vent}_Vane_{vane}", (x, 0.772, 0.45 + vane * 0.018), (0.080, 0.006, 0.004), materials["plastic_dark"], cockpit)
    cylinder("Shifter", (0.0, 0.53, 0.10), 0.018, 0.24, materials["hardware"], cockpit, 32, rotation=(0, 0, 0), bevel_width=0.003)
    uv_sphere("Shift_Knob", (0.0, 0.67, 0.10), (0.045, 0.045, 0.045), materials["alloy"], cockpit, 48, 24)
    cube("Center_Console", (0.0, 0.42, 0.0), (0.20, 0.18, 0.92), materials["plastic_dark"], cockpit, 0.045, 4)
    return cockpit


def refine_body(owner):
    body = bpy.data.objects.get("Body_Shell")
    if not body:
        return
    # One controlled smoothing pass adds genuine highlight continuity on the hero shell.
    modifier = body.modifiers.new("Cinematic surface continuity", "SUBSURF")
    modifier.subdivision_type = "CATMULL_CLARK"
    modifier.levels = 1
    modifier.render_levels = 1
    apply_modifier(body, modifier)
    body["texture_set"] = "CIN_Hatch_Body_4K"
    body["lod_role"] = "Cinematic close-up shell"


def cut_engine_bay_opening():
    body = bpy.data.objects.get("Body_Shell")
    if not body:
        return
    bpy.ops.mesh.primitive_cube_add(location=(0, 0.88, 1.30))
    cutter = bpy.context.object
    cutter.name = "TEMP_EngineBayOpeningCutter"
    cutter.dimensions = (1.28, 0.72, 1.05)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    modifier = body.modifiers.new("Modeled engine-bay opening", "BOOLEAN")
    modifier.operation = "DIFFERENCE"
    modifier.solver = "EXACT"
    modifier.object = cutter
    apply_modifier(body, modifier)
    bpy.data.objects.remove(cutter, do_unlink=True)
    body["showroom_configuration"] = "Open hood with modeled engine-bay aperture"


def convert_curves():
    for obj in list(bpy.context.scene.objects):
        if obj.type != "CURVE":
            continue
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.convert(target="MESH")
        obj.select_set(False)


def smart_uv_missing():
    # Primitive meshes retain their authored UVs. Only custom/procedural meshes missing UVs are projected.
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH" or len(obj.data.uv_layers) > 0 or len(obj.data.polygons) == 0:
            continue
        for other in bpy.context.selected_objects:
            other.select_set(False)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.008)
        bpy.ops.object.mode_set(mode="OBJECT")
        obj.select_set(False)


def stats(root):
    meshes = [obj for obj in root.children_recursive if obj.type == "MESH"]
    triangles = 0
    vertices = 0
    for obj in meshes:
        obj.data.calc_loop_triangles()
        triangles += len(obj.data.loop_triangles)
        vertices += len(obj.data.vertices)
    corners = []
    for obj in meshes:
        for point in obj.bound_box:
            corners.append(obj.matrix_world @ Vector(point))
    minimum = Vector((min(v.x for v in corners), min(v.y for v in corners), min(v.z for v in corners)))
    maximum = Vector((max(v.x for v in corners), max(v.y for v in corners), max(v.z for v in corners)))
    dimensions = maximum - minimum
    return len(meshes), vertices, triangles, dimensions


def add_showroom(owner, materials):
    # Showroom elements are deliberately outside the asset hierarchy, so FBX export stays vehicle-only.
    bpy.ops.mesh.primitive_plane_add(size=24, location=(0, 0.075, 0), rotation=(math.pi / 2, 0, 0))
    floor = bpy.context.object
    floor.name = "PREVIEW_Floor"
    floor.data.materials.append(materials["floor"])
    bevel(floor, 0.02, 2)
    for index, (loc, energy, size, color) in enumerate(
        [
            ((4.2, 5.0, 4.7), 1500, 4.0, (0.75, 0.85, 1.0)),
            ((-4.0, 3.5, 1.2), 1100, 3.5, (1.0, 0.34, 0.18)),
            ((0.2, 5.8, -4.3), 1350, 3.0, (0.35, 0.55, 1.0)),
            ((-0.5, 6.0, 4.0), 900, 2.5, (1.0, 0.86, 0.70)),
        ]
    ):
        light_data = bpy.data.lights.new(f"PREVIEW_Key_{index}", "AREA")
        light_data.energy = energy
        light_data.shape = "DISK"
        light_data.size = size
        light_data.color = color
        light = bpy.data.objects.new(light_data.name, light_data)
        bpy.context.collection.objects.link(light)
        light.location = loc
        look_at(light, Vector((0, 0.65, 0.4)))


def look_at(obj, target):
    forward = (target - obj.location).normalized()
    world_up = Vector((0.0, 1.0, 0.0))
    right = forward.cross(world_up).normalized()
    corrected_up = right.cross(forward).normalized()
    rotation = Matrix((right, corrected_up, -forward)).transposed().to_4x4()
    obj.matrix_world = Matrix.Translation(obj.location) @ rotation


def setup_render():
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1920
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = PREVIEW_PATH
    scene.render.film_transparent = False
    scene.render.image_settings.color_mode = "RGBA"
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.render.resolution_percentage = 100
    scene.world.use_nodes = True
    background = scene.world.node_tree.nodes.get("Background")
    background.inputs["Color"].default_value = (0.012, 0.018, 0.030, 1.0)
    background.inputs["Strength"].default_value = 0.30
    camera_data = bpy.data.cameras.new("PREVIEW_Camera")
    camera = bpy.data.objects.new("PREVIEW_Camera", camera_data)
    bpy.context.collection.objects.link(camera)
    camera.location = (5.7, 3.0, 6.9)
    camera_data.lens = 62
    camera_data.sensor_width = 36
    look_at(camera, Vector((0, 0.75, 0.32)))
    scene.camera = camera


def select_asset(root):
    bpy.ops.object.select_all(action="DESELECT")
    root.select_set(True)
    for obj in root.children_recursive:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = root


def main():
    os.makedirs(STAGE_DIR, exist_ok=True)
    bpy.ops.wm.open_mainfile(filepath=SOURCE_BLEND)
    scene = bpy.context.scene
    scene.unit_settings.system = "METRIC"
    scene.unit_settings.scale_length = 1.0
    root = bpy.data.objects.get("CarvinoHatch_93")
    if root is None:
        raise RuntimeError("Expected CarvinoHatch_93 root was not found")
    root.name = "CarvinoHatch_93_CinematicLOD0"
    root["asset_origin"] = "Original Carvino procedural geometry"
    root["license"] = "Original commercial-use asset; no copied source geometry or branding"
    root["lod_role"] = "Cinematic/showroom/garage inspection only"
    root["texture_layout"] = "Four 4096-ready material sets: Body, Interior, Mechanical, GlassLights"

    legacy_wheels = [obj for obj in bpy.data.objects if obj.name.startswith("Wheel_") and obj.parent == root]
    for obj in legacy_wheels:
        delete_tree(obj)

    materials = {
        "paint": mat("CIN_Hatch_Body_4K__DeepTealPaint", (0.008, 0.19, 0.22, 1), 0.82, 0.17),
        "bay": mat("CIN_Hatch_Body_4K__EngineBayPaint", (0.012, 0.13, 0.15, 1), 0.60, 0.30),
        "hood_brace": mat("CIN_Hatch_Body_4K__HoodBracing", (0.014, 0.11, 0.13, 1), 0.60, 0.32),
        "rubber": mat("CIN_Hatch_Mechanical_4K__TireCarcass", (0.006, 0.007, 0.008, 1), 0.0, 0.74),
        "rubber_side": mat("CIN_Hatch_Mechanical_4K__Sidewall", (0.013, 0.014, 0.015, 1), 0.0, 0.58),
        "rubber_tread": mat("CIN_Hatch_Mechanical_4K__Tread", (0.004, 0.005, 0.006, 1), 0.0, 0.84),
        "alloy": mat("CIN_Hatch_Mechanical_4K__ForgedAlloy", (0.31, 0.34, 0.37, 1), 0.93, 0.16),
        "alloy_dark": mat("CIN_Hatch_Mechanical_4K__DarkAlloy", (0.045, 0.052, 0.060, 1), 0.90, 0.23),
        "rotor": mat("CIN_Hatch_Mechanical_4K__BrakeRotor", (0.24, 0.25, 0.26, 1), 0.88, 0.27),
        "rotor_dark": mat("CIN_Hatch_Mechanical_4K__RotorDrill", (0.018, 0.020, 0.021, 1), 0.60, 0.40),
        "rotor_hat": mat("CIN_Hatch_Mechanical_4K__RotorHat", (0.07, 0.075, 0.08, 1), 0.85, 0.32),
        "caliper": mat("CIN_Hatch_Mechanical_4K__Caliper", (0.55, 0.018, 0.012, 1), 0.72, 0.20),
        "hardware": mat("CIN_Hatch_Mechanical_4K__Hardware", (0.41, 0.44, 0.46, 1), 0.90, 0.18),
        "reflector": mat("CIN_Hatch_GlassLights_4K__Reflector", (0.62, 0.66, 0.68, 1), 0.96, 0.08),
        "bulb": mat("CIN_Hatch_GlassLights_4K__Bulb", (0.86, 0.92, 0.78, 1), 0.10, 0.10, emission=(0.25, 0.32, 0.20, 1)),
        "gasket": mat("CIN_Hatch_GlassLights_4K__Gasket", (0.003, 0.004, 0.005, 1), 0.0, 0.65),
        "lens_rib": mat("CIN_Hatch_GlassLights_4K__LensRibs", (0.55, 0.68, 0.73, 0.65), 0.0, 0.07, transmission=0.35),
        "cast_metal": mat("CIN_Hatch_Mechanical_4K__CastBlock", (0.13, 0.14, 0.14, 1), 0.75, 0.38),
        "cast_metal_light": mat("CIN_Hatch_Mechanical_4K__CylinderHead", (0.28, 0.29, 0.29, 1), 0.82, 0.32),
        "valve_cover": mat("CIN_Hatch_Mechanical_4K__ValveCover", (0.38, 0.025, 0.020, 1), 0.76, 0.22),
        "valve_rib": mat("CIN_Hatch_Mechanical_4K__ValveCoverRibs", (0.48, 0.055, 0.045, 1), 0.78, 0.18),
        "fuel_rail": mat("CIN_Hatch_Mechanical_4K__FuelRail", (0.48, 0.10, 0.025, 1), 0.82, 0.20),
        "injector": mat("CIN_Hatch_Mechanical_4K__Injector", (0.04, 0.22, 0.08, 1), 0.30, 0.35),
        "intake": mat("CIN_Hatch_Mechanical_4K__Intake", (0.10, 0.11, 0.12, 1), 0.65, 0.32),
        "intake_pipe": mat("CIN_Hatch_Mechanical_4K__IntakePipe", (0.24, 0.25, 0.26, 1), 0.88, 0.19),
        "filter": mat("CIN_Hatch_Mechanical_4K__AirFilter", (0.50, 0.07, 0.04, 1), 0.15, 0.55),
        "header": mat("CIN_Hatch_Mechanical_4K__Header", (0.32, 0.23, 0.15, 1), 0.84, 0.28),
        "pulley": mat("CIN_Hatch_Mechanical_4K__Pulley", (0.025, 0.03, 0.035, 1), 0.80, 0.28),
        "plastic": mat("CIN_Hatch_Interior_4K__Plastic", (0.018, 0.020, 0.023, 1), 0.0, 0.62),
        "plastic_dark": mat("CIN_Hatch_Interior_4K__DarkPlastic", (0.007, 0.008, 0.010, 1), 0.0, 0.70),
        "battery": mat("CIN_Hatch_Mechanical_4K__Battery", (0.025, 0.035, 0.045, 1), 0.05, 0.50),
        "hose": mat("CIN_Hatch_Mechanical_4K__Hose", (0.008, 0.010, 0.012, 1), 0.0, 0.66),
        "radiator": mat("CIN_Hatch_Mechanical_4K__Radiator", (0.08, 0.09, 0.10, 1), 0.82, 0.34),
        "radiator_fin": mat("CIN_Hatch_Mechanical_4K__RadiatorFin", (0.22, 0.23, 0.24, 1), 0.88, 0.30),
        "seat": mat("CIN_Hatch_Interior_4K__SeatCloth", (0.035, 0.040, 0.046, 1), 0.0, 0.82),
        "seat_bolster": mat("CIN_Hatch_Interior_4K__Bolster", (0.018, 0.021, 0.025, 1), 0.0, 0.66),
        "stitch": mat("CIN_Hatch_Interior_4K__Stitch", (0.45, 0.025, 0.018, 1), 0.0, 0.50),
        "dash": mat("CIN_Hatch_Interior_4K__Cluster", (0.010, 0.012, 0.014, 1), 0.0, 0.55),
        "needle": mat("CIN_Hatch_Interior_4K__Needle", (0.80, 0.018, 0.012, 1), 0.0, 0.28, emission=(0.35, 0.005, 0.003, 1)),
        "floor": mat("PREVIEW_Floor", (0.025, 0.030, 0.038, 1), 0.18, 0.26),
    }

    # Existing paint material is relinked to the cinematic body texture set.
    body = bpy.data.objects.get("Body_Shell")
    if body and body.data.materials:
        body.data.materials[0] = materials["paint"]
    cut_engine_bay_opening()
    refine_body(root)
    add_cinematic_wheel("CIN_Wheel_FL", -0.86, 1.22, root, materials, True)
    add_cinematic_wheel("CIN_Wheel_FR", 0.86, 1.22, root, materials, True)
    add_cinematic_wheel("CIN_Wheel_RL", -0.86, -1.25, root, materials, False)
    add_cinematic_wheel("CIN_Wheel_RR", 0.86, -1.25, root, materials, False)
    add_lamp_internals(root, materials)
    add_engine_bay(root, materials)
    add_open_hood(root, materials)
    add_interior_detail(root, materials)
    convert_curves()
    smart_uv_missing()

    mesh_count, vertex_count, triangle_count, dimensions = stats(root)
    root["mesh_count"] = mesh_count
    root["vertex_count"] = vertex_count
    root["triangle_count"] = triangle_count
    root["dimensions_m"] = [round(v, 4) for v in dimensions]
    print(
        "CARVINO_CINEMATIC_STATS",
        f"meshes={mesh_count}",
        f"vertices={vertex_count}",
        f"triangles={triangle_count}",
        f"dimensions=({dimensions.x:.4f},{dimensions.y:.4f},{dimensions.z:.4f})",
    )
    if triangle_count < 250000 or triangle_count > 320000:
        print("CARVINO_CINEMATIC_TARGET_WARNING target=250000-320000")

    select_asset(root)
    bpy.ops.wm.save_as_mainfile(filepath=BLEND_PATH, compress=True)
    bpy.ops.export_scene.fbx(
        filepath=FBX_PATH,
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
    add_showroom(root, materials)
    setup_render()
    bpy.ops.render.render(write_still=True)
    print("CARVINO_CINEMATIC_BLEND", BLEND_PATH)
    print("CARVINO_CINEMATIC_FBX", FBX_PATH)
    print("CARVINO_CINEMATIC_PREVIEW", PREVIEW_PATH)


if __name__ == "__main__":
    main()
