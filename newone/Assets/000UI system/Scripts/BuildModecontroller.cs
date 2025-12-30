using UnityEngine;

public class BuildModeController : MonoBehaviour
{
    [Header("UI Roots")]
    [SerializeField] private GameObject buildUIRoot;     // Grid Shop & Remove UI
    [SerializeField] private GameObject functionButtons; // FunctionButtons

    private void Awake()
    {
        // 默认：非建造模式
        SetBuildMode(false);
    }

    public void EnterBuildMode()
    {
        SetBuildMode(true);
    }

    public void ExitBuildMode()
    {
        SetBuildMode(false);
    }

    private void SetBuildMode(bool isBuildMode)
    {
        if (buildUIRoot != null) buildUIRoot.SetActive(isBuildMode);
        if (functionButtons != null) functionButtons.SetActive(!isBuildMode);
    }
}