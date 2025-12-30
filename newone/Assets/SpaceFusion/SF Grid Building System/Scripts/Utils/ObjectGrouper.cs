using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils {
    /// <summary>
    /// Groups objects by the gridDataType so we are able to modify each gridData separately if we want to --> e.g. show only objects of specific type in the remove stage
    /// </summary>
    public class ObjectGrouper : MonoBehaviour {
        public static ObjectGrouper Instance;

        private readonly Dictionary<GridDataType, GameObject> _objectGroupDict = new();

        private void Awake() {
            if (Instance != null) {
                Destroy(gameObject);
            }
            Instance = this;
        }


        public void AddToGroup(GameObject obj, GridDataType gridType) {
            _objectGroupDict.TryGetValue(gridType, out var groupHolder);
            if (!groupHolder) {
                var newGroupHolder = new GameObject(gridType.ToString()) {
                    transform = {
                        parent = transform,
                        localPosition = Vector3.zero,
                        localRotation = Quaternion.identity
                    }
                };
                groupHolder = newGroupHolder;
                _objectGroupDict.Add(gridType, newGroupHolder);
            }

            obj.transform.parent = groupHolder.transform;
        }

        public void DisplayOnlyObjectsOfSelectedGridType(GridDataType gridType) {
            foreach (var entry in _objectGroupDict) {
                entry.Value.gameObject.SetActive(entry.Key == gridType);
            }
        }

        public void DisplayAll() {
            foreach (var entry in _objectGroupDict) {
                entry.Value.gameObject.SetActive(true);
            }
        }
    }
}