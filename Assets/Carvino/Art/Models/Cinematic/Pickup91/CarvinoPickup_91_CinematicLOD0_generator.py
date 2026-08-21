import bpy
import math
import os
from mathutils import Matrix, Vector


SOURCE_BLEND = r"P:\chatgpt projects\Carvino Drag Sim\Assets\Carvino\Art\Models\CarvinoPickup_91.blend"
STAGE_DIR = os.path.dirname(os.path.abspath(__file__))
BLEND_PATH = os.path.join(STAGE_DIR, "CarvinoPickup_91_CinematicLOD0.blend")
FBX_PATH = os.path.join(STAGE_DIR, "CarvinoPickup_91_CinematicLOD0.fbx")
PREVIEW_PATH = os.path.join(STAGE_DIR, "CarvinoPickup_91_CinematicLOD0_Preview.png")


def mat(name, color, metallic=0.0, roughness=0.4, transmission=0.0, emission=None):
    material = bpy.data.materials.get(name) or bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Metallic"].default_value = metallic
    bsdf.inputs["Roughness"].default_value = roughness
    if "Coat Weight" in bsdf.inputs:
        bsdf.inputs["Coat Weight"].default_value = 0.55 if metallic > 0.1 else 0.12
    if "Transmission Weight" in bsdf.inputs:
        bsdf.inputs["Transmission Weight"].default_value = transmission
    if emission is not None:
        bsdf.inputs["Emission Color"].default_value = emission
        bsdf.inputs["Emission Strength"].default_value = 2.5
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
    modifier = obj.modifiers.new("Production edge radii", "BEVEL")
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
    return parent(smooth(obj), owner)


def sphere(name, location, scale, material, owner, segments=48, rings=24):
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


def tread_mesh(name, major_radius, minor_radius, material, owner):
    """Build 336 real tread blocks as one mesh to keep Blender/Unity hierarchy efficient."""
    vertices = []
    faces = []
    dimensions = (0.038, 0.070, 0.017)
    corner_signs = (
        (-1, -1, -1), (-1, -1, 1), (-1, 1, 1), (-1, 1, -1),
        (1, -1, -1), (1, -1, 1), (1, 1, 1), (1, 1, -1),
    )
    box_faces = ((0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (3, 2, 6, 7), (1, 5, 6, 2), (0, 3, 7, 4))
    radius = major_radius + minor_radius - 0.006
    for index in range(84):
        angle = index * math.tau / 84.0
        for band, side_offset in enumerate((-0.085, -0.029, 0.029, 0.085)):
            rotation = -angle + (0.16 if (index + band) % 2 else -0.16)
            center = Vector((side_offset, math.sin(angle) * radius, math.cos(angle) * radius))
            base = len(vertices)
            cosine = math.cos(rotation)
            sine = math.sin(rotation)
            for sx, sy, sz in corner_signs:
                local_y = sy * dimensions[1] * 0.5
                local_z = sz * dimensions[2] * 0.5
                rotated_y = local_y * cosine - local_z * sine
                rotated_z = local_y * sine + local_z * cosine
                vertices.append((center.x + sx * dimensions[0] * 0.5, center.y + rotated_y, center.z + rotated_z))
            faces.extend(tuple(base + corner for corner in face) for face in box_faces)
    mesh = bpy.data.meshes.new(name + " Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    obj["detail_role"] = "336 modeled directional tread blocks"
    return parent(obj, owner)


def remap_original_materials(root, materials):
    for obj in root.children_recursive:
        if obj.type != "MESH":
            continue
        name = obj.name.lower()
        if any(word in name for word in ("glass", "windshield", "window")):
            replacement = materials["glass"]
        elif any(word in name for word in ("headlamp", "reverse")):
            replacement = materials["lens"]
        elif any(word in name for word in ("tail_lamp", "tail lamp")):
            replacement = materials["red_lens"]
        elif any(word in name for word in ("marker", "amber")):
            replacement = materials["amber_lens"]
        elif any(word in name for word in ("interior", "seat", "dash", "steering", "console")):
            replacement = materials["interior"]
        elif any(word in name for word in ("bumper", "grille", "axle", "exhaust", "rim", "hub", "spoke")):
            replacement = materials["metal"]
        elif any(word in name for word in ("tire", "seal", "trim", "valance")):
            replacement = materials["tire"]
        elif "bed_floor" in name or "bed liner" in name:
            replacement = materials["bedliner"]
        else:
            replacement = materials["paint"]
        obj.data.materials.clear()
        obj.data.materials.append(replacement)
        obj["texture_set"] = replacement.name.split("__")[0]


def add_cinematic_wheel(name, x, y, z, owner, materials, rear=False):
    wheel = bpy.data.objects.new(name, None)
    bpy.context.collection.objects.link(wheel)
    wheel.location = (x, y, z)
    wheel["wheel_root"] = True
    wheel["axle"] = "rear" if rear else "front"
    parent(wheel, owner)
    outside = 1 if x > 0 else -1
    tire_major = 0.430 if rear else 0.390
    tire_minor = 0.105 if rear else 0.095
    wheel_width = 0.315 if rear else 0.275

    torus(name + "_TireCarcass", (0, 0, 0), tire_major, tire_minor, materials["tire"], wheel, 160, 28)
    for lip_x in (-wheel_width * 0.44, wheel_width * 0.44):
        torus(name + f"_SidewallRib_{lip_x:+.3f}", (lip_x, 0, 0), tire_major, 0.008, materials["sidewall"], wheel, 96, 10)
    # Modeled symmetric street/drag tread, batched as one efficient mesh per wheel.
    tread_mesh(name + "_ModeledTread", tire_major, tire_minor, materials["tread"], wheel)

    # Deep forged wheel barrel, step lips and ten split spokes.
    torus(name + "_RimOuterLip", (outside * wheel_width * 0.42, 0, 0), 0.285, 0.018, materials["alloy"], wheel, 144, 14)
    torus(name + "_RimInnerLip", (-outside * wheel_width * 0.34, 0, 0), 0.265, 0.012, materials["alloy_dark"], wheel, 112, 12)
    cylinder(name + "_RimBarrel", (0, 0, 0), 0.274, wheel_width * 0.82, materials["alloy_dark"], wheel, 144, bevel_width=0.005)
    face_x = outside * wheel_width * 0.46
    cylinder(name + "_Hub", (face_x, 0, 0), 0.072, 0.035, materials["alloy"], wheel, 80, bevel_width=0.004)
    for index in range(10):
        angle = index * math.tau / 10
        spoke = cube(
            name + f"_ForgedSpoke_{index:02d}",
            (face_x, math.sin(angle) * 0.145, math.cos(angle) * 0.145),
            (0.030, 0.040, 0.300),
            materials["alloy"],
            wheel,
            0.010,
            3,
        )
        spoke.rotation_euler.x = -angle

    # Detailed rotor, cooling vanes, drilled face, caliper, studs and lugs.
    disc_x = outside * wheel_width * 0.34
    rotor_radius = 0.205 if not rear else 0.190
    torus(name + "_RotorFace", (disc_x, 0, 0), rotor_radius, 0.034, materials["rotor"], wheel, 144, 14)
    cylinder(name + "_RotorHat", (disc_x, 0, 0), 0.094, 0.026, materials["rotor_hat"], wheel, 80, bevel_width=0.004)
    for index in range(32):
        angle = index * math.tau / 32
        for ring_radius in (rotor_radius - 0.018, rotor_radius + 0.018):
            cylinder(
                name + f"_RotorDrill_{index:02d}_{ring_radius:.2f}",
                (disc_x + outside * 0.020, math.sin(angle) * ring_radius, math.cos(angle) * ring_radius),
                0.005,
                0.010,
                materials["rotor_dark"],
                wheel,
                18,
            )
    for vane in range(24):
        angle = vane * math.tau / 24
        blade = cube(
            name + f"_RotorVane_{vane:02d}",
            (disc_x, math.sin(angle) * 0.155, math.cos(angle) * 0.155),
            (0.030, 0.012, 0.115),
            materials["rotor_dark"],
            wheel,
            0.002,
            1,
        )
        blade.rotation_euler.x = -angle
    cube(name + "_Caliper", (outside * wheel_width * 0.48, -0.18, 0.02), (0.070, 0.11, 0.205), materials["caliper"], wheel, 0.024, 5)
    for index in range(5):
        angle = index * math.tau / 5
        y_loc = math.sin(angle) * 0.050
        z_loc = math.cos(angle) * 0.050
        cylinder(name + f"_WheelStud_{index}", (outside * wheel_width * 0.52, y_loc, z_loc), 0.009, 0.026, materials["hardware"], wheel, 24, bevel_width=0.002)
        cylinder(name + f"_LugNut_{index}", (outside * wheel_width * 0.59, y_loc, z_loc), 0.014, 0.024, materials["lug"], wheel, 24, bevel_width=0.003)
    return wheel


def add_lamp_detail(owner, materials):
    lighting = bpy.data.objects.new("Cinematic_LampAssemblies", None)
    bpy.context.collection.objects.link(lighting)
    parent(lighting, owner)
    # Rectangular sealed-beam-era headlamps with reflector optics and ribbed lenses.
    for side in (-1, 1):
        x_center = side * 0.69
        cube(f"Headlamp_{side}_ReflectorBox", (x_center, 0.87, 2.682), (0.47, 0.22, 0.045), materials["reflector"], lighting, 0.035, 4)
        for reflector_index in range(2):
            x = x_center + side * (reflector_index - 0.5) * 0.18
            sphere(f"Headlamp_{side}_Reflector_{reflector_index}", (x, 0.88, 2.708), (0.105, 0.075, 0.032), materials["reflector"], lighting, 56, 28)
            sphere(f"Headlamp_{side}_Bulb_{reflector_index}", (x, 0.88, 2.744), (0.026, 0.026, 0.014), materials["bulb"], lighting, 40, 20)
        for rib in range(12):
            x = x_center - 0.205 + rib * 0.037
            cube(f"Headlamp_{side}_LensRib_{rib:02d}", (x, 0.88, 2.752), (0.003, 0.18, 0.008), materials["lens_rib"], lighting, 0.001, 1)
        curve_tube(
            f"Headlamp_{side}_PerimeterSeal",
            [(x_center - 0.235, 0.765, 2.744), (x_center + 0.235, 0.765, 2.744), (x_center + 0.235, 0.975, 2.744), (x_center - 0.235, 0.975, 2.744)],
            materials["gasket"], lighting, 0.009, 3, True,
        )
        # Tall original tail-lamp unit with red/amber/reverse optics.
        for lamp_index, (y, material) in enumerate(((1.20, materials["red_lens"]), (0.98, materials["amber_lens"]), (0.78, materials["lens"]))):
            cube(f"TailLamp_{side}_{lamp_index}_Reflector", (side * 1.044, y, -2.580), (0.055, 0.185, 0.095), materials["reflector"], lighting, 0.015, 3)
            cube(f"TailLamp_{side}_{lamp_index}_Lens", (side * 1.077, y, -2.587), (0.018, 0.205, 0.115), material, lighting, 0.012, 3)
            for rib in range(5):
                cube(f"TailLamp_{side}_{lamp_index}_Rib_{rib}", (side * 1.090, y - 0.072 + rib * 0.036, -2.590), (0.006, 0.004, 0.105), materials["lens_rib"], lighting)
    return lighting


def add_engine_bay(owner, materials):
    bay = bpy.data.objects.new("Cinematic_EngineBay", None)
    bpy.context.collection.objects.link(bay)
    parent(bay, owner)
    cube("EngineBay_Floor", (0, 0.66, 1.62), (1.55, 0.07, 1.45), materials["bay"], bay, 0.025, 3)
    for side in (-1, 1):
        cube(f"InnerFender_{side}", (side * 0.73, 0.84, 1.63), (0.16, 0.36, 1.42), materials["paint"], bay, 0.050, 4)
        sphere(f"StrutTower_{side}", (side * 0.66, 0.96, 1.29), (0.19, 0.12, 0.20), materials["paint"], bay, 56, 28)
        cylinder(f"StrutTop_{side}", (side * 0.66, 1.085, 1.29), 0.075, 0.020, materials["hardware"], bay, 64, rotation=(math.pi / 2, 0, 0), bevel_width=0.004)
        for bolt in range(3):
            angle = bolt * math.tau / 3
            cylinder(f"StrutTop_{side}_Bolt_{bolt}", (side * 0.66 + math.sin(angle) * 0.10, 1.102, 1.29 + math.cos(angle) * 0.10), 0.008, 0.016, materials["hardware"], bay, 20, rotation=(math.pi / 2, 0, 0))

    # Longitudinal original V8 representation suited to future V6/V8/LS swaps.
    cube("Engine_Block", (0, 0.79, 1.60), (0.70, 0.50, 0.82), materials["cast"], bay, 0.060, 4, rotation=(0, 0, 0))
    for side in (-1, 1):
        cube(f"CylinderHead_{side}", (side * 0.38, 0.93, 1.60), (0.30, 0.24, 0.80), materials["alloy"], bay, 0.045, 4)
        cube(f"ValveCover_{side}", (side * 0.43, 1.08, 1.60), (0.24, 0.15, 0.70), materials["valve"], bay, 0.045, 5)
        for bolt in range(6):
            cylinder(f"ValveCover_{side}_Bolt_{bolt}", (side * 0.43, 1.165, 1.34 + bolt * 0.104), 0.008, 0.016, materials["hardware"], bay, 20, rotation=(math.pi / 2, 0, 0), bevel_width=0.002)
        for runner in range(4):
            z = 1.34 + runner * 0.18
            curve_tube(f"Header_{side}_Primary_{runner}", [(side * 0.50, 0.92, z), (side * 0.65, 0.73, z - 0.04), (side * 0.60, 0.56, 1.46 + runner * 0.07)], materials["header"], bay, 0.025, 5)

    cube("IntakeManifold", (0, 1.10, 1.61), (0.48, 0.20, 0.72), materials["intake"], bay, 0.075, 5)
    cylinder("ThrottleBody", (0, 1.13, 2.02), 0.095, 0.16, materials["alloy"], bay, 80, rotation=(0, 0, 0), bevel_width=0.008)
    curve_tube("IntakeTube", [(0, 1.13, 2.10), (0.25, 1.08, 2.25), (0.54, 0.98, 2.34)], materials["intake_pipe"], bay, 0.068, 6)
    cylinder("AirFilter", (0.60, 0.97, 2.34), 0.115, 0.18, materials["filter"], bay, 80, rotation=(math.pi / 2, 0, 0), bevel_width=0.008)
    cube("FuelRail_Left", (-0.28, 1.14, 1.62), (0.045, 0.055, 0.68), materials["fuel"], bay, 0.012, 3)
    cube("FuelRail_Right", (0.28, 1.14, 1.62), (0.045, 0.055, 0.68), materials["fuel"], bay, 0.012, 3)
    for side in (-1, 1):
        for injector in range(4):
            cylinder(f"Injector_{side}_{injector}", (side * 0.30, 1.08, 1.35 + injector * 0.18), 0.014, 0.065, materials["injector"], bay, 28, rotation=(0, 0, 0), bevel_width=0.003)

    # Front dress, belts, cooling package, battery, booster, hoses and wiring.
    cube("RadiatorCore", (0, 0.79, 2.36), (1.20, 0.46, 0.075), materials["radiator"], bay, 0.018, 3)
    for fin in range(44):
        cube(f"RadiatorFin_{fin:02d}", (-0.57 + fin * 0.0265, 0.79, 2.321), (0.006, 0.38, 0.006), materials["radiator_fin"], bay)
    for pulley, (x, y, radius) in enumerate(((0, 0.74, 0.16), (-0.26, 0.79, 0.10), (0.27, 0.80, 0.095))):
        cylinder(f"AccessoryPulley_{pulley}", (x, y, 2.03), radius, 0.040, materials["pulley"], bay, 96, rotation=(0, 0, 0), bevel_width=0.005)
        torus(f"AccessoryPulley_{pulley}_Groove", (x, y, 2.055), radius - 0.010, 0.009, materials["belt"], bay, 96, 12, rotation=(0, 0, 0))
    cube("Battery", (0.55, 0.93, 2.08), (0.34, 0.25, 0.28), materials["battery"], bay, 0.025, 4)
    cylinder("BrakeBooster", (-0.58, 0.99, 0.99), 0.18, 0.12, materials["booster"], bay, 96, rotation=(0, 0, 0), bevel_width=0.008)
    cube("MasterCylinder", (-0.58, 1.06, 1.20), (0.14, 0.12, 0.35), materials["alloy"], bay, 0.025, 4)
    curve_tube("UpperRadiatorHose", [(0.35, 1.02, 1.76), (0.50, 1.00, 2.13), (0.38, 0.94, 2.33)], materials["hose"], bay, 0.034, 5)
    curve_tube("LowerRadiatorHose", [(-0.30, 0.68, 1.82), (-0.48, 0.66, 2.14), (-0.40, 0.73, 2.34)], materials["hose"], bay, 0.030, 5)
    for side in (-1, 1):
        curve_tube(f"EngineHarness_{side}", [(side * 0.40, 1.12, 1.31), (side * 0.51, 1.08, 1.62), (side * 0.42, 1.08, 1.93)], materials["wire"], bay, 0.009, 3)
        for weld in range(24):
            sphere(f"BaySpotWeld_{side}_{weld:02d}", (side * 0.80, 1.04, 1.04 + weld * 0.057), (0.007, 0.004, 0.007), materials["hardware"], bay, 16, 8)
    return bay


def cut_engine_bay_opening():
    body = bpy.data.objects.get("Lower_Body_Shell")
    if body is None:
        raise RuntimeError("Lower_Body_Shell was not found for the engine-bay opening")
    bpy.ops.mesh.primitive_cube_add(location=(0, 1.12, 1.72))
    cutter = bpy.context.object
    cutter.name = "TEMP_EngineBayOpeningCutter"
    cutter.dimensions = (1.56, 1.10, 1.48)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    modifier = body.modifiers.new("Modeled engine-bay aperture", "BOOLEAN")
    modifier.operation = "DIFFERENCE"
    modifier.solver = "EXACT"
    modifier.object = cutter
    apply_modifier(body, modifier)
    bpy.data.objects.remove(cutter, do_unlink=True)
    body["showroom_configuration"] = "Open hood with modeled engine-bay aperture"


def add_open_hood(owner, materials):
    for name in ("Hood", "Hood_Center_Rise"):
        obj = bpy.data.objects.get(name)
        if obj:
            bpy.data.objects.remove(obj, do_unlink=True)
    hinge = bpy.data.objects.new("Hood_Open_Hinge", None)
    bpy.context.collection.objects.link(hinge)
    hinge.location = (0, 1.42, 1.15)
    hinge.rotation_euler.x = math.radians(-58)
    parent(hinge, owner)
    cube("Hood_OuterPanel", (0, 0.02, 0.65), (1.92, 0.060, 1.55), materials["paint"], hinge, 0.050, 5)
    cube("Hood_InnerPanel", (0, -0.035, 0.65), (1.74, 0.036, 1.37), materials["bay"], hinge, 0.045, 4)
    for side in (-1, 1):
        curve_tube(f"HoodBrace_{side}", [(side * 0.75, -0.065, 0.06), (side * 0.22, -0.075, 0.64), (side * 0.72, -0.065, 1.28)], materials["brace"], hinge, 0.020, 4)
    curve_tube("HoodCenterBrace", [(0, -0.075, 0.08), (0, -0.080, 1.28)], materials["brace"], hinge, 0.020, 4)
    cube("HoodLatch", (0, -0.095, 1.37), (0.16, 0.040, 0.09), materials["hardware"], hinge, 0.014, 3)
    return hinge


def add_cockpit(owner, materials):
    cockpit = bpy.data.objects.new("Cinematic_Cockpit", None)
    bpy.context.collection.objects.link(cockpit)
    parent(cockpit, owner)
    cube("CabFloor", (0, 0.47, 0.08), (1.82, 0.10, 1.45), materials["carpet"], cockpit, 0.04, 3)
    cube("Dashboard", (0, 1.29, 0.67), (1.77, 0.33, 0.35), materials["dash"], cockpit, 0.08, 5, rotation=(-0.08, 0, 0))
    cube("DashPad", (0, 1.49, 0.69), (1.70, 0.08, 0.44), materials["dash_pad"], cockpit, 0.045, 4)
    # Two era-correct cloth seats with bolsters, seams, rails and belts.
    for side in (-1, 1):
        x = side * 0.46
        cube(f"Seat_{side}_Cushion", (x, 0.62, -0.05), (0.58, 0.20, 0.62), materials["seat"], cockpit, 0.09, 5, rotation=(-0.08, 0, 0))
        cube(f"Seat_{side}_Back", (x, 0.94, -0.24), (0.58, 0.68, 0.18), materials["seat"], cockpit, 0.10, 5, rotation=(0.20, 0, 0))
        for bolster_side in (-1, 1):
            cube(f"Seat_{side}_Bolster_{bolster_side}", (x + bolster_side * 0.23, 0.94, -0.22), (0.10, 0.58, 0.20), materials["bolster"], cockpit, 0.045, 4, rotation=(0.20, 0, 0))
        for row in range(20):
            z = -0.47 + row * 0.027
            for stitch_side in (-1, 1):
                cube(f"Seat_{side}_Stitch_{stitch_side}_{row:02d}", (x + stitch_side * 0.15, 1.005, z), (0.005, 0.006, 0.015), materials["stitch"], cockpit, 0.001, 1)
        cube(f"SeatRail_{side}_L", (x - 0.18, 0.43, -0.08), (0.045, 0.055, 0.72), materials["hardware"], cockpit, 0.007, 2)
        cube(f"SeatRail_{side}_R", (x + 0.18, 0.43, -0.08), (0.045, 0.055, 0.72), materials["hardware"], cockpit, 0.007, 2)
        curve_tube(f"Seatbelt_{side}", [(side * 0.84, 1.56, -0.45), (x + side * 0.18, 1.00, -0.22), (x + side * 0.12, 0.63, 0.02)], materials["belt_fabric"], cockpit, 0.018, 2)

    # Driver cluster, steering, pedals, HVAC and floor shifter.
    cluster = cube("InstrumentCluster", (-0.48, 1.39, 0.58), (0.60, 0.18, 0.20), materials["cluster"], cockpit, 0.050, 5, rotation=(-0.12, 0, 0))
    cluster["texture_set"] = "CIN_Pickup_Interior_4K"
    for gauge, x in enumerate((-0.66, -0.49, -0.32)):
        cylinder(f"GaugeBezel_{gauge}", (x, 1.41, 0.475), 0.070 if gauge == 1 else 0.055, 0.018, materials["alloy_dark"], cockpit, 72, rotation=(math.pi / 2, 0, 0), bevel_width=0.005)
        cube(f"GaugeNeedle_{gauge}", (x, 1.425, 0.475), (0.005, 0.008, 0.080), materials["needle"], cockpit, 0.001, 1, rotation=(0.65 - gauge * 0.32, 0, 0))
        for tick in range(12):
            angle = tick * math.tau / 12
            cube(f"Gauge_{gauge}_Tick_{tick}", (x + math.sin(angle) * 0.045, 1.429, 0.475 + math.cos(angle) * 0.045), (0.003, 0.005, 0.010), materials["gauge_mark"], cockpit, 0.001, 1, rotation=(-angle, 0, 0))
    cylinder("SteeringColumn", (-0.49, 1.08, 0.34), 0.032, 0.45, materials["hardware"], cockpit, 48, rotation=(math.radians(22), 0, 0), bevel_width=0.004)
    torus("SteeringWheel", (-0.49, 1.07, 0.18), 0.19, 0.025, materials["steering"], cockpit, 144, 20, rotation=(math.radians(68), 0, 0))
    cylinder("SteeringHub", (-0.49, 1.07, 0.18), 0.065, 0.055, materials["steering"], cockpit, 64, rotation=(math.radians(68), 0, 0), bevel_width=0.008)
    for spoke in range(3):
        angle = spoke * math.tau / 3
        part = cube(f"SteeringSpoke_{spoke}", (-0.49, 1.06, 0.18), (0.030, 0.025, 0.29), materials["steering"], cockpit, 0.008, 3)
        part.rotation_euler.x = -angle
    cube("CenterConsole", (0, 0.60, 0.16), (0.24, 0.24, 0.75), materials["console"], cockpit, 0.055, 4)
    cylinder("Shifter", (0, 0.76, 0.22), 0.020, 0.32, materials["hardware"], cockpit, 40, rotation=(0, 0, 0), bevel_width=0.004)
    sphere("ShiftKnob", (0, 0.96, 0.22), (0.055, 0.055, 0.055), materials["alloy"], cockpit, 56, 28)
    for vent, x in enumerate((-0.78, 0.0, 0.78)):
        cube(f"DashVent_{vent}", (x, 1.41, 0.72), (0.18, 0.05, 0.09), materials["vent"], cockpit, 0.018, 3)
        for vane in range(5):
            cube(f"DashVent_{vent}_Vane_{vane}", (x - 0.07 + vane * 0.035, 1.441, 0.72), (0.008, 0.012, 0.065), materials["vent_dark"], cockpit, 0.002, 1)
    for pedal, x in enumerate((-0.63, -0.50, -0.36)):
        cube(f"Pedal_{pedal}", (x, 0.48, 0.54), (0.10 if pedal < 2 else 0.08, 0.035, 0.16), materials["pedal"], cockpit, 0.012, 3, rotation=(-0.28, 0, 0))
    return cockpit


def add_bed_detail(owner, materials):
    bed = bpy.data.objects.new("Cinematic_BedAndTailgate", None)
    bpy.context.collection.objects.link(bed)
    parent(bed, owner)
    # Corrugated steel/liner floor, wheel tubs, tie-downs and visible hardware.
    for rib in range(25):
        x = -0.78 + rib * 0.065
        cube(f"BedFloorCorrugation_{rib:02d}", (x, 1.025, -1.55), (0.025, 0.035, 1.74), materials["bedliner"], bed, 0.006, 2)
    for side in (-1, 1):
        sphere(f"BedWheelTub_{side}", (side * 0.69, 1.10, -1.57), (0.32, 0.25, 0.55), materials["bedliner"], bed, 56, 28)
        for index, z in enumerate((-2.25, -0.92)):
            torus(f"BedTieDown_{side}_{index}", (side * 0.83, 1.25, z), 0.042, 0.009, materials["hardware"], bed, 48, 12, rotation=(0, 0, 0))
        for bolt in range(14):
            cylinder(f"BedRailBolt_{side}_{bolt:02d}", (side * 0.93, 1.46, -2.38 + bolt * 0.13), 0.007, 0.014, materials["hardware"], bed, 20, rotation=(math.pi / 2, 0, 0), bevel_width=0.002)
    cube("TailgateInner", (0, 1.02, -2.51), (1.82, 0.66, 0.08), materials["paint"], bed, 0.035, 4)
    for rib in range(8):
        cube(f"TailgateStamping_{rib}", (-0.72 + rib * 0.205, 1.03, -2.558), (0.11, 0.48, 0.025), materials["bedliner"], bed, 0.020, 3)
    for side in (-1, 1):
        cylinder(f"TailgateHinge_{side}", (side * 0.74, 0.71, -2.54), 0.040, 0.11, materials["hardware"], bed, 56, rotation=(0, math.pi / 2, 0), bevel_width=0.006)
        curve_tube(f"TailgateCable_{side}", [(side * 0.80, 1.34, -2.43), (side * 0.86, 1.07, -2.53), (side * 0.75, 0.80, -2.55)], materials["cable"], bed, 0.009, 3)
    cube("TailgateHandle", (0, 1.21, -2.575), (0.25, 0.09, 0.035), materials["handle"], bed, 0.018, 4)
    return bed


def refine_body(root):
    for name in ("Lower_Body_Shell", "Cab_Lower", "Cab_Roof", "Cab_Rear_Panel"):
        obj = bpy.data.objects.get(name)
        if obj is None or obj.type != "MESH":
            continue
        modifier = obj.modifiers.new("Cinematic surface continuity", "SUBSURF")
        modifier.subdivision_type = "CATMULL_CLARK"
        modifier.levels = 1
        modifier.render_levels = 1
        apply_modifier(obj, modifier)
        obj["lod_role"] = "Cinematic close-up shell"


def convert_curves():
    for obj in list(bpy.context.scene.objects):
        if obj.type != "CURVE":
            continue
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.convert(target="MESH")
        obj.select_set(False)


def ensure_uv_layers(root):
    for obj in root.children_recursive:
        if obj.type != "MESH" or len(obj.data.polygons) == 0 or len(obj.data.uv_layers) > 0:
            continue
        for selected in bpy.context.selected_objects:
            selected.select_set(False)
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.mode_set(mode="EDIT")
        bpy.ops.mesh.select_all(action="SELECT")
        bpy.ops.uv.smart_project(angle_limit=math.radians(66), island_margin=0.008)
        bpy.ops.object.mode_set(mode="OBJECT")
        obj.select_set(False)


def stats(root):
    meshes = [obj for obj in root.children_recursive if obj.type == "MESH"]
    triangles = vertices = 0
    corners = []
    for obj in meshes:
        obj.data.calc_loop_triangles()
        triangles += len(obj.data.loop_triangles)
        vertices += len(obj.data.vertices)
        corners.extend(obj.matrix_world @ Vector(point) for point in obj.bound_box)
    minimum = Vector((min(v.x for v in corners), min(v.y for v in corners), min(v.z for v in corners)))
    maximum = Vector((max(v.x for v in corners), max(v.y for v in corners), max(v.z for v in corners)))
    return len(meshes), vertices, triangles, maximum - minimum


def look_at(obj, target):
    forward = (target - obj.location).normalized()
    right = forward.cross(Vector((0, 1, 0))).normalized()
    corrected_up = right.cross(forward).normalized()
    obj.matrix_world = Matrix.Translation(obj.location) @ Matrix((right, corrected_up, -forward)).transposed().to_4x4()


def add_showroom(materials):
    bpy.ops.mesh.primitive_plane_add(size=24, location=(0, 0.05, 0), rotation=(math.pi / 2, 0, 0))
    floor = bpy.context.object
    floor.name = "PREVIEW_Floor"
    floor.data.materials.append(materials["floor"])
    for index, (location, energy, size, color) in enumerate((
        ((4.5, 5.4, 5.8), 1700, 4.5, (0.74, 0.84, 1.0)),
        ((-4.2, 3.4, 2.2), 1250, 3.6, (1.0, 0.32, 0.14)),
        ((0.0, 5.8, -5.0), 1500, 3.4, (0.30, 0.50, 1.0)),
        ((-0.8, 6.2, 4.8), 1000, 2.8, (1.0, 0.86, 0.68)),
    )):
        light_data = bpy.data.lights.new(f"PREVIEW_Key_{index}", "AREA")
        light_data.energy = energy
        light_data.shape = "DISK"
        light_data.size = size
        light_data.color = color
        light = bpy.data.objects.new(light_data.name, light_data)
        bpy.context.collection.objects.link(light)
        light.location = location
        look_at(light, Vector((0, 0.85, 0.25)))


def setup_render():
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1920
    scene.render.resolution_y = 1080
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = PREVIEW_PATH
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.world.use_nodes = True
    background = scene.world.node_tree.nodes.get("Background")
    background.inputs["Color"].default_value = (0.010, 0.015, 0.026, 1)
    background.inputs["Strength"].default_value = 0.28
    camera_data = bpy.data.cameras.new("PREVIEW_Camera")
    camera = bpy.data.objects.new("PREVIEW_Camera", camera_data)
    bpy.context.collection.objects.link(camera)
    camera.location = (6.6, 3.4, 7.3)
    camera_data.lens = 64
    look_at(camera, Vector((0, 0.90, 0.25)))
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
    root = bpy.data.objects.get("CarvinoPickup_91")
    if root is None:
        raise RuntimeError("Expected CarvinoPickup_91 root was not found")
    root.name = "CarvinoPickup_91_CinematicLOD0"
    root["asset_origin"] = "Original Carvino procedural geometry"
    root["license"] = "Original commercial-use asset; no copied geometry, logos or branding"
    root["lod_role"] = "Cinematic/showroom/garage inspection only"
    root["texture_layout"] = "Four 4096-ready sets: Body, Interior, Mechanical, GlassLights"

    materials = {
        "paint": mat("CIN_Pickup_Body_4K__DeepBluePaint", (0.008, 0.075, 0.24, 1), 0.82, 0.17),
        "bay": mat("CIN_Pickup_Body_4K__EngineBayPaint", (0.008, 0.052, 0.16, 1), 0.65, 0.29),
        "brace": mat("CIN_Pickup_Body_4K__HoodBracing", (0.010, 0.045, 0.13, 1), 0.68, 0.31),
        "bedliner": mat("CIN_Pickup_Body_4K__BedLiner", (0.018, 0.021, 0.025, 1), 0.0, 0.90),
        "glass": mat("CIN_Pickup_GlassLights_4K__TintedGlass", (0.015, 0.050, 0.070, 0.60), 0.05, 0.09, 0.40),
        "lens": mat("CIN_Pickup_GlassLights_4K__ClearLens", (0.58, 0.70, 0.76, 0.70), 0.0, 0.07, 0.30),
        "red_lens": mat("CIN_Pickup_GlassLights_4K__RedLens", (0.50, 0.006, 0.010, 0.75), 0.0, 0.11, 0.22, (0.12, 0.001, 0.001, 1)),
        "amber_lens": mat("CIN_Pickup_GlassLights_4K__AmberLens", (0.62, 0.10, 0.004, 0.75), 0.0, 0.11, 0.20, (0.18, 0.025, 0.001, 1)),
        "reflector": mat("CIN_Pickup_GlassLights_4K__Reflector", (0.62, 0.66, 0.69, 1), 0.96, 0.08),
        "bulb": mat("CIN_Pickup_GlassLights_4K__Bulb", (0.86, 0.92, 0.78, 1), 0.10, 0.10, 0.0, (0.20, 0.26, 0.16, 1)),
        "lens_rib": mat("CIN_Pickup_GlassLights_4K__LensRibs", (0.55, 0.68, 0.73, 0.65), 0.0, 0.07, 0.35),
        "gasket": mat("CIN_Pickup_GlassLights_4K__LampGasket", (0.003, 0.004, 0.005, 1), 0.0, 0.67),
        "metal": mat("CIN_Pickup_Mechanical_4K__ExteriorMetal", (0.24, 0.27, 0.30, 1), 0.86, 0.23),
        "tire": mat("CIN_Pickup_Mechanical_4K__TireCarcass", (0.006, 0.007, 0.008, 1), 0.0, 0.73),
        "sidewall": mat("CIN_Pickup_Mechanical_4K__Sidewall", (0.012, 0.013, 0.014, 1), 0.0, 0.58),
        "tread": mat("CIN_Pickup_Mechanical_4K__Tread", (0.004, 0.005, 0.006, 1), 0.0, 0.84),
        "alloy": mat("CIN_Pickup_Mechanical_4K__MachinedAlloy", (0.31, 0.34, 0.37, 1), 0.94, 0.16),
        "alloy_dark": mat("CIN_Pickup_Mechanical_4K__DarkAlloy", (0.045, 0.052, 0.060, 1), 0.91, 0.23),
        "rotor": mat("CIN_Pickup_Mechanical_4K__BrakeRotor", (0.24, 0.25, 0.26, 1), 0.88, 0.27),
        "rotor_dark": mat("CIN_Pickup_Mechanical_4K__RotorDrill", (0.018, 0.020, 0.021, 1), 0.60, 0.40),
        "rotor_hat": mat("CIN_Pickup_Mechanical_4K__RotorHat", (0.07, 0.075, 0.08, 1), 0.85, 0.32),
        "caliper": mat("CIN_Pickup_Mechanical_4K__Caliper", (0.56, 0.018, 0.010, 1), 0.72, 0.20),
        "hardware": mat("CIN_Pickup_Mechanical_4K__Hardware", (0.41, 0.44, 0.46, 1), 0.90, 0.18),
        "lug": mat("CIN_Pickup_Mechanical_4K__LugHardware", (0.16, 0.17, 0.18, 1), 0.92, 0.16),
        "cast": mat("CIN_Pickup_Mechanical_4K__CastBlock", (0.12, 0.13, 0.14, 1), 0.72, 0.40),
        "valve": mat("CIN_Pickup_Mechanical_4K__ValveCover", (0.39, 0.025, 0.018, 1), 0.78, 0.21),
        "header": mat("CIN_Pickup_Mechanical_4K__Header", (0.31, 0.22, 0.15, 1), 0.84, 0.29),
        "intake": mat("CIN_Pickup_Mechanical_4K__IntakeManifold", (0.10, 0.11, 0.12, 1), 0.65, 0.32),
        "intake_pipe": mat("CIN_Pickup_Mechanical_4K__IntakePipe", (0.24, 0.25, 0.26, 1), 0.88, 0.19),
        "filter": mat("CIN_Pickup_Mechanical_4K__AirFilter", (0.50, 0.07, 0.04, 1), 0.15, 0.55),
        "fuel": mat("CIN_Pickup_Mechanical_4K__FuelRail", (0.48, 0.10, 0.025, 1), 0.82, 0.20),
        "injector": mat("CIN_Pickup_Mechanical_4K__Injector", (0.04, 0.22, 0.08, 1), 0.30, 0.35),
        "radiator": mat("CIN_Pickup_Mechanical_4K__Radiator", (0.08, 0.09, 0.10, 1), 0.82, 0.34),
        "radiator_fin": mat("CIN_Pickup_Mechanical_4K__RadiatorFin", (0.22, 0.23, 0.24, 1), 0.88, 0.30),
        "pulley": mat("CIN_Pickup_Mechanical_4K__Pulley", (0.025, 0.030, 0.035, 1), 0.80, 0.28),
        "belt": mat("CIN_Pickup_Mechanical_4K__AccessoryBelt", (0.006, 0.007, 0.008, 1), 0.0, 0.78),
        "battery": mat("CIN_Pickup_Mechanical_4K__Battery", (0.025, 0.035, 0.045, 1), 0.05, 0.50),
        "booster": mat("CIN_Pickup_Mechanical_4K__BrakeBooster", (0.04, 0.045, 0.05, 1), 0.68, 0.32),
        "hose": mat("CIN_Pickup_Mechanical_4K__Hose", (0.008, 0.010, 0.012, 1), 0.0, 0.66),
        "wire": mat("CIN_Pickup_Mechanical_4K__Wiring", (0.16, 0.014, 0.010, 1), 0.0, 0.58),
        "cable": mat("CIN_Pickup_Mechanical_4K__Cable", (0.05, 0.055, 0.06, 1), 0.75, 0.35),
        "handle": mat("CIN_Pickup_Mechanical_4K__Handle", (0.03, 0.035, 0.04, 1), 0.55, 0.33),
        "interior": mat("CIN_Pickup_Interior_4K__CharcoalInterior", (0.025, 0.030, 0.036, 1), 0.0, 0.69),
        "carpet": mat("CIN_Pickup_Interior_4K__Carpet", (0.014, 0.016, 0.019, 1), 0.0, 0.94),
        "dash": mat("CIN_Pickup_Interior_4K__Dashboard", (0.020, 0.023, 0.027, 1), 0.0, 0.64),
        "dash_pad": mat("CIN_Pickup_Interior_4K__DashPad", (0.012, 0.014, 0.017, 1), 0.0, 0.74),
        "seat": mat("CIN_Pickup_Interior_4K__SeatCloth", (0.045, 0.050, 0.058, 1), 0.0, 0.86),
        "bolster": mat("CIN_Pickup_Interior_4K__SeatBolster", (0.020, 0.023, 0.027, 1), 0.0, 0.72),
        "stitch": mat("CIN_Pickup_Interior_4K__Stitch", (0.38, 0.025, 0.018, 1), 0.0, 0.50),
        "belt_fabric": mat("CIN_Pickup_Interior_4K__Seatbelt", (0.018, 0.020, 0.023, 1), 0.0, 0.78),
        "cluster": mat("CIN_Pickup_Interior_4K__Cluster", (0.008, 0.010, 0.012, 1), 0.0, 0.55),
        "needle": mat("CIN_Pickup_Interior_4K__GaugeNeedle", (0.82, 0.018, 0.010, 1), 0.0, 0.28, 0.0, (0.30, 0.004, 0.002, 1)),
        "gauge_mark": mat("CIN_Pickup_Interior_4K__GaugeMarks", (0.70, 0.74, 0.76, 1), 0.0, 0.46),
        "steering": mat("CIN_Pickup_Interior_4K__SteeringWheel", (0.012, 0.014, 0.016, 1), 0.0, 0.62),
        "console": mat("CIN_Pickup_Interior_4K__Console", (0.018, 0.020, 0.024, 1), 0.0, 0.70),
        "vent": mat("CIN_Pickup_Interior_4K__Vent", (0.025, 0.028, 0.032, 1), 0.10, 0.55),
        "vent_dark": mat("CIN_Pickup_Interior_4K__VentDark", (0.005, 0.006, 0.008, 1), 0.0, 0.78),
        "pedal": mat("CIN_Pickup_Interior_4K__Pedals", (0.10, 0.11, 0.12, 1), 0.72, 0.36),
        "floor": mat("PREVIEW_Floor", (0.025, 0.030, 0.038, 1), 0.18, 0.26),
    }

    legacy_wheels = [obj for obj in bpy.data.objects if obj.name.startswith("Wheel_") and obj.parent == root]
    for obj in legacy_wheels:
        delete_tree(obj)
    remap_original_materials(root, materials)
    cut_engine_bay_opening()
    refine_body(root)
    add_cinematic_wheel("CIN_Wheel_FL", -1.095, 0.52, 1.66, root, materials, False)
    add_cinematic_wheel("CIN_Wheel_FR", 1.095, 0.52, 1.66, root, materials, False)
    add_cinematic_wheel("CIN_Wheel_RL", -1.095, 0.52, -1.56, root, materials, True)
    add_cinematic_wheel("CIN_Wheel_RR", 1.095, 0.52, -1.56, root, materials, True)
    add_lamp_detail(root, materials)
    add_engine_bay(root, materials)
    add_open_hood(root, materials)
    add_cockpit(root, materials)
    add_bed_detail(root, materials)
    convert_curves()
    ensure_uv_layers(root)

    mesh_count, vertex_count, triangle_count, dimensions = stats(root)
    root["mesh_count"] = mesh_count
    root["vertex_count"] = vertex_count
    root["triangle_count"] = triangle_count
    root["dimensions_m"] = [round(v, 4) for v in dimensions]
    print("CARVINO_PICKUP_CINEMATIC_STATS", f"meshes={mesh_count}", f"vertices={vertex_count}", f"triangles={triangle_count}", f"dimensions=({dimensions.x:.4f},{dimensions.y:.4f},{dimensions.z:.4f})")
    if triangle_count < 250000 or triangle_count > 320000:
        print("CARVINO_PICKUP_CINEMATIC_TARGET_WARNING target=250000-320000")

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
    add_showroom(materials)
    setup_render()
    bpy.ops.render.render(write_still=True)
    print("CARVINO_PICKUP_CINEMATIC_BLEND", BLEND_PATH)
    print("CARVINO_PICKUP_CINEMATIC_FBX", FBX_PATH)
    print("CARVINO_PICKUP_CINEMATIC_PREVIEW", PREVIEW_PATH)


if __name__ == "__main__":
    main()
