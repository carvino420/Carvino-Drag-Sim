# Shared system contracts

These contracts prevent specialist work from fighting over the same code.

## Engine telemetry (read-only outside Vehicle Engineering)

`EngineState` exposes:

- RPM, throttle, engine load, torque and horsepower
- boost/MAP, lambda, ignition and injector duty
- coolant and oil temperature, turbo speed and damage
- current gear, wheel speed and vehicle speed

Vehicle Engineering alone changes engine state. Audio, HUD, dyno graphs,
camera effects and AI may read it but must not write it.

## Driver tune request (owned by UI/Tuning)

The UI submits a `TuneRequest` containing target lambda, ignition trim, boost
targets by gear, launch RPM, shift RPM and fuel choice. Vehicle Engineering
validates it against the engine, turbo and fuel components. Invalid settings
produce warnings; they never silently rewrite an engine definition.

## Physics result (owned by Physics)

Physics receives engine wheel torque, vehicle geometry, surface definition and
tire configuration. It returns traction force, contact-patch load, slip ratio,
tire temperature, tire deflection and vehicle acceleration. Multiple tire
contact points belong here, not in the engine or UI.

## Asset contract (owned by Art/Content)

Every shipped external asset needs a source record with its license, author,
date acquired and allowed commercial use. No proprietary game, vehicle-brand
badges, or reference-game assets are copied into the project.

## Merge gates

A feature cannot be marked complete until it:

1. compiles with no errors;
2. leaves existing save data usable;
3. works with keyboard and controller navigation where player-facing;
4. completes one garage-to-race-to-results path;
5. has a short QA note listing what was tested.
