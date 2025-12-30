using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI {
    public class ShopTabInitializer : MonoBehaviour {
        [SerializeField]
        private int maxTabHolderLength=600;
        [SerializeField]
        private GridLayoutGroup gridLayoutGroup;
        [SerializeField]
        private GameObject buttonPrefab;
        [SerializeField]
        private ShopSwitcher shopSwitcher;
    

        private void Start() {
            var count = 0;
            foreach (ObjectGroup group in Enum.GetValues(typeof(ObjectGroup))) {
                var obj = Instantiate(buttonPrefab, transform);
                var button = obj.GetComponent<Button>();
                var text = obj.GetComponentInChildren<TextMeshProUGUI>();
                button.onClick.AddListener(() => {
                    shopSwitcher.ActivateGroup(group);
                });
                text.text = group.ToString();
                count++;
            }

            var usedSpacing = count * gridLayoutGroup.spacing.x; 
            var spaceLeft = maxTabHolderLength - usedSpacing;
            gridLayoutGroup.cellSize = new Vector2(Mathf.Ceil(spaceLeft/count), gridLayoutGroup.cellSize.y);
        }

    }
}