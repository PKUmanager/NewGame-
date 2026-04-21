using System.Collections.Generic;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using TMPro;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI {
    /// <summary>
    /// Simple Tab switcher, When calling Enable with the index parameter, disables all shops and only enables the one with matching index
    /// </summary>
    public class ShopSwitcher : MonoBehaviour {
        private readonly Dictionary<ObjectGroup, GameObject> _shopDir = new();

        [Header("物品属性详情面板（点选物品时刷新）")]
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _safetyText;
        [SerializeField] private TextMeshProUGUI _aestheticsText;
        [SerializeField] private TextMeshProUGUI _environmentText;
        [SerializeField] private TextMeshProUGUI _comfortText;

        private static string FormatSigned(int value) {
            return value > 0 ? $"+{value}" : value.ToString();
        }

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

        public void Setup(Placeable itemData) {
            if (itemData == null) return;

            if (_costText != null) _costText.text = $"{itemData.Cost}";
            if (_safetyText != null) _safetyText.text = $"{FormatSigned(itemData.Safety)}";
            if (_aestheticsText != null) _aestheticsText.text = $"{FormatSigned(itemData.Aesthetics)}";
            if (_environmentText != null) _environmentText.text = $" {FormatSigned(itemData.Environment)}";
            if (_comfortText != null) _comfortText.text = $"{FormatSigned(itemData.Comfort)}";
        }
    }
}