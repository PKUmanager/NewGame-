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

        // 缓存：即使手指抬起，也记住最后一次手指在屏幕上的坐标
        private Vector2 _lastValidScreenPosition;

        private LayerMask _placementLayerMask;
        private float _holdThreshold;
        private float _edgeMarginForAutoMove;

        // 无效位置标记
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
            // 1. 实时更新屏幕坐标缓存
            // 只有当有手指触摸或者鼠标存在时才更新
            if (Input.touchCount > 0)
            {
                _lastValidScreenPosition = Input.GetTouch(0).position;
            }
            else
            {
                _lastValidScreenPosition = Input.mousePosition;
            }

            // 2. 点击事件分发
            if (Input.GetMouseButtonDown(0))
            {
                _isHolding = true;
                _holdTimer = 0;
                OnLmbPress?.Invoke(_lastValidScreenPosition);
            }

            if (_isHolding)
            {
                _holdTimer += Time.deltaTime;
                if (_holdTimer > _holdThreshold)
                {
                    OnLmbHold?.Invoke(_lastValidScreenPosition);
                }
            }

            if (Input.GetMouseButtonUp(0))
            {
                OnLmbRelease?.Invoke();

                if (_holdTimer <= _holdThreshold)
                {
                    OnClicked?.Invoke();
                }

                _isHolding = false;
                _holdTimer = 0;
            }

            // 电脑端快捷键保留
            if (Input.GetKeyDown(KeyCode.Escape)) OnExit?.Invoke();
            if (Input.GetKeyDown(KeyCode.R)) OnRotate?.Invoke();

            // 辅助操作（中键/右键）
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

        // ★★★ 手机端关键：准确的 UI 遮挡检测 ★★★
        public static bool IsPointerOverUIObject()
        {
            // 1. 检查 EventSystem 是否存在
            if (EventSystem.current == null) return false;

            // 2. 检查触摸输入 (针对手机)
            if (Input.touchCount > 0)
            {
                for (int i = 0; i < Input.touchCount; i++)
                {
                    Touch touch = Input.GetTouch(i);
                    // 只要有一个手指按在 UI 上，就视为 UI 操作
                    if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary || touch.phase == TouchPhase.Moved)
                    {
                        if (EventSystem.current.IsPointerOverGameObject(touch.fingerId)) return true;
                    }
                }
            }

            // 3. 检查鼠标输入 (针对电脑/模拟器)
            if (EventSystem.current.IsPointerOverGameObject()) return true;

            return false;
        }

        public Vector3 GetSelectedMapPosition()
        {
            // ★★★ 保护逻辑 ★★★
            // 如果你在操作 UI，我直接返回无效位置。
            // PlacementSystem 收到无效位置后，会停止更新，让物体停留在上一次的地方。
            if (IsPointerOverUIObject()) return InvalidPosition;

            if (_sceneCamera == null) _sceneCamera = Camera.main;

            Vector3 screenPos = _lastValidScreenPosition;
            screenPos.z = _sceneCamera.nearClipPlane;

            var ray = _sceneCamera.ScreenPointToRay(screenPos);

            if (Physics.Raycast(ray, out var hit, 1000f, _placementLayerMask))
            {
                return hit.point;
            }

            // 没打中地板，也返回无效位置
            return InvalidPosition;
        }
    }
}