using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI {
    public class PlaceableShopButton : MonoBehaviour {
        [SerializeField]
        private Button button;
        [SerializeField]
        private Image icon;

        [Header("单个物品属性 UI")]
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private TextMeshProUGUI _safetyText;
        [SerializeField] private TextMeshProUGUI _aestheticsText;
        [SerializeField] private TextMeshProUGUI _environmentText;
        [SerializeField] private TextMeshProUGUI _comfortText;

        [Header("可选：点击时刷新详情面板")]
        [SerializeField] private ShopSwitcher _shopSwitcher;

        private static string FormatSigned(int value) {
            return value > 0 ? $"+{value}" : value.ToString();
        }

        public void Initialize(Placeable placeable) {
            if (_shopSwitcher == null) _shopSwitcher = GetComponentInParent<ShopSwitcher>(true);

            button.onClick.AddListener(() => {
                PlacementSystem.Instance.StartPlacement(placeable.GetAssetIdentifier());
                if (_shopSwitcher != null) _shopSwitcher.Setup(placeable);
            });
            if (placeable.Icon) {
                icon.sprite = placeable.Icon;
                icon.color = Color.white;
            } else {
                // fallback to name if icon not set
                button.GetComponentInChildren<TextMeshProUGUI>().text = placeable.GetAssetIdentifier();
            }

            if (_costText != null) _costText.text = $"成本: {placeable.Cost}";
            if (_safetyText != null) _safetyText.text = $"安全: {FormatSigned(placeable.Safety)}";
            if (_aestheticsText != null) _aestheticsText.text = $"美观: {FormatSigned(placeable.Aesthetics)}";
            if (_environmentText != null) _environmentText.text = $"环境: {FormatSigned(placeable.Environment)}";
            if (_comfortText != null) _comfortText.text = $"舒适: {FormatSigned(placeable.Comfort)}";
        }
    }
}