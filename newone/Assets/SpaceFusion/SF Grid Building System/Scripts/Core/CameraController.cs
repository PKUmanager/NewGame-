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

        // [新增] 记录双指上一帧的距离
        private float _lastPinchDistance;

        private void Start()
        {
            _config = GameConfig.Instance;
            _sceneCamera = GetComponent<Camera>();
            var groundPos = GetWorldPosAtViewportPoint(0.5f, 0.5f);
            _groundCamOffset = _sceneCamera.transform.position - groundPos;

            // 绑定 PC 端输入事件
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLmbRelease += ResetUIHold;
                InputManager.Instance.OnMmbDrag += HandleDrag;      // 中键平移
                InputManager.Instance.OnRmbDrag += HandleRotation;  // 右键旋转
                InputManager.Instance.OnScroll += HandleZoom;       // [PC] 滚轮缩放
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

        /// <summary>
        /// 处理手机触摸逻辑
        /// </summary>
        private void HandleTouchInput()
        {
            // 如果没有触摸，直接返回
            if (Input.touchCount == 0) return;

            // --- 情况1：单指操作 -> 平移 (Pan) ---
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
                    // 手机灵敏度系数，根据手感调整
                    float mobilePanSensitivity = 0.01f;
                    // 反转 delta 使得手指往左滑相机往右看（符合拖拽直觉）
                    HandleDrag(-touch.deltaPosition * mobilePanSensitivity);
                }
            }

            // --- 情况2：双指操作 -> 缩放 (Pinch to Zoom) ---
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                // 只要有一根手指按在UI上，就不处理
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
                        // 初始化距离
                        _lastPinchDistance = Vector2.Distance(touch0.position, touch1.position);
                    }
                }

                if (_startedHoldingOverUI) return;

                // 计算当前帧的距离
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);

                if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    // 计算距离差值 (当前距离 - 上一帧距离)
                    // 正数 = 手指张开 = 放大(拉近)
                    // 负数 = 手指捏合 = 缩小(拉远)
                    float distanceDelta = currentDistance - _lastPinchDistance;

                    // 手机像素密度高，差值通常很大，需要一个很小的系数来缩小
                    float mobileZoomSensitivity = 0.01f;

                    // 只有差值够大才执行，防抖动
                    if (Mathf.Abs(distanceDelta) > 1.0f)
                    {
                        HandleZoom(distanceDelta * mobileZoomSensitivity);
                    }

                    // 更新上一帧距离，为下一次计算做准备
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

            // 计算平移方向
            var moveDirection = transform.right * mouseDelta.x + transform.forward * mouseDelta.y;
            moveDirection.y = 0; // 保证平移不改变高度

            var newPosition = transform.position - moveDirection * (_config.DragSpeed * Time.deltaTime);

            // 简单的边界限制
            newPosition.x = Mathf.Clamp(newPosition.x, xBoundary.x, xBoundary.y);
            newPosition.z = Mathf.Clamp(newPosition.z, zBoundary.x, zBoundary.y);

            transform.position = newPosition;
        }

        private void HandleRotation(Vector2 mouseDelta)
        {
            var rotationSpeed = _config.RotationSpeed;
            transform.Rotate(Vector3.up, mouseDelta.x * rotationSpeed * Time.deltaTime, Space.World);
        }

        // [核心逻辑] 统一处理 PC 和 手机 的缩放
        // delta > 0 : 拉近 (Zoom In)
        // delta < 0 : 拉远 (Zoom Out)
        private void HandleZoom(float scrollDelta)
        {
            if (scrollDelta == 0 || !_sceneCamera) return;

            // 1. 计算想要移动的方向：永远沿着相机正前方
            Vector3 zoomDirection = _sceneCamera.transform.forward;

            // 2. 计算新位置
            Vector3 newPosition = _sceneCamera.transform.position + zoomDirection * (scrollDelta * _config.ZoomSpeed);

            // 3. 检查高度限制 (Y Boundary)
            // 防止缩放穿过地板，或者拉得太远
            // 注意：Config里的 X通常是最小值(MinHeight), Y是最大值(MaxHeight)
            if (newPosition.y < _config.YBoundary.x && scrollDelta > 0)
            {
                // 如果已经到底了，就不允许再拉近
                return;
            }
            if (newPosition.y > _config.YBoundary.y && scrollDelta < 0)
            {
                // 如果已经到顶了，就不允许再拉远
                return;
            }

            // 4. 应用位置
            _sceneCamera.transform.position = newPosition;
        }

        private void HandleMouseAtScreenCorner(Vector2 direction)
        {
            // 手机端禁用边缘移动
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