using UnityEngine;
using UnityEngine.UI;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Utils
{
    public class PreviewCameraController : MonoBehaviour
    {
        [Header("📷 相机设置")]
        [Tooltip("平时用的主相机")]
        public GameObject mainCamera;

        [Tooltip("预览时用的特写相机")]
        public GameObject previewCamera;

        [Header("🔘 交互按钮")]
        [Tooltip("主界面上的'进入预览'按钮")]
        public Button enterPreviewButton;

        [Tooltip("预览界面里的'退出预览'按钮")]
        public Button exitPreviewButton;

        // ★★★ 【新增】 需要隐藏的 UI 面板组 ★★★
        [Header("🙈 UI 显示控制")]
        [Tooltip("进入预览模式时，需要隐藏的 UI 面板（例如：顶部栏、建造栏、背包等）")]
        public GameObject[] panelsToHide;

        private void Start()
        {
            // 1. 初始化相机状态
            if (mainCamera != null) mainCamera.SetActive(true);
            if (previewCamera != null) previewCamera.SetActive(false);

            // 2. 绑定按钮事件
            if (enterPreviewButton != null)
                enterPreviewButton.onClick.AddListener(EnterPreviewMode);

            if (exitPreviewButton != null)
                exitPreviewButton.onClick.AddListener(ExitPreviewMode);
        }

        // === 进入预览模式 ===
        public void EnterPreviewMode()
        {
            // 1. 切换相机
            if (mainCamera != null) mainCamera.SetActive(false);
            if (previewCamera != null) previewCamera.SetActive(true);

            // 2. 切换按钮状态
            if (enterPreviewButton != null) enterPreviewButton.gameObject.SetActive(false);
            if (exitPreviewButton != null) exitPreviewButton.gameObject.SetActive(true);

            // 3. ★★★ 隐藏指定的 UI 面板 ★★★
            ToggleUIPanels(false);

            Debug.Log("📷 进入预览模式：UI已隐藏");
        }

        // === 退出预览模式 ===
        public void ExitPreviewMode()
        {
            // 1. 还原相机
            if (mainCamera != null) mainCamera.SetActive(true);
            if (previewCamera != null) previewCamera.SetActive(false);

            // 2. 还原按钮状态
            if (enterPreviewButton != null) enterPreviewButton.gameObject.SetActive(true);
            if (exitPreviewButton != null) exitPreviewButton.gameObject.SetActive(false);

            // 3. ★★★ 恢复 UI 面板显示 ★★★
            ToggleUIPanels(true);

            Debug.Log("📷 退出预览模式：UI已恢复");
        }

        // 辅助方法：批量开关 UI
        private void ToggleUIPanels(bool show)
        {
            if (panelsToHide == null) return;

            foreach (var panel in panelsToHide)
            {
                if (panel != null)
                {
                    panel.SetActive(show);
                }
            }
        }
    }
}