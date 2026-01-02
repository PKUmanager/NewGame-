using System.Collections;
using SpaceFusion.SF_Grid_Building_System.Scripts.Managers;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems; // [新增]用于检测是否点击在UI上

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Core
{
    /// <summary>
    /// 修改版：增加了手机触控支持 (单指平移，双指缩放/旋转)
    /// </summary>
    public class CameraController : MonoBehaviour
    {
        private Vector3 _groundCamOffset;
        private Camera _sceneCamera;

        private bool _startedHoldingOverUI;
        private bool _isInPlacementState;

        private GameConfig _config;

        // [新增] 记录上一次双指的距离和角度，用于计算增量
        private float _lastPinchDistance;
        private float _lastTurnAngle;

        private void Start()
        {
            _config = GameConfig.Instance;
            _sceneCamera = GetComponent<Camera>();
            var groundPos = GetWorldPosAtViewportPoint(0.5f, 0.5f);
            _groundCamOffset = _sceneCamera.transform.position - groundPos;

            // 保持原有的PC端事件绑定
            if (InputManager.Instance != null)
            {
                InputManager.Instance.OnLmbRelease += ResetUIHold;
                InputManager.Instance.OnMmbDrag += HandleDrag; // PC: 中键平移
                InputManager.Instance.OnRmbDrag += HandleRotation; // PC: 右键旋转
                InputManager.Instance.OnScroll += HandleZoom; // PC: 滚轮缩放
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

        // [核心新增] 每帧检测触摸输入
        private void Update()
        {
            HandleTouchInput();
        }

        private void HandleTouchInput()
        {
            // 如果没有触摸，直接返回
            if (Input.touchCount == 0) return;

            // --- 1. 单指操作：平移 (Pan) ---
            if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);

                // 开始触摸时，检查是否按在了UI上
                if (touch.phase == TouchPhase.Began)
                {
                    _startedHoldingOverUI = IsPointerOverUI(touch.fingerId);
                }

                if (_startedHoldingOverUI) return;

                // 只有手指移动时才执行平移
                if (touch.phase == TouchPhase.Moved)
                {
                    // touch.deltaPosition 是像素距离，我们需要根据 DragSpeed 调整
                    // 注意：手机屏幕DPI较高，通常需要乘以一个灵敏度系数，这里简单复用 DragSpeed
                    // PC上的 mouseDelta 也是像素差，所以逻辑可以通用，但手机滑动通常比较快，可能需要微调系数
                    float mobileSensitivity = 0.5f;
                    HandleDrag(touch.deltaPosition * mobileSensitivity);
                }
            }

            // --- 2. 双指操作：缩放 (Zoom) & 旋转 (Rotate) ---
            else if (Input.touchCount == 2)
            {
                Touch touch0 = Input.GetTouch(0);
                Touch touch1 = Input.GetTouch(1);

                // 检查是否点在UI上
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
                    }
                }

                if (_startedHoldingOverUI) return;

                // 计算当前的距离和角度
                float currentDistance = Vector2.Distance(touch0.position, touch1.position);
                float currentAngle = Vector2.SignedAngle(touch1.position - touch0.position, Vector2.right);

                // 如果是双指刚放下的瞬间，初始化记录值
                if (touch0.phase == TouchPhase.Began || touch1.phase == TouchPhase.Began)
                {
                    _lastPinchDistance = currentDistance;
                    _lastTurnAngle = currentAngle;
                }
                else if (touch0.phase == TouchPhase.Moved || touch1.phase == TouchPhase.Moved)
                {
                    // A. 处理缩放
                    float distanceDelta = currentDistance - _lastPinchDistance;
                    // 手机像素距离较大，缩小一点系数以获得平滑缩放
                    float zoomSensitivity = 0.05f;
                    if (Mathf.Abs(distanceDelta) > 0.1f)
                    {
                        HandleZoom(distanceDelta * zoomSensitivity);
                    }

                    // B. 处理旋转
                    float angleDelta = Mathf.DeltaAngle(currentAngle, _lastTurnAngle); // 注意参数顺序
                    float rotationSensitivity = 2.0f; // 旋转灵敏度
                    // 只有当旋转角度超过一定阈值才旋转，防止误触
                    if (Mathf.Abs(angleDelta) > 0.5f)
                    {
                        // 复用PC的旋转逻辑，PC逻辑接收 Vector2 delta
                        // 我们构造一个 (x=旋转量, y=俯仰量) 的向量。这里暂时只做水平旋转(Y轴)
                        // 因为手机双指很难同时控制俯仰和旋转，体验不好。这里只允许水平旋转。
                        HandleRotation(new Vector2(angleDelta * rotationSensitivity, 0));
                    }

                    // 更新记录值
                    _lastPinchDistance = currentDistance;
                    _lastTurnAngle = currentAngle;
                }
            }
            else
            {
                // 手指抬起，重置UI状态
                _startedHoldingOverUI = false;
            }
        }

        // [辅助方法] 检测触摸点是否在UI上
        private bool IsPointerOverUI(int fingerId)
        {
            return EventSystem.current != null && EventSystem.current.IsPointerOverGameObject(fingerId);
        }

        // --- 以下逻辑保持原样 ---

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
            if (_startedHoldingOverUI)
            {
                return;
            }

            var xBoundary = _config.XBoundary;
            var zBoundary = _config.ZBoundary;

            // [注意] 手机滑动方向与相机移动方向通常是相反的（拖拽地图的感觉）
            // 如果你觉得方向反了，把下面的减号改成加号
            // PC逻辑是：transform.right * x + transform.forward * y

            var moveDirection = transform.right * mouseDelta.x + transform.forward * mouseDelta.y;
            moveDirection.y = 0;

            // 使用配置的 DragSpeed
            var newPosition = transform.position - moveDirection * (_config.DragSpeed * Time.deltaTime);

            newPosition.x = Mathf.Clamp(newPosition.x, xBoundary.x, xBoundary.y);
            newPosition.z = Mathf.Clamp(newPosition.z, zBoundary.x, zBoundary.y);
            transform.position = newPosition;
        }

        private void HandleRotation(Vector2 mouseDelta)
        {
            var rotationSpeed = _config.RotationSpeed;
            // Y轴旋转 (左右转)
            transform.Rotate(Vector3.up, mouseDelta.x * rotationSpeed * Time.deltaTime, Space.World);

            // X轴旋转 (俯仰) - 如果你在手机上也想支持俯仰，可以把 touch 逻辑里的 y 传进来
            // var angle = -mouseDelta.y * rotationSpeed * Time.deltaTime;
            // var rightAxis = transform.right; 
            // transform.Rotate(rightAxis, angle, Space.World);
        }

        private void HandleMouseAtScreenCorner(Vector2 direction)
        {
            if (_config.EnableAutoMove == false) return;
            // 手机上通常不需要边缘自动移动，这里可以加个宏判断或者保留
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
                return; 
#endif

            var xBoundary = _config.XBoundary;
            var zBoundary = _config.ZBoundary;
            if (_config.RestrictAutoMoveForPlacement && !_isInPlacementState)
            {
                return;
            }

            var newPosition = transform.position + new Vector3(direction.x, 0, direction.y) * (_config.DragSpeed * Time.deltaTime);
            newPosition.x = Mathf.Clamp(newPosition.x, xBoundary.x, xBoundary.y);
            newPosition.z = Mathf.Clamp(newPosition.z, zBoundary.x, zBoundary.y);
            transform.position = newPosition;
        }

        private void HandleZoom(float scrollDelta)
        {
            if (scrollDelta == 0 || !_sceneCamera) return;

            // 限制缩放范围
            if (scrollDelta > 0 && _sceneCamera.transform.position.y <= _config.YBoundary.x) return;
            if (scrollDelta < 0 && _sceneCamera.transform.position.y >= _config.YBoundary.y) return;

            // 手机端没有鼠标位置，我们默认向屏幕中心缩放
            // 或者使用 _sceneCamera.transform.forward 方向
            Vector3 zoomDirection;

#if UNITY_EDITOR || UNITY_STANDALONE
            // PC: 向鼠标位置缩放
            var mouseRay = _sceneCamera.ScreenPointToRay(Input.mousePosition);
            var groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(mouseRay, out var distance)) {
                var mouseWorldPos = mouseRay.GetPoint(distance);
                zoomDirection = (mouseWorldPos - _sceneCamera.transform.position).normalized;
            } else {
                zoomDirection = _sceneCamera.transform.forward;
            }
#else
            // Mobile: 向屏幕中心缩放
            zoomDirection = _sceneCamera.transform.forward;
#endif

            var newPosition = _sceneCamera.transform.position + zoomDirection * (scrollDelta * _config.ZoomSpeed);
            _sceneCamera.transform.position = newPosition;
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