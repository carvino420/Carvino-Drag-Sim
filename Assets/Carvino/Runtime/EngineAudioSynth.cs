using UnityEngine;

namespace Carvino
{
    [RequireComponent(typeof(AudioSource))]
    public sealed class EngineAudioSynth : MonoBehaviour
    {
        private AudioSource source;
        private float idleRpm = 900f;
        private float redlineRpm = 6500f;

        private void Awake()
        {
            source = GetComponent<AudioSource>();
            source.loop = true;
            source.spatialBlend = 0f;
            source.volume = 0.18f * SettingsController.EngineVolume;
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
            source.pitch = Mathf.Lerp(0.72f, 2.05f, normalizedRpm);
            source.volume = (racing ? Mathf.Lerp(0.09f, 0.32f, Mathf.Max(normalizedRpm, throttle)) : 0.07f) * SettingsController.EngineVolume;
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
    }
}
