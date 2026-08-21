using UnityEngine;
using UnityEngine.SceneManagement;

namespace Carvino
{
    public sealed class GarageController : MonoBehaviour
    {
        private int vehicleIndex;
        private int engineIndex;
        private bool upgrades;
        private bool engineIsNew;
        private int upgradeMask;
        private int partsPage;
        private bool showHistory;
        private bool showAppearance;
        private bool showBuildSheet;
        private bool showEngineHealth;
        private bool showControls;
        [SerializeField] private Transform hatchDisplay;
        [SerializeField] private Transform pickupDisplay;
        [SerializeField] private Renderer engineBlock;
        [SerializeField] private GameObject intakeVisual;
        [SerializeField] private GameObject exhaustVisual;
        [SerializeField] private GameObject ecuVisual;
        [SerializeField] private GameObject slickVisual;
        [SerializeField] private GameObject weightReductionVisual;
        [SerializeField] private GameObject turboVisual;
        [SerializeField] private GarageInspectionController inspectionController;
        private string statusMessage = "Pick a car, build it, then race.";
        private static readonly string[] PaintNames = { "RACE RED", "MIDNIGHT BLUE", "CHAMPIONSHIP WHITE", "GUNMETAL", "V-TEC TEAL" };
        private static readonly Color[] PaintColors =
        {
            new Color(.72f, .035f, .018f), new Color(.018f, .08f, .28f), new Color(.82f, .84f, .86f), new Color(.16f, .18f, .22f), new Color(.01f, .50f, .46f)
        };
        private static readonly string[] WheelNames = { "MACHINED ALLOY", "GLOSS BLACK", "BRONZE", "WHITE" };
        private static readonly Color[] WheelColors =
        {
            new Color(.48f, .52f, .56f), new Color(.025f, .028f, .033f), new Color(.47f, .25f, .08f), new Color(.78f, .80f, .82f)
        };

        private VehicleSpec Vehicle => CarvinoCatalog.Vehicles[vehicleIndex];
        private EngineSpec Engine => CarvinoCatalog.FindEngine(Vehicle.compatibleEngineIds[engineIndex]);

        private void Start()
        {
            GarageSession.Load();
            vehicleIndex = GarageSession.VehicleId == "pickup" ? 1 : 0;
            engineIndex = Mathf.Max(0, Vehicle.compatibleEngineIds.IndexOf(GarageSession.EngineId));
            upgradeMask = GarageSession.UpgradeMask;
            upgrades = upgradeMask != 0;
            engineIsNew = GarageSession.EngineIsNew;
            UpdateVehicleVisual();
            UpdateBuildVisuals();
            ApplyAppearance();
        }

        private void Update()
        {
            SelectVehicleFromGarageClick();
            if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.JoystickButton4)) ChangeVehicle(-1);
            if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.JoystickButton5)) ChangeVehicle(1);
            if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.JoystickButton3)) ChangeEngine(1);
            if (Input.GetKeyDown(KeyCode.DownArrow)) ChangeEngine(-1);
            if (Input.GetKeyDown(KeyCode.N) || Input.GetKeyDown(KeyCode.JoystickButton1)) engineIsNew = !engineIsNew;
            if (Input.GetKeyDown(KeyCode.U) || Input.GetKeyDown(KeyCode.JoystickButton2))
            {
                ToggleStarterBundle();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) ToggleUpgrade(0);
            if (Input.GetKeyDown(KeyCode.Alpha2)) ToggleUpgrade(1);
            if (Input.GetKeyDown(KeyCode.Alpha3)) ToggleUpgrade(2);
            if (Input.GetKeyDown(KeyCode.Alpha4)) ToggleUpgrade(3);
            if (Input.GetKeyDown(KeyCode.Alpha5)) ToggleUpgrade(4);
            if (Input.GetKeyDown(KeyCode.Alpha6)) ToggleUpgrade(5);
            if (Input.GetKeyDown(KeyCode.Alpha7)) ToggleUpgrade(6);
            if (Input.GetKeyDown(KeyCode.Alpha8)) ToggleUpgrade(7);
            if (Input.GetKeyDown(KeyCode.P)) PurchaseSelectedEngine();
            if (Input.GetKeyDown(KeyCode.Q)) RotateVehicle(-20f);
            if (Input.GetKeyDown(KeyCode.E)) RotateVehicle(20f);
            if (Input.GetKeyDown(KeyCode.I)) ToggleInspection();
            if (Input.GetKeyDown(KeyCode.L)) showAppearance = !showAppearance;
            if (Input.GetKeyDown(KeyCode.B)) showBuildSheet = !showBuildSheet;
            if (Input.GetKeyDown(KeyCode.H)) showEngineHealth = !showEngineHealth;
            if (Input.GetKeyDown(KeyCode.K)) showControls = !showControls;
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.JoystickButton6)) OpenDyno();
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.JoystickButton0)) StartRace();
            if (Input.GetKeyDown(KeyCode.Escape)) SceneManager.LoadScene("MainMenu");
        }

        private void ChangeVehicle(int direction)
        {
            vehicleIndex = (vehicleIndex + direction + CarvinoCatalog.Vehicles.Count) % CarvinoCatalog.Vehicles.Count;
            engineIndex = 0;
            engineIsNew = false;
            UpdateVehicleVisual();
            UpdateBuildVisuals();
            ApplyAppearance();
        }

        private void SelectVehicle(int index)
        {
            if (index < 0 || index >= CarvinoCatalog.Vehicles.Count || index == vehicleIndex) return;
            vehicleIndex = index;
            engineIndex = 0;
            engineIsNew = false;
            statusMessage = "Vehicle selected. Choose an engine and build it.";
            UpdateVehicleVisual();
            UpdateBuildVisuals();
            ApplyAppearance();
        }

        private void SelectVehicleFromGarageClick()
        {
            if (!Input.GetMouseButtonDown(0) || Camera.main == null) return;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit, 100f)) return;
            string rootName = hit.collider.transform.root.name;
            if (rootName == "Garage Hatch") SelectVehicle(0);
            if (rootName == "Garage Pickup") SelectVehicle(1);
        }

        private void ChangeEngine(int direction)
        {
            engineIndex = (engineIndex + direction + Vehicle.compatibleEngineIds.Count) % Vehicle.compatibleEngineIds.Count;
            engineIsNew = false;
            UpdateBuildVisuals();
        }

        private void ToggleUpgrade(int index)
        {
            if (!GarageSession.OwnsPart(index) && !GarageSession.TryBuyPart(index))
            {
                statusMessage = "Not enough V-TECoins for that part.";
                return;
            }
            upgradeMask ^= 1 << index;
            upgrades = upgradeMask != 0;
            statusMessage = (upgradeMask & (1 << index)) != 0 ? "Part installed." : "Part removed — it stays in your inventory.";
            UpdateBuildVisuals();
        }

        private void ToggleStarterBundle()
        {
            if (upgradeMask == GarageSession.DefaultUpgradeMask)
            {
                upgradeMask = 0;
                upgrades = false;
                statusMessage = "Starter parts removed — they remain in your inventory.";
                UpdateBuildVisuals();
                return;
            }

            int missingCost = 0;
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
                if ((GarageSession.DefaultUpgradeMask & (1 << index)) != 0 && !GarageSession.OwnsPart(index)) missingCost += CarvinoCatalog.Upgrades[index].price;
            if (GarageSession.VteCoins < missingCost)
            {
                statusMessage = "Not enough V-TECoins for the starter bundle.";
                return;
            }
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
                if ((GarageSession.DefaultUpgradeMask & (1 << index)) != 0) GarageSession.TryBuyPart(index);
            upgradeMask = GarageSession.DefaultUpgradeMask;
            upgrades = true;
            statusMessage = "Starter bundle installed.";
            UpdateBuildVisuals();
        }

        private void PurchaseSelectedEngine()
        {
            if (GarageSession.TryBuyEngine(Engine, engineIsNew))
                statusMessage = GarageSession.OwnsEngine(Engine.id, engineIsNew) ? "Engine purchased / ready to install." : "Engine ready.";
            else
                statusMessage = "Not enough V-TECoins for this engine.";
        }

        private void RepairSelectedEngine()
        {
            if (!GarageSession.OwnsEngine(Engine.id, engineIsNew))
            {
                statusMessage = "Buy this engine before repairing it.";
                return;
            }
            statusMessage = GarageSession.TryRepairEngine(Engine, engineIsNew) ? "Engine repaired and ready for another pass." : "Not enough V-TECoins for that repair.";
        }

        private void UpdateVehicleVisual()
        {
            if (hatchDisplay != null) hatchDisplay.gameObject.SetActive(vehicleIndex == 0);
            if (pickupDisplay != null) pickupDisplay.gameObject.SetActive(vehicleIndex == 1);
            if (inspectionController != null) inspectionController.SetVehicle(vehicleIndex);
        }

        private void RotateVehicle(float degrees)
        {
            if (inspectionController != null) inspectionController.Rotate(degrees);
        }

        private void ToggleInspection()
        {
            if (inspectionController != null) inspectionController.ToggleInspection();
            statusMessage = inspectionController != null && inspectionController.InspectionOpen ? "Inspection bay open — rotate the vehicle with Q / E and review installed hardware." : "Inspection bay closed.";
        }

        private void UpdateBuildVisuals()
        {
            if (engineBlock != null)
            {
                bool fourCylinder = Engine.id == "d16" || Engine.id == "b20" || Engine.id == "k20" || Engine.id == "k24";
                engineBlock.material.color = fourCylinder ? new Color(0.3f, 0.38f, 0.42f) : Engine.id == "v6_43" ? new Color(0.42f, 0.32f, 0.2f) : new Color(0.5f, 0.14f, 0.08f);
                engineBlock.transform.localScale = fourCylinder ? new Vector3(1.6f, 0.7f, 0.8f) : Engine.id == "v6_43" ? new Vector3(1.8f, 0.85f, 1.1f) : Engine.id == "i6_42" ? new Vector3(2.45f, 0.72f, 0.82f) : Engine.id == "big_block_74" ? new Vector3(2.65f, 1.18f, 1.35f) : new Vector3(2.2f, 0.95f, 1.15f);
            }
            SetVisual(intakeVisual, 0);
            SetVisual(exhaustVisual, 1);
            SetVisual(ecuVisual, 2);
            SetVisual(slickVisual, 3);
            SetVisual(weightReductionVisual, 4);
            SetVisual(turboVisual, 5);
        }

        private void SetAppearance(int paintIndex, int wheelIndex)
        {
            GarageSession.SetAppearance(paintIndex, wheelIndex);
            ApplyAppearance();
            statusMessage = PaintNames[GarageSession.PaintIndex % PaintNames.Length] + " paint and " + WheelNames[GarageSession.WheelFinishIndex % WheelNames.Length] + " wheels applied.";
        }

        private void ApplyAppearance()
        {
            ApplyAppearanceTo(hatchDisplay);
            ApplyAppearanceTo(pickupDisplay);
        }

        private void ApplyAppearanceTo(Transform vehicle)
        {
            if (vehicle == null) return;
            Color paint = PaintColors[GarageSession.PaintIndex % PaintColors.Length];
            Color wheels = WheelColors[GarageSession.WheelFinishIndex % WheelColors.Length];
            foreach (Renderer renderer in vehicle.GetComponentsInChildren<Renderer>(true))
            {
                Material[] materials = renderer.materials;
                for (int index = 0; index < materials.Length; index++)
                {
                    string materialName = materials[index].name;
                    if (materialName.IndexOf("Carvino", System.StringComparison.OrdinalIgnoreCase) >= 0 || materialName.IndexOf("Metallic", System.StringComparison.OrdinalIgnoreCase) >= 0 || materialName.IndexOf("Midnight", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        materials[index].color = paint;
                    if (materialName.IndexOf("Machined", System.StringComparison.OrdinalIgnoreCase) >= 0 || renderer.name.IndexOf(" Rim", System.StringComparison.OrdinalIgnoreCase) >= 0)
                        materials[index].color = wheels;
                }
            }
        }

        private void SetVisual(GameObject visual, int upgradeIndex)
        {
            if (visual != null) visual.SetActive((upgradeMask & (1 << upgradeIndex)) != 0);
        }

        private void StartRace()
        {
            if (!GarageSession.OwnsEngine(Engine.id, engineIsNew))
            {
                statusMessage = "Press P to buy this engine before racing it.";
                return;
            }
            GarageSession.SetBuild(Vehicle.id, Engine.id, upgrades, engineIsNew, upgradeMask);
            SceneManager.LoadScene("RaceDay");
        }

        private void OpenDyno()
        {
            GarageSession.SetBuild(Vehicle.id, Engine.id, upgrades, engineIsNew, upgradeMask);
            SceneManager.LoadScene("Dyno");
        }

        private void OnGUI()
        {
            Matrix4x4 previousMatrix = CarvinoUi.Begin();
            GUI.Box(new Rect(16, 16, 750, 638), "CARVINO WORKS — STARTER GARAGE");
            GUI.Label(new Rect(36, 58, 320, 28), "YOUR RIDE");
            GUI.Label(new Rect(36, 90, 320, 32), Vehicle.displayName);
            GUI.Label(new Rect(36, 122, 320, 24), Vehicle.drivetrain == DrivetrainLayout.Fwd ? "Front-wheel drive compact" : "Rear-wheel drive compact pickup");
            GUI.Label(new Rect(36, 142, 320, 24), $"V-TECoins: {GarageSession.VteCoins:N0}");
            if (GUI.Button(new Rect(36, 172, 150, 34), "1993 HATCH")) SelectVehicle(0);
            if (GUI.Button(new Rect(196, 172, 150, 34), "1991 PICKUP")) SelectVehicle(1);
            GUI.Label(new Rect(36, 218, 320, 24), "ENGINE SWAP");
            GUI.Label(new Rect(36, 244, 320, 27), Engine.displayName);
            GUI.Label(new Rect(36, 270, 320, 22), $"{Engine.peakHorsepower:0} hp  •  {Engine.peakTorqueLbFt:0} lb-ft");
            if (GUI.Button(new Rect(36, 297, 52, 28), "<")) ChangeEngine(-1);
            if (GUI.Button(new Rect(94, 297, 110, 28), engineIsNew ? "NEW" : "USED")) { engineIsNew = !engineIsNew; statusMessage = engineIsNew ? "New engine: full health, double price." : "Used engine: 93% health, base price."; }
            if (GUI.Button(new Rect(210, 297, 52, 28), ">")) ChangeEngine(1);
            if (GUI.Button(new Rect(268, 297, 78, 28), "BUY")) PurchaseSelectedEngine();
            GUI.Label(new Rect(36, 328, 220, 20), $"Condition: {GarageSession.GetEngineHealth(Engine.id, engineIsNew) * 100f:0}%");
            if (GUI.Button(new Rect(260, 326, 86, 24), "REPAIR " + GarageSession.RepairCost(Engine, engineIsNew).ToString("N0"))) RepairSelectedEngine();

            DragBuild preview = new DragBuild { vehicle = Vehicle, engine = Engine, engineIsNew = engineIsNew, engineHealth = GarageSession.GetEngineHealth(Engine.id, engineIsNew) };
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
                if ((upgradeMask & (1 << index)) != 0) preview.upgrades.Add(CarvinoCatalog.Upgrades[index]);

            GUI.Box(new Rect(390, 58, 338, 230), "BUILD PREVIEW");
            float stockPower = Engine.peakHorsepower * preview.EngineHealthMultiplier;
            float powerDelta = preview.Horsepower - stockPower;
            GUI.Label(new Rect(414, 98, 290, 24), $"Power: {preview.Horsepower:0} hp  ({(powerDelta >= 0f ? "+" : "")}{powerDelta:0})");
            GUI.Label(new Rect(414, 126, 290, 24), $"Weight: {preview.MassKg:0} kg");
            GUI.Label(new Rect(414, 154, 290, 24), $"Grip rating: {preview.Grip:0.00}");
            GUI.Label(new Rect(414, 182, 290, 24), $"Engine: {(engineIsNew ? "NEW — 100% health" : "USED — 93% health")}");
            GUI.Label(new Rect(414, 206, 290, 24), $"Swap price: {preview.EngineCost:N0} VTC  •  {(GarageSession.OwnsEngine(Engine.id, engineIsNew) ? "OWNED" : "NOT OWNED")}");
            GUI.Label(new Rect(414, 238, 290, 24), $"Parts: {PartNameList()}");
            GUI.Label(new Rect(414, 218, 290, 20), inspectionController != null && inspectionController.InspectionOpen ? "GARAGE VIEW: ENGINE INSPECTION" : "GARAGE VIEW: VEHICLE TURNTABLE");
            GUI.Label(new Rect(414, 262, 180, 20), "PARTS INVENTORY");
            int pageCount = Mathf.CeilToInt(CarvinoCatalog.Upgrades.Count / 6f);
            if (GUI.Button(new Rect(594, 258, 28, 22), "<")) partsPage = (partsPage + pageCount - 1) % pageCount;
            GUI.Label(new Rect(626, 262, 34, 20), (partsPage + 1) + "/" + pageCount);
            if (GUI.Button(new Rect(664, 258, 28, 22), ">")) partsPage = (partsPage + 1) % pageCount;
            for (int localIndex = 0; localIndex < 6; localIndex++)
            {
                int index = partsPage * 6 + localIndex;
                if (index >= CarvinoCatalog.Upgrades.Count) break;
                float x = localIndex % 2 == 0 ? 390f : 558f;
                float y = 288f + (localIndex / 2) * 38f;
                DrawPartButton(index, new Rect(x, y, 160, 31));
            }

            if (GUI.Button(new Rect(36, 360, 98, 38), "BUNDLE")) ToggleStarterBundle();
            if (GUI.Button(new Rect(142, 360, 98, 38), "INSPECT [I]")) ToggleInspection();
            if (GUI.Button(new Rect(248, 360, 98, 38), "CUSTOM [L]")) showAppearance = !showAppearance;
            if (GUI.Button(new Rect(36, 406, 150, 48), "DYNO & TUNE")) OpenDyno();
            if (GUI.Button(new Rect(196, 406, 150, 48), "GO RACE")) StartRace();
            if (GUI.Button(new Rect(36, 462, 310, 36), "BACK TO TITLE")) SceneManager.LoadScene("MainMenu");
            if (GUI.Button(new Rect(36, 506, 98, 34), "HISTORY")) showHistory = !showHistory;
            if (GUI.Button(new Rect(142, 506, 106, 34), "HEALTH [H]")) showEngineHealth = !showEngineHealth;
            if (GUI.Button(new Rect(256, 506, 90, 34), "SHEET [B]")) showBuildSheet = !showBuildSheet;
            if (GUI.Button(new Rect(36, 610, 122, 28), "CONTROLS [K]")) showControls = !showControls;
            GUI.Box(new Rect(36, 554, 692, 44), statusMessage);
            GUI.Label(new Rect(170, 614, 550, 20), "1–8 parts  •  Q / E rotate  •  I inspect  •  H health  •  B sheet  •  K controls");
            if (showHistory) DrawHistory(preview);
            if (showAppearance) DrawAppearance();
            if (showBuildSheet) DrawBuildSheet(preview);
            if (showEngineHealth) DrawEngineHealth();
            if (showControls) DrawGarageControls();
            CarvinoUi.End(previousMatrix);
        }

        private void DrawGarageControls()
        {
            GUI.Box(new Rect(136, 104, 590, 438), "GARAGE CONTROLS — QUICK REFERENCE");
            GUI.Label(new Rect(166, 146, 250, 24), "KEYBOARD");
            GUI.Label(new Rect(166, 178, 520, 22), "Left / Right   Change vehicle");
            GUI.Label(new Rect(166, 204, 520, 22), "Up / Down       Change engine");
            GUI.Label(new Rect(166, 230, 520, 22), "N                New / used engine");
            GUI.Label(new Rect(166, 256, 520, 22), "U                Starter bundle");
            GUI.Label(new Rect(166, 282, 520, 22), "1–8              Buy / install parts");
            GUI.Label(new Rect(166, 308, 520, 22), "Q / E            Rotate car");
            GUI.Label(new Rect(166, 334, 520, 22), "I / L / H / B / D  Inspect / custom / health / sheet / dyno");
            GUI.Label(new Rect(166, 376, 250, 24), "CONTROLLER");
            GUI.Label(new Rect(166, 408, 520, 22), "LB / RB          Change vehicle");
            GUI.Label(new Rect(166, 434, 520, 22), "Y / D-pad up     Change engine");
            GUI.Label(new Rect(166, 460, 520, 22), "X                Starter bundle");
            GUI.Label(new Rect(166, 486, 430, 22), "A                Race    •    Back: dyno");
            if (GUI.Button(new Rect(590, 500, 106, 30), "CLOSE [K]")) showControls = false;
        }

        private void DrawEngineHealth()
        {
            EngineCondition condition = GarageSession.GetEngineCondition(Engine.id, engineIsNew);
            GUI.Box(new Rect(104, 72, 650, 470), "ENGINE HEALTH — DIAGNOSTIC & SERVICE");
            GUI.Label(new Rect(134, 112, 590, 24), Engine.displayName + "  •  " + (engineIsNew ? "NEW" : "USED") + "  •  " + EngineHealthLabel(condition.OverallHealth));
            GUI.Label(new Rect(134, 142, 590, 22), "LAST PASS NOTE: " + condition.lastDamageCause);
            GUI.Label(new Rect(134, 174, 350, 22), "COMPONENT");
            GUI.Label(new Rect(494, 174, 200, 22), "SERVICE STATUS");

            for (int index = 0; index < EngineComponentWearCatalog.All.Count; index++)
            {
                EngineComponentWearSpec component = EngineComponentWearCatalog.All[index];
                float health = condition.GetHealth(component.id);
                float y = 204 + index * 44;
                GUI.Label(new Rect(134, y, 350, 21), component.displayName + " — " + ComponentConcern(component.id));
                GUI.Label(new Rect(494, y, 200, 21), ComponentServiceLabel(health));
                GUI.HorizontalSlider(new Rect(134, y + 25, 540, 12), health, 0f, 1f);
            }

            int repairCost = GarageSession.RepairCost(Engine, engineIsNew);
            GUI.Label(new Rect(134, 438, 520, 22), repairCost > 0 ? "A service restores this engine to " + (engineIsNew ? "100%" : "98%") + " health." : "This engine is already fully serviced.");
            if (GUI.Button(new Rect(134, 472, 260, 34), "SERVICE ENGINE — " + repairCost.ToString("N0") + " VTC")) RepairSelectedEngine();
            if (GUI.Button(new Rect(562, 472, 130, 34), "CLOSE")) showEngineHealth = false;
        }

        private static string ComponentConcern(string componentId)
        {
            switch (componentId)
            {
                case "rings": return "compression / blow-by";
                case "bearings": return "oil pressure / knock";
                case "head_gasket": return "heat / cylinder pressure";
                case "valvetrain": return "over-rev protection";
                case "turbo": return "overspeed / heat";
                default: return "general condition";
            }
        }

        private static string ComponentServiceLabel(float health)
        {
            float percent = health * 100f;
            if (percent >= 98f) return "EXCELLENT — " + percent.ToString("0") + "%";
            if (percent >= 85f) return "SERVICEABLE — " + percent.ToString("0") + "%";
            if (percent >= 65f) return "WORN — " + percent.ToString("0") + "%";
            return "AT RISK — " + percent.ToString("0") + "%";
        }

        private void DrawBuildSheet(DragBuild preview)
        {
            GUI.Box(new Rect(104, 70, 650, 500), "BUILD SHEET — READY-TO-RACE CHECK");
            GUI.Label(new Rect(134, 112, 580, 24), Vehicle.displayName + "  •  " + (Vehicle.drivetrain == DrivetrainLayout.Fwd ? "FWD" : "RWD"));
            GUI.Label(new Rect(134, 142, 580, 24), "ENGINE: " + Engine.displayName + "  •  " + (engineIsNew ? "NEW" : "USED") + "  •  " + EngineHealthLabel(GarageSession.GetEngineHealth(Engine.id, engineIsNew)));
            float torqueEstimate = Engine.peakTorqueLbFt * preview.EngineHealthMultiplier * preview.TorqueMultiplier;
            GUI.Label(new Rect(134, 172, 580, 24), "POWER ESTIMATE: " + preview.Horsepower.ToString("0") + " hp  •  " + torqueEstimate.ToString("0") + " lb-ft  •  " + preview.MassKg.ToString("0") + " kg");
            GUI.Label(new Rect(134, 208, 280, 22), "INSTALLED PARTS (" + InstalledPartCount() + "/" + CarvinoCatalog.Upgrades.Count + ")");
            GUI.Label(new Rect(442, 208, 270, 22), "STATUS");
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
            {
                UpgradeSpec part = CarvinoCatalog.Upgrades[index];
                bool installed = (upgradeMask & (1 << index)) != 0;
                bool owned = GarageSession.OwnsPart(index);
                float y = 236 + index * 23;
                GUI.Label(new Rect(134, y, 300, 20), part.displayName);
                GUI.Label(new Rect(442, y, 270, 20), installed ? "INSTALLED" : owned ? "OWNED — NOT INSTALLED" : "NOT OWNED — " + part.price.ToString("N0") + " VTC");
            }
            string readiness = GarageSession.OwnsEngine(Engine.id, engineIsNew) ? "ENGINE OWNED — YOU CAN RACE THIS BUILD" : "ENGINE NOT OWNED — BUY IT BEFORE RACING";
            GUI.Box(new Rect(134, 524, 460, 28), readiness);
            if (GUI.Button(new Rect(612, 524, 110, 28), "CLOSE")) showBuildSheet = false;
        }

        private int InstalledPartCount()
        {
            int count = 0;
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
                if ((upgradeMask & (1 << index)) != 0) count++;
            return count;
        }

        private static string EngineHealthLabel(float health)
        {
            float percent = health * 100f;
            if (percent >= 98f) return "EXCELLENT — " + percent.ToString("0") + "%";
            if (percent >= 85f) return "SERVICEABLE — " + percent.ToString("0") + "%";
            if (percent >= 65f) return "WORN — " + percent.ToString("0") + "% (REPAIR ADVISED)";
            return "AT RISK — " + percent.ToString("0") + "% (REPAIR BEFORE RACING)";
        }

        private void DrawAppearance()
        {
            GUI.Box(new Rect(120, 92, 600, 370), "APPEARANCE — ORIGINAL GARAGE CUSTOMIZATION");
            GUI.Label(new Rect(150, 134, 500, 22), "PAINT COLOR");
            for (int index = 0; index < PaintNames.Length; index++)
            {
                int paintIndex = index;
                float x = 150 + (index % 2) * 270;
                float y = 166 + (index / 2) * 44;
                Color previous = GUI.color;
                GUI.color = PaintColors[index];
                if (GUI.Button(new Rect(x, y, 250, 34), PaintNames[index])) SetAppearance(paintIndex, GarageSession.WheelFinishIndex % WheelNames.Length);
                GUI.color = previous;
            }
            GUI.Label(new Rect(150, 310, 500, 22), "WHEEL FINISH");
            for (int index = 0; index < WheelNames.Length; index++)
            {
                int wheelIndex = index;
                float x = 150 + (index % 2) * 270;
                float y = 342 + (index / 2) * 40;
                if (GUI.Button(new Rect(x, y, 250, 30), WheelNames[index])) SetAppearance(GarageSession.PaintIndex % PaintNames.Length, wheelIndex);
            }
            if (GUI.Button(new Rect(560, 412, 126, 28), "CLOSE")) showAppearance = false;
        }

        private void DrawHistory(DragBuild preview)
        {
            GUI.Box(new Rect(142, 104, 580, 352), "SAVED BUILD HISTORY");
            GUI.Label(new Rect(170, 146, 500, 24), preview.vehicle.displayName + "  •  " + preview.engine.displayName);
            DrawRecordRow(174, "Prepped strip — 1/8 mile", RaceHistory.BestEt(preview, TrackSurfaceCatalog.PreppedStrip, RaceDistanceCatalog.EighthMile), RaceHistory.BestTrapMph(preview, TrackSurfaceCatalog.PreppedStrip, RaceDistanceCatalog.EighthMile));
            DrawRecordRow(218, "Prepped strip — 1/4 mile", RaceHistory.BestEt(preview, TrackSurfaceCatalog.PreppedStrip, RaceDistanceCatalog.QuarterMile), RaceHistory.BestTrapMph(preview, TrackSurfaceCatalog.PreppedStrip, RaceDistanceCatalog.QuarterMile));
            DrawRecordRow(262, "Street — 1/8 mile", RaceHistory.BestEt(preview, TrackSurfaceCatalog.Street, RaceDistanceCatalog.EighthMile), RaceHistory.BestTrapMph(preview, TrackSurfaceCatalog.Street, RaceDistanceCatalog.EighthMile));
            DrawRecordRow(306, "Street — 1/4 mile", RaceHistory.BestEt(preview, TrackSurfaceCatalog.Street, RaceDistanceCatalog.QuarterMile), RaceHistory.BestTrapMph(preview, TrackSurfaceCatalog.Street, RaceDistanceCatalog.QuarterMile));
            GUI.Label(new Rect(170, 356, 500, 22), $"CAREER: {RaceHistory.TotalWins} wins  •  {RaceHistory.TotalPasses} passes");
            GUI.Label(new Rect(170, 388, 500, 34), "Records are unique to the saved vehicle, engine, installed parts, surface, and pass length.");
            if (GUI.Button(new Rect(564, 408, 126, 28), "CLOSE")) showHistory = false;
        }

        private static void DrawRecordRow(float y, string label, float et, float mph)
        {
            GUI.Label(new Rect(170, y, 270, 24), label);
            GUI.Label(new Rect(458, y, 200, 24), et > 0f ? et.ToString("0.000") + "s  @  " + mph.ToString("0.0") + " mph" : "No completed pass yet");
        }

        private void DrawPartButton(int index, Rect rect)
        {
            UpgradeSpec part = CarvinoCatalog.Upgrades[index];
            bool installed = (upgradeMask & (1 << index)) != 0;
            string action = installed ? "REMOVE" : GarageSession.OwnsPart(index) ? "INSTALL" : "BUY";
            if (GUI.Button(rect, action + "  " + part.displayName + "\n" + part.price.ToString("N0") + " VTC")) ToggleUpgrade(index);
        }

        private string PartNameList()
        {
            if (upgradeMask == 0) return "stock";
            string result = string.Empty;
            for (int index = 0; index < CarvinoCatalog.Upgrades.Count; index++)
            {
                if ((upgradeMask & (1 << index)) == 0) continue;
                if (result.Length > 0) result += ", ";
                result += CarvinoCatalog.Upgrades[index].displayName;
            }
            return result;
        }
    }
}
