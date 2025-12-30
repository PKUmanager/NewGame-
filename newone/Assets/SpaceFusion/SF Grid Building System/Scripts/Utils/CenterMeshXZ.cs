using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils {
    /// <summary>
    /// Centers the child mesh or child meshContainer in a manner that the new virtual pivot point is exactly at 0,0,0 position of the parent object holder.
    /// This assures that we can smoothly rotate the objects without any weird behaviours.
    /// And this also simplifies rotation based offset handling for the grid, since all placed objects now behave the same, independent of the actual pivot point of the 3D model
    /// This script assumes that there is only 1 child object with a mesh or only 1 meshContainer that contains multiple child meshes
    /// </summary>
    public class CenterMeshXZ : MonoBehaviour {
        [Tooltip("Assign your child object with the mesh or a meshHolder object with children that have meshes")]
        private Transform _childMeshOrContainer;

        public void Awake() {
            _childMeshOrContainer = transform.GetChild(0);
            if (!_childMeshOrContainer) {
                Debug.LogError("Child object is not assigned!");
                return;
            }

            var isCentered = CenterChildMesh();
            if (isCentered) {
                // finished centering
                return;
            }

            // otherwise child is probably a mesh container, so we need to go one layer deeper
            CenterMeshContainer();
        }


        private bool CenterChildMesh() {
            var meshFilter = _childMeshOrContainer.GetComponent<MeshFilter>();
            if (!meshFilter || !meshFilter.sharedMesh) {
                // No MeshFilter or Mesh found on the child object!
                return false;
            }

            var mesh = meshFilter.sharedMesh;
            var bounds = mesh.bounds;
            var meshCenter = bounds.center;
            var offset = new Vector3(-meshCenter.x, 0f, -meshCenter.z);

            // Debug.Log($"Successfully recalculated mesh center: {offset}");
            _childMeshOrContainer.localPosition = offset;
            return true;
        }

        /// <summary>
        /// The child object has no mesh attached, so it is probably a meshContainer with additional children with meshes
        /// We fetch all meshes of the children and create a combined meshCenter that we use to calculate the final pivot offset
        /// </summary>
        private void CenterMeshContainer() {
            if (!_childMeshOrContainer) {
                Debug.LogError("Mesh container is not assigned!");
                return;
            }

            var meshFilters = _childMeshOrContainer.GetComponentsInChildren<MeshFilter>();

            if (meshFilters.Length == 0) {
                Debug.LogError("No MeshFilters found in child objects!");
                return;
            }

            var combinedCenter = Vector3.zero;
            var meshCount = 0;

            foreach (var meshFilter in meshFilters) {
                if (!meshFilter.sharedMesh) {
                    continue;
                }

                var mesh = meshFilter.sharedMesh;
                // Local-space bounds of the mesh
                var bounds = mesh.bounds;
                combinedCenter += meshFilter.transform.localPosition + bounds.center;
                meshCount++;
            }

            if (meshCount == 0) {
                Debug.LogError("No Mesh found!");
                return;
            }

            combinedCenter /= meshCount;

            // Offset only X and Z, keep Y unchanged, since we do not want to change the height of the object
            var offset = new Vector3(-combinedCenter.x, 0f, -combinedCenter.z);
            // Debug.Log($"Successfully recalculated mesh center: {offset}");
            _childMeshOrContainer.localPosition = offset;
        }
    }
}