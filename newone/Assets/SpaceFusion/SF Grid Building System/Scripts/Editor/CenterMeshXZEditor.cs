using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEditor;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Editor {
    [CustomEditor(typeof(CenterMeshXZ))]
    public class CenterMeshXZEditor : UnityEditor.Editor {
        public override void OnInspectorGUI() {
            DrawDefaultInspector();

            var script = (CenterMeshXZ)target;
            if (GUILayout.Button("Center Mesh XZ")) {
                script.Awake();
            }
        }
    }
}