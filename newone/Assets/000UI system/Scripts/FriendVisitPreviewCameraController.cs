using UnityEngine;

public class FriendVisitPreviewCameraController : MonoBehaviour
{
    [Header("=== 主相机（好友家主视角用的相机）===")]
    public Camera mainCamera;

    [Header("=== 预览相机（NPCCamera）：可拖场景里的，也可拖 Project 里的 Prefab ===")]
    public Camera npcCameraPrefabOrSceneCamera;

    [Header("=== 好友家视角点（空物体 Transform）===")]
    public Transform friendMainView;     // 好友家主视角点
    public Transform friendPreviewView;  // 好友家预览视角点（你想要的那个角度）

    [Header("=== UI（进入/退出预览时要隐藏/显示）===")]
    public GameObject previewButton;        // “预览”按钮
    public GameObject cancelPreviewButton;  // “取消预览”按钮
    public GameObject functionButtons;      // FunctionButtons 整个面板（预览时隐藏）

    [Header("=== 可选：进入预览时也隐藏的按钮（不需要可不拖）===")]
    public GameObject btnCancelBuild;
    public GameObject btnExitBuild;

    private Camera _npcCam;        // 运行时实际使用的 NPCCamera（可能是实例化出来的）
    private bool _isPreview = false;

    private void Awake()
    {
        if (mainCamera == null)
        {
            // 尽量兜底：使用场景的 MainCamera
            mainCamera = Camera.main;
        }

        PrepareNpcCamera();
        SetUIState(false);
    }

    private void OnEnable()
    {
        // 避免切场景/重载后卡状态：保证默认回到主视角
        ForceExitPreviewToMain();
    }

    private void OnDisable()
    {
        // 防止离开好友家时还处于预览导致相机状态乱
        ForceExitPreviewToMain();
    }

    /// <summary>
    /// 按钮：进入预览（绑定到“预览”按钮 OnClick）
    /// </summary>
    public void EnterPreview()
    {
        PrepareNpcCamera();

        if (_npcCam == null)
        {
            Debug.LogError("[FriendVisitPreviewCameraController] NPCCamera 未设置！请在 Inspector 里把 NPCCamera 拖到 npcCameraPrefabOrSceneCamera。");
            return;
        }
        if (friendPreviewView == null)
        {
            Debug.LogError("[FriendVisitPreviewCameraController] friendPreviewView 未设置！请创建空物体 FriendPreviewView 并拖进来。");
            return;
        }

        // 关键：进入预览时 强制启用 NPCCamera，禁用主相机
        if (mainCamera != null) mainCamera.enabled = false;

        _npcCam.gameObject.SetActive(true);
        _npcCam.enabled = true;

        SnapCameraTo(_npcCam, friendPreviewView);

        _isPreview = true;
        SetUIState(true);
    }

    /// <summary>
    /// 按钮：退出预览（绑定到“取消预览”按钮 OnClick）
    /// </summary>
    public void ExitPreview()
    {
        ForceExitPreviewToMain();
    }

    // -------------------------
    // 内部方法
    // -------------------------

    private void ForceExitPreviewToMain()
    {
        // 退出预览：关 NPCCamera，开主相机，并回到好友家主视角点
        if (_npcCam != null)
        {
            _npcCam.enabled = false;
            // 这里不强制 SetActive(false) 也可以；为了避免被其他脚本依赖，我们只关 Camera 组件
            _npcCam.gameObject.SetActive(false);
        }

        if (mainCamera != null)
        {
            mainCamera.enabled = true;

            if (friendMainView != null)
            {
                SnapCameraTo(mainCamera, friendMainView);
            }
        }

        _isPreview = false;
        SetUIState(false);
    }

    private void PrepareNpcCamera()
    {
        if (_npcCam != null) return;
        if (npcCameraPrefabOrSceneCamera == null) return;

        // 情况 A：拖的是 Project 里的 Prefab（不在场景中）
        // 情况 B：拖的是场景里的 NPCCamera（在场景中）
        bool isSceneObject = npcCameraPrefabOrSceneCamera.gameObject.scene.IsValid();

        if (!isSceneObject)
        {
            // Prefab -> 实例化一份
            _npcCam = Instantiate(npcCameraPrefabOrSceneCamera);
            _npcCam.name = "NPCCamera_Runtime";
            _npcCam.gameObject.SetActive(false);
            _npcCam.enabled = false;
        }
        else
        {
            // 场景对象 -> 直接用
            _npcCam = npcCameraPrefabOrSceneCamera;
            _npcCam.gameObject.SetActive(false);
            _npcCam.enabled = false;
        }
    }

    private static void SnapCameraTo(Camera cam, Transform viewPoint)
    {
        if (cam == null || viewPoint == null) return;

        cam.transform.position = viewPoint.position;
        cam.transform.rotation = viewPoint.rotation;
    }

    private void SetUIState(bool isPreview)
    {
        // 预览：预览按钮隐藏，取消预览显示
        if (previewButton != null) previewButton.SetActive(!isPreview);
        if (cancelPreviewButton != null) cancelPreviewButton.SetActive(isPreview);

        // 预览时隐藏 FunctionButtons（你要求的）
        if (functionButtons != null) functionButtons.SetActive(!isPreview);

        // 预览时隐藏“取消建造/退出建造”（可选）
        if (btnCancelBuild != null) btnCancelBuild.SetActive(!isPreview);
        if (btnExitBuild != null) btnExitBuild.SetActive(!isPreview);
    }
}