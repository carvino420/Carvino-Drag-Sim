using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carvino
{
    [Serializable]
    public sealed class TuneSettings
    {
        public float airFuelRatio = 12.8f;
        public float ignitionTiming = 18f;
        public float launchRpm = 4200f;
        public float shiftRpm = 6800f;
        public float frontTirePressurePsi = 29f;
        public float rearTirePressurePsi = 27f;
        public float boostGear1Psi = 7f;
        public float boostGear2Psi = 9f;
        public float boostGear3Psi = 11f;
        public float boostGear4Psi = 12f;
        public float boostGear5Psi = 12f;
        public FuelType fuelType = FuelType.PumpGas;
        public float frontRideHeightMm = 130f;
        public float rearRideHeightMm = 135f;
        public float frontRebound = 5f;
        public float rearRebound = 5f;
        public float antiSquat = 0.50f;

        public float Lambda => airFuelRatio / FuelCatalog.Get(fuelType).stoichAfr;

        public float BoostForGear(int gear)
        {
            switch (Mathf.Clamp(gear, 1, 5))
            {
                case 1: return boostGear1Psi;
                case 2: return boostGear2Psi;
                case 3: return boostGear3Psi;
                case 4: return boostGear4Psi;
                default: return boostGear5Psi;
            }
        }

        public void ChangeAllBoost(float amount, float maximum)
        {
            boostGear1Psi = Mathf.Clamp(boostGear1Psi + amount, 0f, maximum);
            boostGear2Psi = Mathf.Clamp(boostGear2Psi + amount, 0f, maximum);
            boostGear3Psi = Mathf.Clamp(boostGear3Psi + amount, 0f, maximum);
            boostGear4Psi = Mathf.Clamp(boostGear4Psi + amount, 0f, maximum);
            boostGear5Psi = Mathf.Clamp(boostGear5Psi + amount, 0f, maximum);
        }
    }

    [Serializable]
    public sealed class DragBuild
    {
        public VehicleSpec vehicle;
        public EngineSpec engine;
        public List<UpgradeSpec> upgrades = new List<UpgradeSpec>();
        public float finalDrive = 4.1f;
        public float launchRpm = 4200f;
        public float shiftRpm = 6800f;
        public bool engineIsNew;
        public float engineHealth;
        public TuneSettings tune = new TuneSettings();

        public float EngineHealthMultiplier => engineHealth > 0f ? engineHealth : (engineIsNew ? 1f : 0.93f);
        public int EngineCost => engineIsNew ? engine.price * 2 : engine.price;
        public UpgradeSpec Turbo
        {
            get
            {
                foreach (UpgradeSpec upgrade in upgrades)
                    if (upgrade.turboMaxBoostPsi > 0f) return upgrade;
                return null;
            }
        }
        public bool HasTurbo => Turbo != null;
        public float FuelCapacityMultiplier
        {
            get
            {
                float value = 1f;
                foreach (UpgradeSpec upgrade in upgrades) value *= upgrade.fuelCapacityMultiplier;
                return value;
            }
        }
        public float DrivetrainEfficiency
        {
            get
            {
                float value = .84f;
                foreach (UpgradeSpec upgrade in upgrades) value *= upgrade.drivetrainEfficiencyMultiplier;
                return Mathf.Clamp(value, .70f, .93f);
            }
        }

        /// <summary>Combined torque effect from installed hardware. Evaluated by the shared engine model.</summary>
        public float TorqueMultiplier
        {
            get
            {
                float value = 1f;
                foreach (UpgradeSpec upgrade in upgrades) value *= upgrade.torqueMultiplier;
                return Mathf.Clamp(value, .75f, 1.35f);
            }
        }

        /// <summary>
        /// Applies a torque-focused part most strongly around this engine's data-defined
        /// torque peak, while still retaining a small effect on either side of the curve.
        /// This is deliberately shared by dyno and race output.
        /// </summary>
        public float TorqueUpgradeMultiplierAtRpm(float rpm)
        {
            float normalizedRpm = Mathf.Clamp01(rpm / Mathf.Max(1f, engine.redlineRpm));
            float normalizedPeak = Mathf.Clamp(engine.peakTorqueRpm / Mathf.Max(1f, engine.redlineRpm), .25f, .88f);
            float rise = Mathf.InverseLerp(.12f, normalizedPeak, normalizedRpm);
            float fall = 1f - Mathf.InverseLerp(normalizedPeak, .98f, normalizedRpm) * .40f;
            float torqueBand = Mathf.Lerp(.35f, 1f, Mathf.Clamp01(rise * fall));
            return Mathf.Lerp(1f, TorqueMultiplier, torqueBand);
        }
        public float DrivenTractionMultiplier
        {
            get
            {
                float value = 1f;
                foreach (UpgradeSpec upgrade in upgrades) value *= upgrade.drivenTractionMultiplier;
                return value;
            }
        }
        public float ShiftCutSeconds
        {
            get
            {
                float value = .14f;
                foreach (UpgradeSpec upgrade in upgrades) value *= upgrade.shiftCutMultiplier;
                return Mathf.Clamp(value, .055f, .18f);
            }
        }

        public float MassKg
        {
            get
            {
                float value = vehicle.chassisMassKg + engine.massKg;
                foreach (UpgradeSpec upgrade in upgrades) value += upgrade.massDeltaKg;
                return Mathf.Max(500f, value);
            }
        }

        public float BaseHorsepower
        {
            get
            {
                float value = engine.peakHorsepower * EngineHealthMultiplier;
                foreach (UpgradeSpec upgrade in upgrades) value *= upgrade.powerMultiplier;
                FuelSpec fuel = FuelCatalog.Get(tune.fuelType);
                float fuelEfficiency = Mathf.Clamp(1f - Mathf.Abs(tune.Lambda - 0.87f) * 0.58f, 0.80f, 1.01f) * fuel.powerMultiplier;
                float timingEfficiency = Mathf.Clamp(1f + (tune.ignitionTiming - 18f) * 0.008f, 0.88f, 1.08f);
                value *= fuelEfficiency * timingEfficiency;
                return value;
            }
        }

        /// <summary>Advertised full-boost power used by garage and dyno previews.</summary>
        public float Horsepower => BaseHorsepower * BoostMultiplier(tune.BoostForGear(5), 1f);

        public float BoostMultiplier(float requestedBoostPsi, float spoolFraction)
        {
            if (!HasTurbo) return 1f;
            UpgradeSpec turbo = Turbo;
            float actualBoost = Mathf.Clamp(requestedBoostPsi, 0f, turbo.turboMaxBoostPsi) * Mathf.Clamp01(spoolFraction);
            float pressureRatio = (14.7f + actualBoost) / 14.7f;
            return 1f + (pressureRatio - 1f) * turbo.turboEfficiency;
        }

        public float Grip
        {
            get
            {
                float value = vehicle.tireGrip;
                foreach (UpgradeSpec upgrade in upgrades) value *= upgrade.gripMultiplier;
                return value;
            }
        }

        public TireCompoundSpec TireCompound
        {
            get
            {
                foreach (UpgradeSpec upgrade in upgrades)
                    if (!string.IsNullOrEmpty(upgrade.tireCompoundId))
                        return TireCompoundCatalog.Get(upgrade.tireCompoundId);
                return TireCompoundCatalog.StreetRadial;
            }
        }
    }

    public sealed class DragSimulation
    {
        public const float QuarterMileMeters = 402.336f;
        public const float EighthMileMeters = 201.168f;

        public float DistanceMeters { get; private set; }
        public float SpeedMps { get; private set; }
        public float ElapsedSeconds { get; private set; }
        public float SixtyFootSeconds { get; private set; } = -1f;
        public float ThreeThirtyFootSeconds { get; private set; } = -1f;
        public float EighthMileSeconds { get; private set; } = -1f;
        public float ThousandFootSeconds { get; private set; } = -1f;
        public float EighthMileMph { get; private set; }
        public float QuarterMileMph { get; private set; }
        public float FinishTrapMph { get; private set; }
        public float FinishDistanceMeters => finishDistanceMeters;
        public float TireTemperatureC { get; private set; } = 22f;
        public float TireWear { get; private set; }
        public float EffectiveTireGrip
        {
            get
            {
                int firstDrivenTire = build.vehicle.drivetrain == DrivetrainLayout.Fwd ? 0 : 2;
                float measuredGrip = 0f;
                int measuredPatches = 0;
                for (int tireIndex = firstDrivenTire; tireIndex < firstDrivenTire + 2; tireIndex++)
                {
                    foreach (TireContactPatch patch in tires[tireIndex].Patches)
                    {
                        if (patch.FrictionCoefficient <= 0f) continue;
                        measuredGrip += patch.FrictionCoefficient;
                        measuredPatches++;
                    }
                }
                if (measuredPatches > 0) return measuredGrip / measuredPatches * (1f - TireWear * .18f);

                float heatFactor = Mathf.Clamp01(1f - Mathf.Abs(TireTemperatureC - 58f) / 45f);
                float temperatureGrip = Mathf.Lerp(0.72f, 1.08f, heatFactor);
                return build.Grip * surface.gripMultiplier * temperatureGrip * (1f - TireWear * 0.18f);
            }
        }
        public IReadOnlyList<TireAssembly> Tires => tires;
        public EngineState State { get; } = new EngineState();
        public float EngineRpm => State.Rpm;
        public bool Finished => DistanceMeters >= finishDistanceMeters;
        public float TireRollingRadiusMeters => tireRollingRadiusMeters;
        public float CurrentOverallGearRatio { get; private set; }
        public float WheelTorqueLbFt { get; private set; }
        public float AvailableDriveForceNewtons { get; private set; }
        public float AppliedDriveForceNewtons { get; private set; }
        public float AeroDragNewtons { get; private set; }
        public float RollingResistanceNewtons { get; private set; }
        public float DrivenAxleLoadNewtons { get; private set; }
        public float LongitudinalAccelerationMps2 { get; private set; }

        private static readonly float[] GearRatios = { 3.25f, 2.12f, 1.52f, 1.18f, 0.95f };
        private readonly DragBuild build;
        private readonly TireAssembly[] tires;
        private readonly TrackSurfaceSpec surface;
        private readonly float finishDistanceMeters;
        private readonly float tireRollingRadiusMeters;
        private float previousAcceleration;
        private float shiftCutRemaining;

        public DragSimulation(DragBuild build, TrackSurfaceSpec surface = null, float finishDistanceMeters = QuarterMileMeters)
        {
            this.build = build;
            this.surface = surface ?? TrackSurfaceCatalog.PreppedStrip;
            this.finishDistanceMeters = Mathf.Clamp(finishDistanceMeters, EighthMileMeters, QuarterMileMeters);
            // Persistent component condition feeds the same authoritative failure
            // state used by racing, dyno output, AI, audio, and UI.
            State.BaselineDamage = Mathf.Clamp01(1f - build.EngineHealthMultiplier);
            State.Damage = State.BaselineDamage;
            float frontPressure = build.tune.frontTirePressurePsi;
            float rearPressure = build.tune.rearTirePressurePsi;
            float width = build.vehicle.drivetrain == DrivetrainLayout.Rwd ? 275f : 235f;
            TireCompoundSpec compound = build.TireCompound;
            // Until explicit wheel/tire dimensions move into VehicleSpec, use a
            // representative loaded radius for each starter chassis. Keeping this
            // value finite is essential: wheel force is wheel torque divided by
            // rolling radius, rather than engine power divided by an arbitrary speed.
            tireRollingRadiusMeters = build.vehicle.drivetrain == DrivetrainLayout.Rwd ? .335f : .305f;
            tires = new[]
            {
                new TireAssembly { corner = WheelCorner.FrontLeft, PressurePsi = frontPressure, WidthMm = width, Compound = compound },
                new TireAssembly { corner = WheelCorner.FrontRight, PressurePsi = frontPressure, WidthMm = width, Compound = compound },
                new TireAssembly { corner = WheelCorner.RearLeft, PressurePsi = rearPressure, WidthMm = width, Compound = compound },
                new TireAssembly { corner = WheelCorner.RearRight, PressurePsi = rearPressure, WidthMm = width, Compound = compound }
            };
        }

        public float HorsepowerAtRpm(float rpm)
        {
            float activeBoost = Mathf.Max(0f, State.ManifoldPressurePsi - 14.7f);
            return BaseHorsepowerAtRpm(rpm) * build.BoostMultiplier(activeBoost, 1f) * State.PowerDerate;
        }

        public float DynoHorsepowerAtRpm(float rpm)
        {
            return BaseHorsepowerAtRpm(rpm) * build.BoostMultiplier(build.tune.BoostForGear(5), 1f);
        }

        private float BaseHorsepowerAtRpm(float rpm)
        {
            float normalized = Mathf.Clamp01(rpm / build.engine.redlineRpm);
            float shape = Mathf.Clamp01(0.35f + normalized * 0.95f - normalized * normalized * 0.28f);
            return build.BaseHorsepower * shape * build.TorqueUpgradeMultiplierAtRpm(rpm);
        }

        public float DynoTorqueAtRpm(float rpm)
        {
            float hp = DynoHorsepowerAtRpm(rpm);
            return rpm < 1f ? 0f : hp * 5252f / rpm;
        }

        public float TorqueAtRpm(float rpm)
        {
            float hp = HorsepowerAtRpm(rpm);
            return rpm < 1f ? 0f : hp * 5252f / rpm;
        }

        public void Burnout(float deltaTime)
        {
            if (Finished) return;
            int firstDrivenTire = build.vehicle.drivetrain == DrivetrainLayout.Fwd ? 0 : 2;
            tires[firstDrivenTire].Burnout(deltaTime);
            tires[firstDrivenTire + 1].Burnout(deltaTime);
            TireTemperatureC = (tires[firstDrivenTire].AverageTemperatureC + tires[firstDrivenTire + 1].AverageTemperatureC) * 0.5f;
            TireWear = Mathf.Clamp01(TireWear + deltaTime * 0.0008f);
        }

        public void BeginShift(float duration) => shiftCutRemaining = Mathf.Max(shiftCutRemaining, duration);

        public void Step(float deltaTime, float throttle, int gear)
        {
            if (Finished) return;
            throttle = Mathf.Clamp01(throttle);
            float effectiveThrottle = shiftCutRemaining > 0f ? throttle * .14f : throttle;
            shiftCutRemaining = Mathf.Max(0f, shiftCutRemaining - deltaTime);
            ElapsedSeconds += deltaTime;

            CurrentOverallGearRatio = GearRatios[Mathf.Clamp(gear - 1, 0, GearRatios.Length - 1)] * build.finalDrive;
            float coupledRpm = SpeedMps / (2f * Mathf.PI * tireRollingRadiusMeters) * CurrentOverallGearRatio * 60f;
            float launchRpm = Mathf.Clamp(build.tune.launchRpm > 0f ? build.tune.launchRpm : build.launchRpm, 900f, build.engine.redlineRpm);
            float clutchControlledRpm = gear == 1 && effectiveThrottle > .05f && coupledRpm < launchRpm
                ? Mathf.Lerp(900f, launchRpm, effectiveThrottle)
                : coupledRpm;
            State.Rpm = Mathf.Max(900f, clutchControlledRpm);
            UpdateEngineState(deltaTime, effectiveThrottle, gear);
            float crankTorqueLbFt = TorqueAtRpm(State.Rpm);
            WheelTorqueLbFt = crankTorqueLbFt * CurrentOverallGearRatio * build.DrivetrainEfficiency * effectiveThrottle;
            const float poundFootToNewtonMeter = 1.35581795f;
            AvailableDriveForceNewtons = Mathf.Max(0f, WheelTorqueLbFt * poundFootToNewtonMeter / tireRollingRadiusMeters);
            float staticFrontBias = build.vehicle.drivetrain == DrivetrainLayout.Fwd ? 0.62f : 0.54f;
            float wheelbaseM = build.vehicle.drivetrain == DrivetrainLayout.Fwd ? 2.57f : 2.92f;
            float baseCenterOfGravityHeightM = build.vehicle.drivetrain == DrivetrainLayout.Fwd ? 0.48f : 0.56f;
            float averageRideHeight = (build.tune.frontRideHeightMm + build.tune.rearRideHeightMm) * 0.5f;
            float centerOfGravityHeightM = Mathf.Clamp(baseCenterOfGravityHeightM + (averageRideHeight - 132f) * .00075f, .40f, .68f);
            float reboundControl = build.vehicle.drivetrain == DrivetrainLayout.Fwd
                ? Mathf.Lerp(1.10f, .86f, Mathf.InverseLerp(1f, 10f, build.tune.frontRebound))
                : Mathf.Lerp(.90f, 1.08f, Mathf.InverseLerp(1f, 10f, build.tune.rearRebound));
            float antiSquatControl = build.vehicle.drivetrain == DrivetrainLayout.Rwd ? Mathf.Lerp(.90f, 1.13f, build.tune.antiSquat) : 1f;
            float weightTransfer = build.MassKg * previousAcceleration * centerOfGravityHeightM / wheelbaseM * reboundControl;
            float frontLoad = Mathf.Max(0f, build.MassKg * 9.81f * staticFrontBias - weightTransfer);
            float rearLoad = Mathf.Max(0f, build.MassKg * 9.81f * (1f - staticFrontBias) + weightTransfer);
            float requestedForce = AvailableDriveForceNewtons;
            float surfaceGrip = build.Grip * surface.gripMultiplier;
            float frontCapacity = tires[0].Update(frontLoad * 0.5f, build.vehicle.drivetrain == DrivetrainLayout.Fwd ? requestedForce * 0.5f : 0f, surfaceGrip, deltaTime)
                                  + tires[1].Update(frontLoad * 0.5f, build.vehicle.drivetrain == DrivetrainLayout.Fwd ? requestedForce * 0.5f : 0f, surfaceGrip, deltaTime);
            float rearCapacity = (tires[2].Update(rearLoad * 0.5f, build.vehicle.drivetrain == DrivetrainLayout.Rwd ? requestedForce * 0.5f : 0f, surfaceGrip, deltaTime)
                                 + tires[3].Update(rearLoad * 0.5f, build.vehicle.drivetrain == DrivetrainLayout.Rwd ? requestedForce * 0.5f : 0f, surfaceGrip, deltaTime)) * antiSquatControl;
            float tractionForce = (build.vehicle.drivetrain == DrivetrainLayout.Fwd ? frontCapacity : rearCapacity) * build.DrivenTractionMultiplier;
            float driveForce = Mathf.Min(tractionForce, requestedForce);
            AppliedDriveForceNewtons = driveForce;
            DrivenAxleLoadNewtons = build.vehicle.drivetrain == DrivetrainLayout.Fwd ? frontLoad : rearLoad;
            AeroDragNewtons = 0.5f * 1.225f * build.vehicle.dragCoefficient * build.vehicle.frontalAreaM2 * SpeedMps * SpeedMps;
            RollingResistanceNewtons = SpeedMps > .01f || driveForce > .01f
                ? build.MassKg * 9.81f * surface.rollingResistance
                : 0f;
            LongitudinalAccelerationMps2 = (driveForce - AeroDragNewtons - RollingResistanceNewtons) / build.MassKg;

            SpeedMps = Mathf.Max(0f, SpeedMps + LongitudinalAccelerationMps2 * deltaTime);
            previousAcceleration = LongitudinalAccelerationMps2;
            DistanceMeters += SpeedMps * deltaTime;
            int firstDrivenTire = build.vehicle.drivetrain == DrivetrainLayout.Fwd ? 0 : 2;
            TireTemperatureC = (tires[firstDrivenTire].AverageTemperatureC + tires[firstDrivenTire + 1].AverageTemperatureC) * 0.5f;
            TireWear = Mathf.Clamp01(TireWear + (tires[firstDrivenTire].Patches[1].SlipRatio + tires[firstDrivenTire + 1].Patches[1].SlipRatio) * deltaTime * 0.0004f);
            if (SixtyFootSeconds < 0f && DistanceMeters >= 18.288f) SixtyFootSeconds = ElapsedSeconds;
            if (ThreeThirtyFootSeconds < 0f && DistanceMeters >= 100.584f) ThreeThirtyFootSeconds = ElapsedSeconds;
            if (EighthMileSeconds < 0f && DistanceMeters >= 201.168f)
            {
                EighthMileSeconds = ElapsedSeconds;
                EighthMileMph = SpeedMps * 2.23694f;
            }
            if (ThousandFootSeconds < 0f && DistanceMeters >= 304.8f) ThousandFootSeconds = ElapsedSeconds;
            if (Finished)
            {
                FinishTrapMph = SpeedMps * 2.23694f;
                if (finishDistanceMeters >= QuarterMileMeters - .01f) QuarterMileMph = FinishTrapMph;
            }
        }

        private void UpdateEngineState(float deltaTime, float throttle, int gear)
        {
            State.IgnitionTimingDegrees = build.tune.ignitionTiming;
            FuelSpec fuel = FuelCatalog.Get(build.tune.fuelType);
            State.Lambda = build.tune.Lambda;
            float spool = build.HasTurbo ? Mathf.InverseLerp(build.Turbo.turboSpoolRpm * 0.72f, build.Turbo.turboSpoolRpm * 1.45f, State.Rpm) * throttle : 0f;
            float targetBoost = build.HasTurbo ? Mathf.Min(build.tune.BoostForGear(gear), build.Turbo.turboMaxBoostPsi) : 0f;
            float actualBoost = targetBoost * spool;
            State.ManifoldPressurePsi = 14.7f + actualBoost;
            State.TurboSpeedRpm = build.HasTurbo ? Mathf.Lerp(18000f, 156000f, Mathf.Clamp01(actualBoost / build.Turbo.turboMaxBoostPsi)) : 0f;
            State.InjectorDutyPercent = Mathf.Clamp01(throttle * (0.28f + State.Rpm / build.engine.redlineRpm * 0.62f) * (1f + actualBoost * 0.035f) * fuel.fuelVolumeMultiplier / build.FuelCapacityMultiplier) * 100f;

            float leanRisk = Mathf.InverseLerp(0.93f, 1.05f, State.Lambda);
            float timingRisk = Mathf.InverseLerp(24f, 35f, build.tune.ignitionTiming);
            float rpmRisk = Mathf.InverseLerp(build.engine.redlineRpm * 0.96f, build.engine.redlineRpm * 1.08f, State.Rpm);
            float boostRisk = build.HasTurbo ? Mathf.InverseLerp(build.Turbo.turboSafeBoostPsi, build.Turbo.turboMaxBoostPsi + 3f, actualBoost) : 0f;
            State.KnockIntensity = Mathf.Clamp01((leanRisk * 0.62f + timingRisk * 0.42f + rpmRisk * 0.35f + boostRisk * 0.5f) * fuel.knockRiskMultiplier * throttle);
            State.CoolantTempC = Mathf.Clamp(State.CoolantTempC + (throttle * State.Rpm / build.engine.redlineRpm * 3.3f - 0.42f) * deltaTime, 72f, 128f);
            State.OilTempC = Mathf.Clamp(State.OilTempC + (throttle * State.Rpm / build.engine.redlineRpm * 2.5f - 0.22f) * deltaTime, 68f, 142f);
            float heatRisk = Mathf.InverseLerp(108f, 125f, State.CoolantTempC);
            float fuelRisk = Mathf.InverseLerp(92f, 100f, State.InjectorDutyPercent);
            float turboRisk = build.HasTurbo ? Mathf.InverseLerp(145000f, 156000f, State.TurboSpeedRpm) : 0f;
            float overRevRisk = Mathf.InverseLerp(build.engine.redlineRpm, build.engine.redlineRpm * 1.12f, State.Rpm);
            float knockWear = State.KnockIntensity * 0.0045f * deltaTime;
            float heatWear = heatRisk * 0.0025f * deltaTime;
            float fuelWear = fuelRisk * 0.003f * deltaTime;
            float turboWear = turboRisk * 0.002f * deltaTime;
            float overRevWear = overRevRisk * 0.0035f * deltaTime;
            State.KnockWear += knockWear;
            State.HeatWear += heatWear;
            State.FuelWear += fuelWear;
            State.TurboWear += turboWear;
            State.OverRevWear += overRevWear;
            State.Damage = Mathf.Clamp01(State.Damage + knockWear + heatWear + fuelWear + turboWear + overRevWear);
            State.SafetyPowerMultiplier = 1f;
            State.Warning = "SYSTEMS NORMAL";
            if (State.KnockIntensity > 0.55f)
            {
                State.SafetyPowerMultiplier = Mathf.Min(State.SafetyPowerMultiplier, Mathf.Lerp(0.88f, 0.68f, Mathf.InverseLerp(0.55f, 1f, State.KnockIntensity)));
                State.Warning = "KNOCK DETECTED — TIMING PULL";
            }
            if (fuelRisk > 0.1f)
            {
                State.SafetyPowerMultiplier = Mathf.Min(State.SafetyPowerMultiplier, Mathf.Lerp(0.86f, 0.62f, fuelRisk));
                State.Warning = "FUEL LIMIT — LEAN PROTECTION";
            }
            if (turboRisk > 0.1f)
            {
                State.SafetyPowerMultiplier = Mathf.Min(State.SafetyPowerMultiplier, Mathf.Lerp(0.9f, 0.7f, turboRisk));
                State.Warning = "TURBO OVERSPEED — BOOST CUT";
            }
            if (heatRisk > 0.1f)
            {
                State.SafetyPowerMultiplier = Mathf.Min(State.SafetyPowerMultiplier, Mathf.Lerp(0.88f, 0.58f, heatRisk));
                State.Warning = "OVERHEAT — POWER REDUCTION";
            }
            if (overRevRisk > 0.1f)
            {
                State.SafetyPowerMultiplier = Mathf.Min(State.SafetyPowerMultiplier, Mathf.Lerp(0.90f, 0.52f, overRevRisk));
                State.Warning = "OVER-REV — VALVETRAIN RISK";
            }
            if (State.Damage > 0.55f && State.Warning == "SYSTEMS NORMAL") State.Warning = "MISFIRE — ENGINE DAMAGE";
            State.IsFailed = State.Damage >= 0.9f;
            if (State.IsFailed) State.Warning = "ENGINE FAILED — RETURN TO GARAGE";
            State.Horsepower = HorsepowerAtRpm(State.Rpm);
            State.TorqueLbFt = TorqueAtRpm(State.Rpm);
        }
    }
}
