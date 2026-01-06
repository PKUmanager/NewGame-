using UnityEngine;
using UnityEngine.UI;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.UI
{
    [RequireComponent(typeof(Button))]
    public class UI_UndoButton : MonoBehaviour
    {
        private Button _button;

        void Start()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(OnUndoClicked);
        }

        private void OnUndoClicked()
        {
            if (UndoManager.Instance != null)
            {
                UndoManager.Instance.PerformUndo();
            }
            else
            {
                Debug.LogWarning("场景中找不到 UndoManager，请创建一个空物体并挂载 UndoManager 脚本。");
            }
        }
    }
}