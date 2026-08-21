using UnityEngine;

namespace Carvino
{
    /// <summary>Simple garage turntable and engine-bay display for the selected vehicle.</summary>
    public sealed class GarageInspectionController : MonoBehaviour
    {
        [SerializeField] private Transform hatch;
        [SerializeField] private Transform pickup;
        [SerializeField] private GameObject engineBayDisplay;
        private int vehicleIndex;
        private bool inspectionOpen;

        public bool InspectionOpen => inspectionOpen;

        private void Start()
        {
            if (engineBayDisplay != null) engineBayDisplay.SetActive(false);
        }

        public void SetVehicle(int index)
        {
            vehicleIndex = index;
            if (hatch != null) hatch.gameObject.SetActive(index == 0);
            if (pickup != null) pickup.gameObject.SetActive(index == 1);
        }

        public void Rotate(float degrees)
        {
            Transform selected = vehicleIndex == 0 ? hatch : pickup;
            if (selected != null) selected.Rotate(Vector3.up, degrees, Space.World);
        }

        public void ToggleInspection()
        {
            inspectionOpen = !inspectionOpen;
            if (engineBayDisplay != null) engineBayDisplay.SetActive(inspectionOpen);
        }
    }
}
