using System;
using SpaceFusion.SF_Grid_Building_System.Scripts.Utils;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers
{
    public class InputManager : MonoBehaviour
    {
        public static InputManager Instance;

        public event Action OnClicked;
        public event Action OnExit;
        public event Action OnRotate;
        public event Action<Vector2> OnLmbPress;
        public event Action<Vector2> OnLmbHold;
        public event Action OnLmbRelease;
        public event Action<Vector2> OnMmbDrag;
        public event Action<Vector2> OnRmbDrag;
        public event Action<float> OnScroll;
        public event Action<Vector2> OnMouseAtScreenCorner;

        private bool _isHolding;
        private float _holdTimer;
        private Camera _sceneCamera;
        private Vector2 _screenSize;
        private Vector2 _lastMousePositionMmb;
        private Vector2 _lastMousePositionRmb;

        // 缓存最后一次有效的触摸位置
        private Vector2 _lastValidScreenPosition;

        // ★★★ 新增标记：记录这次点击是否始于 UI ★★★
        private bool _clickStartedOnUI = false;

        private LayerMask _placementLayerMask;
        private float _holdThreshold;
        private float _edgeMarginForAutoMove;

        // 定义无效位置
        public static readonly Vector3 InvalidPosition = Vector3.negativeInfinity;

        private void Awake()
        {
            if (Instance != null) Destroy(gameObject);
            Instance = this;
            _screenSize = new Vector2(Screen.width, Screen.height);
        }

        private void Start()
        {
            var config = GameConfig.Instance;
            if (config != null)
            {
                _placementLayerMask = config.PlacementLayerMask;
                _holdThreshold = config.HoldThreshold;
                _edgeMarginForAutoMove = config.EdgeMarginForAutoMove;
            }
            _sceneCamera = GameManager.Instance.SceneCamera;
        }

        private void Update()
        {
            // 1. 更新屏幕坐标
            if (Input.touchCount > 0)
            {
                _lastValidScreenPosition = Input.GetTouch(0).position;
            }
            else
            {
                // 仅在非移动平台更新鼠标位置，防止手机抬手后坐标归零
                if (!Application.isMobilePlatform)
                {
                    _lastValidScreenPosition = Input.mousePosition;
                }
            }

            // 2. 按下逻辑 (Down)
            if (Input.GetMouseButtonDown(0))
            {
                // ★★★ 核心检查：按下的瞬间，是否在 UI 上？ ★★★
                if (IsPointerOverUIObject())
                {
                    _clickStartedOnUI = true; // 标记：这次操作是属于 UI 的
                }
                else
                {
                    _clickStartedOnUI = false; // 标记：这次操作是属于场景的
                    _isHolding = true;
                    _holdTimer = 0;
                    OnLmbPress?.Invoke(_lastValidScreenPosition);
                }
            }

            // 3. 按住逻辑 (Hold)
            if (_isHolding)
            {
                // 如果这次操作是 UI 操作，直接跳过场景逻辑
                if (_clickStartedOnUI)
                {
                    _isHolding = false;
                }
                else
                {
                    _holdTimer += Time.deltaTime;
                    if (_holdTimer > _holdThreshold)
                    {
                        OnLmbHold?.Invoke(_lastValidScreenPosition);
                    }
                }
            }

            // 4. 抬起逻辑 (Up)
            if (Input.GetMouseButtonUp(0))
            {
                // ★★★ 核心检查：抬起的瞬间 ★★★
                // 只有当：(1) 按下时不在UI上 AND (2) 抬起时也不在UI上
                // 才触发场景点击事件
                if (!_clickStartedOnUI && !IsPointerOverUIObject())
                {
                    OnLmbRelease?.Invoke();

                    if (_holdTimer <= _holdThreshold)
                    {
                        OnClicked?.Invoke();
                    }
                }

                // 重置状态
                _isHolding = false;
                _holdTimer = 0;
                _clickStartedOnUI = false;
            }

            // 快捷键和辅助操作保持不变
            if (Input.GetKeyDown(KeyCode.Escape)) OnExit?.Invoke();
            if (Input.GetKeyDown(KeyCode.R)) OnRotate?.Invoke();

            if (Input.GetMouseButtonDown(2)) _lastMousePositionMmb = Input.mousePosition;
            if (Input.GetMouseButton(2))
            {
                Vector2 delta = (Vector2)Input.mousePosition - _lastMousePositionMmb;
                OnMmbDrag?.Invoke(delta);
                _lastMousePositionMmb = Input.mousePosition;
            }

            if (Input.GetMouseButtonDown(1)) _lastMousePositionRmb = Input.mousePosition;
            if (Input.GetMouseButton(1))
            {
                Vector2 delta = (Vector2)Input.mousePosition - _lastMousePositionRmb;
                OnRmbDrag?.Invoke(delta);
                _lastMousePositionRmb = Input.mousePosition;
            }

            if (Math.Abs(Input.mouseScrollDelta.y) > 0f) OnScroll?.Invoke(Input.mouseScrollDelta.y);

            HandleMouseAtScreenCorner();
        }

        private void HandleMouseAtScreenCorner()
        {
            Vector2 mousePos = Input.mousePosition;
            if ((mousePos.x > _edgeMarginForAutoMove) &&
                (mousePos.x < _screenSize.x - _edgeMarginForAutoMove) &&
                (mousePos.y > _edgeMarginForAutoMove) &&
                (mousePos.y < _screenSize.y - _edgeMarginForAutoMove))
            {
                return;
            }
            var screenCenter = new Vector2(_screenSize.x / 2, _screenSize.y / 2);
            var direction = (mousePos - screenCenter).normalized;
            OnMouseAtScreenCorner?.Invoke(direction);
        }

        public static bool IsPointerOverUIObject()
        {
            if (EventSystem.current == null) return false;

            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    // 包含所有触摸状态，确保任何时候碰到UI都返回true
                    if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return true;
                }
            }

            if (EventSystem.current.IsPointerOverGameObject()) return true;
            return false;
        }

        public Vector3 GetSelectedMapPosition()
        {
            // 1. 如果手指在 UI 上，返回无效
            if (IsPointerOverUIObject()) return InvalidPosition;

            // 2. ★★★ 如果这次操作本身就是从 UI 开始的（比如按住摇杆滑到了外面），也视为无效 ★★★
            if (_clickStartedOnUI) return InvalidPosition;

            // 3. 手机端如果完全没有触摸，返回无效
            if (Application.isMobilePlatform && Input.touchCount == 0) return InvalidPosition;

            if (_sceneCamera == null) _sceneCamera = Camera.main;

            Vector3 screenPos = _lastValidScreenPosition;
            screenPos.z = _sceneCamera.nearClipPlane;

            var ray = _sceneCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 1000f, _placementLayerMask))
            {
                return hit.point;
            }

            return InvalidPosition;
        }
    }
}