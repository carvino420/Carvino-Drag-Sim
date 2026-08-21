using UnityEngine;

namespace Carvino
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class EngineAudioSynth : MonoBehaviour
    {
        private AudioSource source;
        private AudioSource transientSource;
        private AudioClip shiftCutClip;
        private float idleRpm = 900f;
        private float redlineRpm = 6500f;
        private float lastRpm;
        private float nextShiftSoundTime;
        private bool wasRacing;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0.18f * SettingsController.EngineVolume;

            // Keep the shift transient separate from the continuous engine loop so it
            // communicates a gear change without altering the simulation or HUD flow.
            transientSource = gameObject.AddComponent<AudioSource>();
            transientSource.spatialBlend = 0f;
            transientSource.playOnAwake = false;
            transientSource.loop = false;
            shiftCutClip = BuildShiftCut();
        }

        public void Configure(EngineSpec engine)
        {
            redlineRpm = engine.redlineRpm;
            bool fourCylinder = engine.id == "d16" || engine.id == "b20" || engine.id == "k20" || engine.id == "k24";
            bool sixCylinder = engine.id == "v6_43" || engine.id == "i6_42";
            float baseFrequency = fourCylinder ? 115f : sixCylinder ? 92f : 72f;
            source.clip = BuildLoop(baseFrequency, fourCylinder ? 0.46f : sixCylinder ? 0.6f : 0.78f);
            if (!source.isPlaying) source.Play();
        }

        public void SetEngineState(float rpm, float throttle, bool racing)
        {
            if (source.clip == null) return;
            float normalizedRpm = Mathf.InverseLerp(idleRpm, redlineRpm, rpm);
            float targetPitch = Mathf.Lerp(0.72f, 2.05f, normalizedRpm);
            float targetVolume = (racing ? Mathf.Lerp(0.09f, 0.32f, Mathf.Max(normalizedRpm, throttle)) : 0.07f) * SettingsController.EngineVolume;
            float smoothing = 1f - Mathf.Exp(-12f * Time.unscaledDeltaTime);
            source.pitch = Mathf.Lerp(source.pitch, targetPitch, smoothing);
            source.volume = Mathf.Lerp(source.volume, targetVolume, smoothing);

            // A fast RPM fall under throttle is produced by the existing shared
            // simulation's shift cut.  Turning that state change into a brief clutch/
            // exhaust chirp gives the driver a useful, non-visual shift confirmation.
            float shiftThreshold = Mathf.Max(280f, redlineRpm * 0.045f);
            bool shifted = racing && wasRacing && throttle > 0.45f && lastRpm - rpm > shiftThreshold;
            if (shifted && Time.unscaledTime >= nextShiftSoundTime)
            {
                float intensity = Mathf.InverseLerp(redlineRpm * 0.55f, redlineRpm, lastRpm);
                PlayShiftCut(intensity);
                nextShiftSoundTime = Time.unscaledTime + 0.11f;
            }

            lastRpm = rpm;
            wasRacing = racing;
        }

        private void PlayShiftCut(float intensity)
        {
            if (transientSource == null || shiftCutClip == null) return;
            transientSource.pitch = Mathf.Lerp(0.88f, 1.12f, intensity);
            transientSource.volume = Mathf.Lerp(0.08f, 0.20f, intensity) * SettingsController.EngineVolume;
            transientSource.PlayOneShot(shiftCutClip);
        }

        private static AudioClip BuildLoop(float fundamental, float growl)
        {
            const int sampleRate = 44100;
            const float duration = 1.5f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];
            for (int index = 0; index < samples; index++)
            {
                float time = index / (float)sampleRate;
                float signal = Mathf.Sin(time * fundamental * Mathf.PI * 2f);
                signal += Mathf.Sin(time * fundamental * 2f * Mathf.PI * 2f) * growl;
                signal += Mathf.Sin(time * fundamental * 3f * Mathf.PI * 2f) * 0.24f;
                signal += Mathf.Sin(time * fundamental * 0.5f * Mathf.PI * 2f) * (growl * 0.13f);
                data[index] = Mathf.Clamp(signal * 0.26f, -1f, 1f);
            }
            AudioClip clip = AudioClip.Create("Carvino Procedural Engine", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip BuildShiftCut()
        {
            const int sampleRate = 44100;
            const float duration = 0.075f;
            int samples = Mathf.RoundToInt(sampleRate * duration);
            float[] data = new float[samples];
            for (int index = 0; index < samples; index++)
            {
                float progress = index / (float)(samples - 1);
                float envelope = Mathf.Pow(1f - progress, 2.2f);
                float time = index / (float)sampleRate;
                float click = Mathf.Sin(time * 980f * Mathf.PI * 2f) * 0.62f;
                float bark = Mathf.Sin(time * 340f * Mathf.PI * 2f) * 0.38f;
                data[index] = (click + bark) * envelope * 0.30f;
            }

            AudioClip clip = AudioClip.Create("Carvino Procedural Shift Cut", samples, 1, sampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
