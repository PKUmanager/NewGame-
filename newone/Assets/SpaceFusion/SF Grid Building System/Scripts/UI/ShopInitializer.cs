using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using SpaceFusion.SF_Grid_Building_System.Scripts.Scriptables;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI {
    /// <summary>
    /// filters the database for entries with predefined objectGroup and creates buttons for each entry that trigger object placement
    /// Additionally adapts the scrollable rect width based on the layout config and the button amount
    /// </summary>
    public class ShopInitializer : MonoBehaviour {
        [SerializeField]
        private GameObject contentHolder;
        [SerializeField]
        private GameObject buttonPrefab;

        public ObjectGroup objectGroup;


        private GameConfig _config;
        private void Start() {
            _config = GameConfig.Instance;
            var objects = _config.PlaceableObjectDatabase.GetGroupedByObjectGroup(objectGroup);
            if (objects.Count == 0) {
                Debug.LogWarning($"No objects of group '{objectGroup}' found in database");
                return;
            }

            foreach (var placeable in objects) {
                var button = Instantiate(buttonPrefab, contentHolder.transform);
                var placeableShopButton = button.GetComponent<PlaceableShopButton>();
                placeableShopButton.Initialize(placeable);
            }

            // calculate the needed width of the scrollable rect based on the amount of buttons and the grid layout configuration
            var layout = contentHolder.GetComponent<GridLayoutGroup>();
            var sizePerButton = layout.cellSize.x + layout.padding.left + layout.padding.right;
            var rectTransform = contentHolder.GetComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(sizePerButton * objects.Count + layout.spacing.x, rectTransform.sizeDelta.y);
        }
    }
}