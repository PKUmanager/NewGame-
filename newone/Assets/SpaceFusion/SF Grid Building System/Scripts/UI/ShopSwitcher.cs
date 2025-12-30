using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI {
    /// <summary>
    /// Simple Tab switcher, When calling Enable with the index parameter, disables all shops and only enables the one with matching index
    /// </summary>
    public class ShopSwitcher : MonoBehaviour {
        private readonly Dictionary<ObjectGroup, GameObject> _shopDir = new();

        public void Start() {
            var shops = GetComponentsInChildren<ShopInitializer>(true);
            foreach (var initializer in shops) {
                _shopDir.Add(initializer.objectGroup, initializer.gameObject);
            }
        }

        public void ActivateGroup(ObjectGroup targetGroup) {
            foreach (var kvp in _shopDir) {
                kvp.Value.SetActive(kvp.Key == targetGroup);
            }
        }
    }
}