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


        public void Initialize(Placeable placeable) {

            button.onClick.AddListener(() => PlacementSystem.Instance.StartPlacement(placeable.GetAssetIdentifier()));
            if (placeable.Icon) {
                icon.sprite = placeable.Icon;
                icon.color = Color.white;
            } else {
                // fallback to name if icon not set
                button.GetComponentInChildren<TextMeshProUGUI>().text = placeable.GetAssetIdentifier();
            }
        }
    }
}