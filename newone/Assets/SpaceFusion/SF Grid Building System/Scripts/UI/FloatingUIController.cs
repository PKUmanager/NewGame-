using UnityEngine;
using UnityEngine.UI;
using System;

public class FloatingUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform floatingPanel;
    [SerializeField] private Button rotateButton;
    [SerializeField] private Button confirmButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private bool bindButtonClicksInCode = false;

    [Header("Follow Target")]
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Vector3 worldOffset = Vector3.zero;
    
    private Action _onRotate;
    private Action _onConfirm;
    private Action _onCancel;

    private void Awake()
    {
        if (!bindButtonClicksInCode)
        {
            return;
        }

        if (rotateButton != null) rotateButton.onClick.AddListener(OnRotateClick);
        if (confirmButton != null) confirmButton.onClick.AddListener(OnConfirmClick);
        if (cancelButton != null) cancelButton.onClick.AddListener(OnCancelClick);
    }

    private void OnDestroy()
    {
        if (!bindButtonClicksInCode)
        {
            return;
        }

        if (rotateButton != null) rotateButton.onClick.RemoveListener(OnRotateClick);
        if (confirmButton != null) confirmButton.onClick.RemoveListener(OnConfirmClick);
        if (cancelButton != null) cancelButton.onClick.RemoveListener(OnCancelClick);
    }

    private void Update()
    {
        if (targetTransform == null || floatingPanel == null || !floatingPanel.gameObject.activeSelf)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 screenPos = mainCamera.WorldToScreenPoint(targetTransform.position + worldOffset);
        floatingPanel.position = screenPos;
    }

    public void ShowMenu(Transform target)
    {
        targetTransform = target;

        if (floatingPanel != null)
        {
            floatingPanel.gameObject.SetActive(true);
        }
    }

    public void HideMenu()
    {
        targetTransform = null;

        if (floatingPanel != null)
        {
            floatingPanel.gameObject.SetActive(false);
        }
    }

    public void OnRotateClick()
    {
        _onRotate?.Invoke();
    }

    public void OnConfirmClick()
    {
        _onConfirm?.Invoke();
    }

    public void OnCancelClick()
    {
        _onCancel?.Invoke();
    }

    public void BindActions(Action onRotate, Action onConfirm, Action onCancel)
    {
        _onRotate = onRotate;
        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }
}
