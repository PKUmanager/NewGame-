using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;
using SpaceFusion.SF_Grid_Building_System.Scripts.Enums;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI
{
    public class RemoveButtonInitializer : MonoBehaviour
    {
        [SerializeField]
        private Button buttonPrefab;

        private void Start()
        {
            // 1. 生成各个类型的删除按钮
            foreach (GridDataType gridType in Enum.GetValues(typeof(GridDataType)))
            {
                var removeButton = Instantiate(buttonPrefab, transform);

                // 逻辑保持不变
                removeButton.onClick.AddListener(() => PlacementSystem.Instance.StartRemoving(gridType));

                // [核心修改] 这里不再直接用 ToString()，而是调用下面的翻译函数
                removeButton.GetComponentInChildren<TextMeshProUGUI>()?.SetText(GetChineseName(gridType));
            }

            // 2. 生成“全部删除”按钮
            var removeAllButton = Instantiate(buttonPrefab, transform);
            removeAllButton.onClick.AddListener(() => PlacementSystem.Instance.StartRemovingAll());

            // [核心修改] 直接在这里把英文改成中文
            removeAllButton.GetComponentInChildren<TextMeshProUGUI>()?.SetText("所有类型");
        }

        /// <summary>
        /// 简单的翻译函数：把代码里的英文枚举转换成中文显示
        /// </summary>
        private string GetChineseName(GridDataType type)
        {
            // 根据你的 GridDataType.cs 文件，目前只有 Blocking 和 Terrain
            // 你可以在这里自由修改引号里的中文
            return type switch
            {
                GridDataType.Blocking => "物品",  // 对应 Blocking
                GridDataType.Terrain => "铺地",  // 对应 Terrain

                // 如果以后加了新类型，默认显示英文，或者你再来这里加一行
                _ => type.ToString()
            };
        }
    }
}