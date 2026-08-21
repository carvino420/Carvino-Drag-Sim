using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    /// <summary>
    /// Small, original procedural interface sounds shared by all prototype scenes.
    /// The object is created at runtime so UI feedback does not require scene-by-scene wiring.
    /// </summary>
    public sealed class CarvinoUiAudio : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private static CarvinoUiAudio instance;
        private AudioSource source;
        private AudioClip navigateClip;
        private AudioClip confirmClip;
        private AudioClip backClip;
        private float lastAxis;
        private float nextClickTime;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void CreateRuntimeAudio()
        {
            if (instance != null) return;
            GameObject host = new GameObject("Carvino UI Audio");
            DontDestroyOnLoad(host);
            instance = host.AddComponent<CarvinoUiAudio>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.loop = false;

            // All clips are synthesized locally from simple waveforms.  No third-party
            // recordings or distributable sound assets are used by this helper.
            navigateClip = BuildTone("Carvino UI Navigate", 0.042f, 620f, 780f, 0.11f, 0.72f);
            confirmClip = BuildTone("Carvino UI Confirm", 0.075f, 410f, 1040f, 0.15f, 0.60f);
            backClip = BuildTone("Carvino UI Back", 0.055f, 540f, 270f, 0.11f, 0.68f);
        }

        private void Update()
        {
            if (Time.unscaledTime < nextClickTime) return;

            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow) ||
                Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.S) ||
                Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) ||
                Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.D) ||
                Input.GetKeyDown(KeyCode.JoystickButton4) || Input.GetKeyDown(KeyCode.JoystickButton5))
            {
                PlayNavigate();
                return;
            }

            float axis = Input.GetAxisRaw("Vertical");
            if ((axis >= 0.6f && lastAxis < 0.6f) || (axis <= -0.6f && lastAxis > -0.6f))
            {
                PlayNavigate();
                lastAxis = axis;
                return;
            }
            lastAxis = axis;

            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Space) ||
                Input.GetKeyDown(KeyCode.JoystickButton0) || Input.GetKeyDown(KeyCode.JoystickButton7))
            {
                PlayConfirm();
                return;
            }

            if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton6))
            {
                PlayBack();
                return;
            }

            // IMGUI buttons are used throughout the current prototype.  A mouse release
            // is the only shared hook that catches their confirmation without duplicating
            // audio calls in every controller.
            if (Input.GetMouseButtonUp(0)) PlayConfirm();
        }

        public static void PlayNavigate() => instance?.Play(navigateClipFor: UiCue.Navigate);
        public static void PlayConfirm() => instance?.Play(navigateClipFor: UiCue.Confirm);
        public static void PlayBack() => instance?.Play(navigateClipFor: UiCue.Back);

        private void Play(UiCue navigateClipFor)
        {
            if (source == null) return;
            AudioClip clip = navigateClipFor == UiCue.Navigate ? navigateClip : navigateClipFor == UiCue.Confirm ? confirmClip : backClip;
            if (clip == null) return;

            float baseVolume = navigateClipFor == UiCue.Confirm ? 0.34f : 0.25f;
            source.PlayOneShot(clip, baseVolume * Mathf.Clamp01(SettingsController.EngineVolume));
            nextClickTime = Time.unscaledTime + 0.035f;
        }

        private static AudioClip BuildTone(string clipName, float duration, float startFrequency, float endFrequency, float amplitude, float decay)
        {
            int sampleCount = Mathf.Max(1, Mathf.RoundToInt(SampleRate * duration));
            float[] samples = new float[sampleCount];
            float phase = 0f;
            for (int index = 0; index < sampleCount; index++)
            {
                float progress = index / (float)(sampleCount - 1);
                float frequency = Mathf.Lerp(startFrequency, endFrequency, progress);
                phase += frequency / SampleRate;
                float envelope = Mathf.Pow(1f - progress, decay);
                float fundamental = Mathf.Sin(phase * Mathf.PI * 2f);
                float harmonic = Mathf.Sin(phase * Mathf.PI * 4f) * 0.18f;
                samples[index] = (fundamental + harmonic) * envelope * amplitude;
            }

            AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private enum UiCue
        {
            Navigate,
            Confirm,
            Back
        }
    }
}
