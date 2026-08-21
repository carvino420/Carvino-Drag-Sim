#if UNITY_EDITOR
using System;
using System.IO;
using UnityEngine;

namespace Carvino.Editor
{
    /// <summary>Fast pre-build smoke tests for the playable prototype.</summary>
    public static class CarvinoBuildValidation
    {
        public static void ValidateOrThrow()
        {
            Require(File.Exists("Assets/Carvino/Scenes/MainMenu.unity"), "Main menu scene is missing.");
            Require(File.Exists("Assets/Carvino/Scenes/Controls.unity"), "Controls scene is missing.");
            Require(File.Exists("Assets/Carvino/Scenes/Garage.unity"), "Garage scene is missing.");
            Require(File.Exists("Assets/Carvino/Scenes/Dyno.unity"), "Dyno scene is missing.");
            Require(File.Exists("Assets/Carvino/Scenes/RaceDay.unity"), "Race Day scene is missing.");
            Require(File.Exists("Assets/Carvino/Scenes/QuarterMilePrototype.unity"), "Race scene is missing.");
            Require(CarvinoCatalog.Vehicles.Count == 2, "Alpha must contain exactly two starter vehicles.");
            Require(RaceEventSession.Events.Length >= 6, "Race Day needs the six-event early career ladder.");
            Require(TrackSurfaceCatalog.PreppedStrip != null && !string.IsNullOrEmpty(RaceHistory.BuildKey(new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16") }, TrackSurfaceCatalog.PreppedStrip, RaceDistanceCatalog.QuarterMile)), "Race history needs data-driven build keys.");
            foreach (RaceEvent raceEvent in RaceEventSession.Events)
            {
                Require(raceEvent.opponent != null && !string.IsNullOrEmpty(raceEvent.opponent.engineId), raceEvent.name + " needs an AI driver profile.");
                Require(CarvinoCatalog.FindEngine(raceEvent.opponent.engineId).id == raceEvent.opponent.engineId, raceEvent.name + " AI uses an unknown engine.");
                Require(TrackSurfaceCatalog.Get(raceEvent.careerSurface) != null, raceEvent.name + " needs a valid career surface.");
            }
            Require(TunePresets.All.Length >= 3 && TunePresets.All[0].price == 0, "Dyno needs a free safe base tune and purchasable tune presets.");

            foreach (VehicleSpec vehicle in CarvinoCatalog.Vehicles)
            {
                Require(vehicle.compatibleEngineIds.Count > 0, vehicle.displayName + " has no compatible engines.");
                foreach (string engineId in vehicle.compatibleEngineIds)
                    Require(CarvinoCatalog.FindEngine(engineId).id == engineId, vehicle.displayName + " references an unknown engine: " + engineId);
            }
            Require(CarvinoCatalog.FindEngine("k24").id == "k24", "Hatch K24 engine swap is missing.");
            Require(CarvinoCatalog.FindEngine("i6_42").id == "i6_42", "Pickup inline-six engine swap is missing.");
            Require(CarvinoCatalog.FindEngine("big_block_74").id == "big_block_74", "Pickup big-block engine swap is missing.");

            DragBuild newBuild = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16"), engineIsNew = true };
            DragBuild usedBuild = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16"), engineIsNew = false };
            Require(newBuild.Horsepower > usedBuild.Horsepower, "New-engine output must exceed used-engine output.");
            Require(newBuild.EngineCost == usedBuild.EngineCost * 2, "New engines must cost twice the used-engine price.");
            Require(CarvinoCatalog.Upgrades[5].price == 1700, "Street turbo kit price must match the starter economy.");
            Require(CarvinoCatalog.Upgrades.Count >= 12, "Fuel and drivetrain upgrades are missing.");
            FuelSpec e85 = FuelCatalog.Get(FuelType.E85);
            Require(e85.fuelVolumeMultiplier > 1f && e85.knockRiskMultiplier < 1f, "E85 must require more fuel volume and improve knock tolerance.");
            float healthyPower = newBuild.Horsepower;
            newBuild.engineHealth = 0.75f;
            Require(newBuild.Horsepower < healthyPower, "Engine condition must reduce power.");
            newBuild.engineHealth = 1f;

            float stockPower = newBuild.Horsepower;
            newBuild.upgrades.Add(CarvinoCatalog.Upgrades[0]);
            Require(newBuild.Horsepower > stockPower, "Intake upgrade must increase power.");

            DragBuild torqueCurveStock = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16"), engineIsNew = true };
            DragBuild torqueCurveUpgraded = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16"), engineIsNew = true };
            torqueCurveUpgraded.upgrades.Add(CarvinoCatalog.Upgrades[0]);
            DragSimulation torqueCurveStockDyno = new DragSimulation(torqueCurveStock);
            DragSimulation torqueCurveUpgradeDyno = new DragSimulation(torqueCurveUpgraded);
            float peakTorqueRpm = torqueCurveUpgraded.engine.peakTorqueRpm;
            Require(torqueCurveUpgradeDyno.DynoTorqueAtRpm(peakTorqueRpm) > torqueCurveStockDyno.DynoTorqueAtRpm(peakTorqueRpm), "Torque-focused upgrades must affect the shared engine curve.");
            Require(torqueCurveUpgraded.TorqueUpgradeMultiplierAtRpm(peakTorqueRpm) > torqueCurveUpgraded.TorqueUpgradeMultiplierAtRpm(torqueCurveUpgraded.engine.redlineRpm), "Torque upgrade effect must peak at the engine data torque RPM.");

            DragBuild fuelCapacityBuild = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("k20"), engineIsNew = true };
            DragSimulation stockFuelCheck = new DragSimulation(fuelCapacityBuild);
            for (int frame = 0; frame < 60; frame++) stockFuelCheck.Step(0.02f, 1f, 1);
            fuelCapacityBuild.upgrades.Add(CarvinoCatalog.Upgrades[6]);
            fuelCapacityBuild.upgrades.Add(CarvinoCatalog.Upgrades[7]);
            DragSimulation upgradedFuelCheck = new DragSimulation(fuelCapacityBuild);
            for (int frame = 0; frame < 60; frame++) upgradedFuelCheck.Step(0.02f, 1f, 1);
            Require(upgradedFuelCheck.State.InjectorDutyPercent < stockFuelCheck.State.InjectorDutyPercent, "Injectors and fuel pump must increase fuel capacity.");

            DragBuild drivetrainBuild = new DragBuild { vehicle = CarvinoCatalog.Vehicles[1], engine = CarvinoCatalog.FindEngine("ls_53"), engineIsNew = true };
            float stockDrivetrainEfficiency = drivetrainBuild.DrivetrainEfficiency;
            drivetrainBuild.upgrades.Add(CarvinoCatalog.Upgrades[8]);
            drivetrainBuild.upgrades.Add(CarvinoCatalog.Upgrades[9]);
            Require(drivetrainBuild.DrivetrainEfficiency > stockDrivetrainEfficiency && drivetrainBuild.ShiftCutSeconds < .14f, "Clutch and gearset must improve drivetrain efficiency and shift recovery.");

            DragBuild turboBuild = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16"), engineIsNew = true };
            float naturallyAspiratedPower = turboBuild.Horsepower;
            turboBuild.upgrades.Add(CarvinoCatalog.Upgrades[5]);
            Require(turboBuild.HasTurbo && turboBuild.Horsepower > naturallyAspiratedPower, "Turbo kit must add boosted power.");
            Require(turboBuild.tune.BoostForGear(1) < turboBuild.tune.BoostForGear(5), "Boost-by-gear must ramp through the gears.");

            float baseTunePower = newBuild.Horsepower;
            newBuild.tune.airFuelRatio = 15.5f;
            Require(newBuild.Horsepower < baseTunePower, "A poor AFR must reduce power.");

            DragBuild warningBuild = new DragBuild
            {
                vehicle = CarvinoCatalog.Vehicles[0],
                engine = CarvinoCatalog.FindEngine("k20"),
                engineIsNew = true,
                tune = new TuneSettings { airFuelRatio = 15.8f, ignitionTiming = 35f, shiftRpm = 7900f }
            };
            DragSimulation warningCheck = new DragSimulation(warningBuild);
            for (int frame = 0; frame < 300; frame++) warningCheck.Step(0.02f, 1f, 1);
            Require(warningCheck.State.Warning != "SYSTEMS NORMAL", "Unsafe tuning must produce a progressive engine warning.");

            EngineCondition componentCondition = new EngineCondition();
            componentCondition.ApplyWear(new EngineWearReport
            {
                RunDamage = .65f,
                KnockWear = .8f,
                HeatWear = .1f,
                FuelWear = .1f,
                DominantCause = "DETONATION / KNOCK"
            });
            Require(componentCondition.rings < componentCondition.valvetrain, "Knock damage must hurt rings more than the valvetrain according to component data.");
            Require(componentCondition.lastDamageCause == "DETONATION / KNOCK", "Component condition must retain the last diagnosed damage cause.");
            float damagedCondition = componentCondition.OverallHealth;
            componentCondition.RepairTo(.98f);
            Require(componentCondition.OverallHealth > damagedCondition && componentCondition.WeakestComponent.Length > 0, "Component repair must restore aggregate engine condition.");

            DragSimulation tireCheck = new DragSimulation(newBuild);
            Require(tireCheck.Tires.Count == 4 && tireCheck.Tires[0].Patches.Length == 3, "Tire model must expose four tires with three contact patches each.");
            float coldGrip = tireCheck.EffectiveTireGrip;
            tireCheck.Burnout(5f);
            Require(tireCheck.TireTemperatureC > 22f && tireCheck.EffectiveTireGrip > coldGrip, "Burnout heat must improve cold-tire grip.");

            DragSimulation preppedSurfaceCheck = new DragSimulation(newBuild, TrackSurfaceCatalog.PreppedStrip);
            DragSimulation streetSurfaceCheck = new DragSimulation(newBuild, TrackSurfaceCatalog.Street);
            Require(preppedSurfaceCheck.EffectiveTireGrip > streetSurfaceCheck.EffectiveTireGrip, "Prepped strip must provide more grip than street surface.");
            Require(TrackSurfaceCatalog.DampStreet.gripMultiplier < TrackSurfaceCatalog.Street.gripMultiplier, "Damp street must be the lowest-grip surface.");

            DragBuild chassisCheck = new DragBuild { vehicle = CarvinoCatalog.Vehicles[1], engine = CarvinoCatalog.FindEngine("ls_53"), engineIsNew = true };
            chassisCheck.tune.antiSquat = .2f;
            DragSimulation softChassis = new DragSimulation(chassisCheck);
            for (int frame = 0; frame < 30; frame++) softChassis.Step(.02f, 1f, 1);
            chassisCheck.tune.antiSquat = .9f;
            DragSimulation plantedChassis = new DragSimulation(chassisCheck);
            for (int frame = 0; frame < 30; frame++) plantedChassis.Step(.02f, 1f, 1);
            Require(plantedChassis.DistanceMeters > softChassis.DistanceMeters, "RWD anti-squat must affect launch acceleration.");

            DragBuild softPressureBuild = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16"), tune = new TuneSettings { frontTirePressurePsi = 18f, rearTirePressurePsi = 18f } };
            DragBuild hardPressureBuild = new DragBuild { vehicle = CarvinoCatalog.Vehicles[0], engine = CarvinoCatalog.FindEngine("d16"), tune = new TuneSettings { frontTirePressurePsi = 38f, rearTirePressurePsi = 38f } };
            DragSimulation softPressureCheck = new DragSimulation(softPressureBuild);
            DragSimulation hardPressureCheck = new DragSimulation(hardPressureBuild);
            softPressureCheck.Step(0.02f, 1f, 1);
            hardPressureCheck.Step(0.02f, 1f, 1);
            Require(softPressureCheck.Tires[0].AverageDeflectionMeters > hardPressureCheck.Tires[0].AverageDeflectionMeters, "Lower tire pressure must increase tire deflection.");

            DragBuild gearingBuild = new DragBuild
            {
                vehicle = CarvinoCatalog.Vehicles[0],
                engine = CarvinoCatalog.FindEngine("d16"),
                engineIsNew = true,
                finalDrive = 4.1f,
                tune = new TuneSettings { launchRpm = 4200f }
            };
            DragBuild tallFinalDriveBuild = new DragBuild
            {
                vehicle = CarvinoCatalog.Vehicles[0],
                engine = CarvinoCatalog.FindEngine("d16"),
                engineIsNew = true,
                finalDrive = 3.1f,
                tune = new TuneSettings { launchRpm = 4200f }
            };
            DragSimulation shortFinalDriveForce = new DragSimulation(gearingBuild);
            DragSimulation tallFinalDriveForce = new DragSimulation(tallFinalDriveBuild);
            shortFinalDriveForce.Step(.02f, 1f, 1);
            tallFinalDriveForce.Step(.02f, 1f, 1);
            Require(shortFinalDriveForce.CurrentOverallGearRatio > tallFinalDriveForce.CurrentOverallGearRatio, "A shorter final drive must expose a larger overall transmission ratio.");
            Require(shortFinalDriveForce.AvailableDriveForceNewtons > tallFinalDriveForce.AvailableDriveForceNewtons * 1.2f, "Gear and final-drive ratio must multiply wheel torque and available launch force.");
            Require(shortFinalDriveForce.WheelTorqueLbFt > 0f && shortFinalDriveForce.TireRollingRadiusMeters > .2f, "Driveline telemetry must expose wheel torque and a finite tire rolling radius.");

            DragSimulation coastCheck = new DragSimulation(gearingBuild);
            int coastGear = 1;
            for (int frame = 0; frame < 250; frame++)
            {
                if (coastCheck.EngineRpm > gearingBuild.tune.shiftRpm && coastGear < 5) coastGear++;
                coastCheck.Step(.02f, 1f, coastGear);
            }
            float coastStartSpeed = coastCheck.SpeedMps;
            for (int frame = 0; frame < 100; frame++) coastCheck.Step(.02f, 0f, coastGear);
            Require(coastStartSpeed > 3f && coastCheck.SpeedMps < coastStartSpeed, "Aerodynamic drag and rolling resistance must decelerate a coasting car.");
            Require(coastCheck.AeroDragNewtons > 0f && coastCheck.RollingResistanceNewtons > 0f, "Longitudinal force telemetry must expose aero and rolling resistance.");

            SimulateQuarterMile(new DragBuild
            {
                vehicle = CarvinoCatalog.Vehicles[1],
                engine = CarvinoCatalog.FindEngine("ls_53"),
                engineIsNew = true,
                tune = new TuneSettings { airFuelRatio = 12.8f, ignitionTiming = 18f, launchRpm = 3200f, shiftRpm = 5900f }
            });
            Debug.Log("Carvino pre-build validation passed.");
        }

        private static void SimulateQuarterMile(DragBuild build)
        {
            DragSimulation sim = new DragSimulation(build);
            int gear = 1;
            const float step = 0.02f;
            for (int frame = 0; frame < 3000 && !sim.Finished; frame++)
            {
                if (sim.EngineRpm > build.shiftRpm && gear < 5) gear++;
                sim.Step(step, 1f, gear);
            }
            Require(sim.Finished, "Quarter-mile smoke test did not finish.");
            Require(sim.SixtyFootSeconds > 0f && sim.ThreeThirtyFootSeconds > 0f && sim.EighthMileSeconds > 0f && sim.ThousandFootSeconds > 0f, "Time-slip intervals were not recorded.");
            Require(sim.QuarterMileMph > 10f, "Quarter-mile trap speed was not recorded.");
        }

        private static void Require(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("Carvino validation failed: " + message);
        }
    }
}
#endif
