using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils {
    /// <summary>
    /// Singleton GameConfig class that should be placed in the first scene where the grid system occurs.
    /// This class will not be destroyed when switching scenes, so it is enough to add it once
    /// </summary>
    public class GameConfig : MonoBehaviour {
        public static GameConfig Instance;

        [field: Header("Save System")]
        [field: SerializeField]
        [field: Tooltip("Enables or disables the complete save system ")]
        public bool EnableSaveSystem { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Folder path where the saveFile should be stored in. Root is considered the Assets folder in your project")]
        public string SaveFilePath { get; private set; } = "SpaceFusion/SF Grid Building System/Saves";

        [field: SerializeField]
        [field: Tooltip("Name of the saveFile where the game state will be saved. It will be a .json file located under SF Grid System/Saves ")]
        public string SaveFileName { get; private set; } = "SaveFile";

        [field: Header("Grid Visualization")]
        [field: SerializeField]
        [field:
            Tooltip(
                "Defines the Layer Mask for the input control of the grid. You will only be able to place objects on an ground that has the same layer as defined here ")]
        public LayerMask PlacementLayerMask { get; private set; }

        [field: Tooltip("Optional prefab for visualizing the placement grid cells")]
        [field: SerializeField]
        public GameObject CellVisualizationPrefab { get; private set; }

        [field: Tooltip("Optional GameObject that visualizes the placement of the whole grid. Can only be used if cellVisualization is empty!!")]
        [field: SerializeField]
        public GridVisualizer GridVisualizationPrefab { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Color of the Grid visualization shader")]
        public Color GridVisualizationColor { get; private set; } = Color.white;

        [field: SerializeField]
        [field: Tooltip("Cell margin of the Grid visualization shader")]
        [field: Range(0.01f, 0.4f)]
        public float GridVisualizationThickness { get; private set; } = 0.05f;

        [field: Header("Grid Control")]
        [field: SerializeField]
        [field:
            Tooltip(
                "On true always shows grid, independent of the state you are in. If false then you can configure the visibility with the next property ShowGridOnPlacement")]
        public bool AlwaysShowGrid { get; private set; }

        [field: SerializeField]
        [field:
            Tooltip(
                "If true shows Grid only if placement state is active, otherwise grid will never be shown. Only effective if alwaysShowGrid is disabled!")]
        public bool ShowGridOnPlacement { get; private set; } = true;


        [field: Header("Preview Control")]
        [field: SerializeField]
        [field: Tooltip("True if you want to use a separate preview material for placement previews instead of the original object material")]
        public bool UsePreviewMaterial { get; private set; } = true;

        [field: SerializeField]
        [field: Tooltip("If UsePreviewMaterial is enabled you need to provide a material that will be applied to the previewed objects")]
        public Material PreviewMaterialPrefab { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Offset previewed object in Y axis, so that the preview object will hover over the grid")]
        public float PreviewYOffset { get; private set; } = 0.06f;

        [field: SerializeField]
        [field: Tooltip("Prefab to preview the potentially occupied grid cells when object is placed")]
        public GameObject CellIndicatorPrefab { get; private set; }

        [field: Header("Preview Colors")]
        [field: SerializeField]
        [field: Tooltip("CellIndicator color if the placement position is valid")]
        public Color ValidPlacementColor { get; private set; } = Color.green;

        [field: SerializeField]
        [field: Tooltip("CellIndicator color if the placement position is invalid")]
        public Color InValidPlacementColor { get; private set; } = Color.red;

        [field: SerializeField]
        [field: Tooltip("CellIndicator color when the selected position has a removable object")]
        public Color ValidRemovalColor { get; private set; } = Color.green;

        [field: SerializeField]
        [field: Tooltip("CellIndicator color when the selected position is missing a removable object")]
        public Color InValidRemovalColor { get; private set; } = Color.red;

        [field: Header("Placed Objects")]
        [field: SerializeField]
        [field: Tooltip("PlaceableSODatabase scriptable reference that holds the list of all placeable objects ")]
        public PlaceableObjectDatabase PlaceableObjectDatabase { get; private set; }

        [field: SerializeField]
        [field: Tooltip("Hold time until action of placed object is triggered (e.g. show Action menu --> remove, move or cancel)")]
        public float HoldThreshold { get; private set; } = 0.6f;

        [field: Header("Camera Control")]
        [field: SerializeField]
        [field: Tooltip("Drag speed in x and z axis for the camera")]
        public float DragSpeed { get; private set; } = 10f;

        [field: SerializeField]
        [field: Tooltip("zoom speed for the cameras y axis")]
        public float ZoomSpeed { get; private set; } = 10f;

        [field: SerializeField]
        [field: Tooltip("Camera rotation speed")]
        public float RotationSpeed { get; private set; } = 5f;

        [field: SerializeField]
        [field: Tooltip("Camera Boundary in X Axis")]
        public Vector2 XBoundary { get; private set; } = new(0, 20);

        [field: SerializeField]
        [field:
            Tooltip("Zoom soft-boundary, can go slightly over the bounds(prevents cam shakes) but blocks further zooming in the desired direction")]
        public Vector2 YBoundary { get; private set; } = new(5, 10);

        [field: SerializeField]
        [field: Tooltip("Camera Boundary in X Axis")]
        public Vector2 ZBoundary { get; private set; } = new(-8, 7);

        [field: Header("Camera auto movement")]
        [field: SerializeField]
        [field:
            Tooltip(
                "Set false to disable auto move completely")]
        public bool EnableAutoMove { get; private set; } = true;

        [field: SerializeField]
        [field: Tooltip(
            "Defines the margin from the screen edges from where the camera starts to moving in the Mouse direction (if the mouse is in this area)")]
        public float EdgeMarginForAutoMove { get; private set; } = 50;

        [field: SerializeField]
        [field:
            Tooltip(
                "If true then auto move is only activated when a placement mode is active (e.g. place or move). Otherwise it will always be active")]
        public bool RestrictAutoMoveForPlacement { get; private set; } = true;

        private void Awake() {
            if (Instance != null) {
                Destroy(this);
            }

            Instance = this;
            DontDestroyOnLoad(this);
        }
    }
}