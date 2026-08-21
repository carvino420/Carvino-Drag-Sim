using UnityEngine;

namespace Carvino
{
    /// <summary>Lightweight wheel-smoke effect driven by the race controller's burnout state.</summary>
    [RequireComponent(typeof(ParticleSystem))]
    public sealed class BurnoutVisualEffects : MonoBehaviour
    {
        [SerializeField] private PrototypeRaceController raceController;
        private ParticleSystem smoke;
        private ParticleSystem.EmissionModule emission;

        private void Awake()
        {
            smoke = GetComponent<ParticleSystem>();
            emission = smoke.emission;
            emission.rateOverTime = 0f;
        }

        public void SetRaceController(PrototypeRaceController controller) => raceController = controller;

        private void Update()
        {
            bool activeBurnout = raceController != null && raceController.IsBurningOut && transform.root.gameObject.activeInHierarchy;
            emission.rateOverTime = activeBurnout ? 70f : 0f;
        }
    }
}
