using System.Collections;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    public class CameraController : MonoBehaviour
    {
        private Vector3 _groundCamOffset;
        private Camera _sceneCamera;

        private bool _startedHoldingOverUI;
        private bool _isInPlacementState;

        private GameConfig _config;

        // [Inspector 调整] 手机端灵敏度
        [Header("Mobile Controls")]
        [SerializeField] private float mobilePanSensitivity = 0.02f;
        [SerializeField] private float mobileZoomSensitivity = 0.01f;

        // 记录上一帧双指距离
        private float _lastPinchDistance;

        private void Start()
        {
            _config = GameConfig.Instance;
            _sceneCamera = GetComponent<Camera>();

            // 确保是正交相机，如果不是给予警告
            if (!_sceneCamera.orthographic)
            {
                Debug.LogWarning("CameraController: 检测到当前不是正交相机(Orthographic)，请在Inspector中修改 Camera -> Projection 为 Orthographic！");
            }

            var groundPos = GetWorldPosAtViewportPoint(0.5f, 0.5f);
            _groundCamOffset = _sceneCamera.transform.position - groundPos;

            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLmbRelease += ResetUIHold;
                InputManager.Instance.OnMmbDrag += HandleDrag;
                InputManager.Instance.OnRmbDrag += HandleRotation;
                InputManager.Instance.OnScroll += HandleZoom;
                InputManager.Instance.OnMouseAtScreenCorner += HandleMouseAtScreenCorner;
            }

            if (PlacementSystem.Instance != null)
            {
                PlacementSystem.Instance.OnPlacementStateStart += PlacementStateActivated;
                PlacementSystem.Instance.OnPlacementStateEnd += PlacementStateEnded;
            }
        }

        private void OnDestroy()
        {
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLmbRelease -= ResetUIHold;
                InputManager.Instance.OnMmbDrag -= HandleDrag;
                InputManager.Instance.OnRmbDrag -= HandleRotation;
                InputManager.Instance.OnScroll -= HandleZoom;
                InputManager.Instance.OnMouseAtScreenCorner -= HandleMouseAtScreenCorner;
            }
            if (PlacementSystem.Instance != null)
            {
                PlacementSystem.Instance.OnPlacementStateStart -= PlacementStateActivated;
                PlacementSystem.Instance.OnPlacementStateEnd -= PlacementStateEnded;
            }
        }

        private void Update()
        {
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            if (Input.touchCount == 0) return;

            // --- 1. 单指操作：平移 ---
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    _startedHoldingOverUI = IsPointerOverUI(touch.fingerId);
                }

                if (_startedHoldingOverUI) return;

                if (touch.phase == TouchPhase.Moved)
                {
                    // 平移逻辑对正交相机依然有效（移动 X 和 Z）
                    HandleDrag(touch.deltaPosition * mobilePanSensitivity);
                }
            }
            // --- 2. 双指操作：缩放 ---
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    if (IsPointerOverUI(touch0.fingerId) || IsPointerOverUI(touch1.fingerId))
                    {
                        _startedHoldingOverUI = true;
                        return;
                    }
                    else
                    {
                        _startedHoldingOverUI = false;
                        _lastPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                    }
                }

                if (_startedHoldingOverUI) return;

                if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                    // 计算差值：正数=放大(张开)，负数=缩小(捏合)
                    float distanceDelta = currentDistance - _lastPinchDistance;

                    if (Mathf.Abs(distanceDelta) > 1.0f)
                    {
                        // 传入差值
                        HandleZoom(distanceDelta * mobileZoomSensitivity);
                    }

                    _lastPinchDistance = currentDistance;
                }
            }
            else
            {
                _startedHoldingOverUI = false;
            }
        }

        private bool IsPointerOverUI(int fingerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        private Vector3 GetWorldPosAtViewportPoint(float vx, float vy)
        {
            var worldRay = _sceneCamera.ViewportPointToRay(new Vector3(vx, vy, 0));
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            groundPlane.Raycast(worldRay, out var distanceToGround);
            return worldRay.GetPoint(distanceToGround);
        }

        private void ResetUIHold()
        {
            _startedHoldingOverUI = false;
        }

        private void HandleDrag(Vector2 mouseDelta)
        {
            if (_startedHoldingOverUI) return;

            var xBoundary = _config.XBoundary;
            var zBoundary = _config.ZBoundary;

            // 根据正交相机的 Size 动态调整拖拽速度
            // 当视野很大(Size大)时，拖动速度应该快；视野很小(Size小)时，拖动应该慢
            float dragFactor = _sceneCamera.orthographic ? (_sceneCamera.orthographicSize / 5f) : 1f;

            var moveDirection = transform.right * mouseDelta.x + transform.forward * mouseDelta.y;
            moveDirection.y = 0;

            var newPosition = transform.position - moveDirection * (_config.DragSpeed * dragFactor * Time.deltaTime);

            newPosition.x = Mathf.Clamp(newPosition.x, xBoundary.x, xBoundary.y);
            newPosition.z = Mathf.Clamp(newPosition.z, zBoundary.x, zBoundary.y);

            transform.position = newPosition;
        }

        private void HandleRotation(Vector2 mouseDelta)
        {
            var rotationSpeed = _config.RotationSpeed;
            transform.Rotate(Vector3.up, mouseDelta.x * rotationSpeed * Time.deltaTime, Space.World);
        }

        private void HandleMouseAtScreenCorner(Vector2 direction)
        {
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
            return; 
#endif
            if (_config.EnableAutoMove == false) return;
            var xBoundary = _config.XBoundary;
            var zBoundary = _config.ZBoundary;
            if (_config.RestrictAutoMoveForPlacement && !_isInPlacementState) return;

            var newPosition = transform.position + new Vector3(direction.x, 0, direction.y) * (_config.DragSpeed * Time.deltaTime);
            newPosition.x = Mathf.Clamp(newPosition.x, xBoundary.x, xBoundary.y);
            newPosition.z = Mathf.Clamp(newPosition.z, zBoundary.x, zBoundary.y);
            transform.position = newPosition;
        }

        /// <summary>
        /// [核心修改] 正交相机缩放逻辑
        /// 修改 orthographicSize 而不是 position
        /// </summary>
        private void HandleZoom(float scrollDelta)
        {
            if (scrollDelta == 0 || !_sceneCamera) return;

            // 1. 如果不是正交相机，不执行此逻辑
            if (!_sceneCamera.orthographic) return;

            // 2. 计算新的 Size
            // scrollDelta > 0 (滚轮向前/手指张开) -> 想要放大 -> Size 应该变小
            // scrollDelta < 0 (滚轮向后/手指捏合) -> 想要缩小 -> Size 应该变大
            // 所以这里用 减号
            float currentSize = _sceneCamera.orthographicSize;
            float targetSize = currentSize - (scrollDelta * _config.ZoomSpeed * 0.5f);

            // 3. 限制 Size 的范围
            // 利用 GameConfig 的 YBoundary 作为 Size 的范围
            // X = Min Size (放大极限), Y = Max Size (缩小极限)
            // 建议在 Config 中设置 X=2, Y=15 左右
            targetSize = Mathf.Clamp(targetSize, _config.YBoundary.x, _config.YBoundary.y);

            // 4. 应用
            _sceneCamera.orthographicSize = targetSize;
        }

        private void PlacementStateActivated()
        {
            _isInPlacementState = true;
        }

        private void PlacementStateEnded()
        {
            _isInPlacementState = false;
        }

        public void FocusOnPosition(Vector3 target, float duration)
        {
            StartCoroutine(CameraUpdateCoroutine(target, duration));
        }

        private IEnumerator CameraUpdateCoroutine(Vector3 target, float duration)
        {
            var startPosition = _sceneCamera.transform.position;
            var endPosition = target + _groundCamOffset;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                var t = Mathf.Clamp01(elapsedTime / duration);
                t = Mathf.SmoothStep(0f, 1f, t);
                _sceneCamera.transform.position = Vector3.Lerp(startPosition, endPosition, t);
                yield return null;
            }
        }
    }
}