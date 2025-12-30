using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEditor;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Editor {
    /// <summary>
    /// Helper script that allows you to precalculate the object size of a placeable Object prefab via the editor
    /// It will prefill the Size property of the PlaceableSO, which then later will be used to properly initialize the objects on the grid
    /// The size is independent of the grid cell size, the cell size handling will be handled on runtime
    /// </summary>
    [CustomEditor(typeof(Placeable))]
    public class PlaceableGridDataCalculator : UnityEditor.Editor {
        private static Vector2 CalculateObjectSize(GameObject prefab) {
            if (!prefab) {
                Debug.LogWarning("Prefab is not assigned!");
                return Vector2.zero;
            }

            // Load the prefab as an asset (without instantiating it), so we can properly calculate the size
            var prefabAsset = PrefabUtility.LoadPrefabContents(AssetDatabase.GetAssetPath(prefab));
            if (!prefabAsset) {
                Debug.LogWarning("Could not load prefab asset.");
                return Vector2.zero;
            }

            var rend = prefabAsset.GetComponentInChildren<Renderer>();
            if (!rend) {
                Debug.LogWarning("No Renderer found in the prefab!");
                PrefabUtility.UnloadPrefabContents(prefabAsset);
                return Vector2.zero;
            }

            var originalSize = rend.bounds.size;
            PrefabUtility.UnloadPrefabContents(prefabAsset);
            const int decimalsToRound = 6;
            var roundedSize = SfMathUtils.RoundVector(originalSize, decimalsToRound);
            var objectSize = new Vector2(roundedSize.x, roundedSize.z);
            Debug.Log($"Successfully calculated size of {prefab.name}: {objectSize}");
            return objectSize;
        }


        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            // Add spacing before the button
            GUILayout.Space(10);
            var buttonStyle = new GUIStyle(GUI.skin.button) {
                fontSize = 14,
                fixedHeight = 35,
                margin = new RectOffset(10, 10, 10, 5)
            };

            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);

            if (GUILayout.Button("Calculate Object Size", buttonStyle)) {
                var script = (Placeable)target;
                var result = CalculateObjectSize(script.Prefab);
                if (result == Vector2.zero) {
                    return;
                }

                script.SetObjectSize(result);
                // Ensure changes are saved
                EditorUtility.SetDirty(script);
            }
        }
    }
}