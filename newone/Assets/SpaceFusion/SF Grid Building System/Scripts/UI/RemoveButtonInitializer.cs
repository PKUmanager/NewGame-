using UnityEngine;
using UnityEngine.UI;
using SpaceFusion.SF_Grid_Building_System.Scripts.Core;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI
{
    // 这是一个极简的按钮控制脚本
    [RequireComponent(typeof(Button))]
    public class UI_DeleteButton : MonoBehaviour
    {
        private Button _myButton;

        void Start()
        {
            // 1. 获取当前物体上的 Button 组件
            _myButton = GetComponent<Button>();

            // 2. 监听点击事件
            // 一旦点击，直接呼叫 PlacementSystem 开启“全删除模式”
            _myButton.onClick.AddListener(OnDeleteClicked);
        }

        private void OnDeleteClicked()
        {
            if (PlacementSystem.Instance != null)
            {
                // 调用我们上一轮修改好的“万能删除”方法
                PlacementSystem.Instance.StartRemovingAll();
            }
        }
    }
}