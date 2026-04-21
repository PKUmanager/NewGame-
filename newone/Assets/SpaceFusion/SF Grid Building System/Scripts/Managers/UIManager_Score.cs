using TMPro;
using UnityEngine;

namespace SpaceFusion.SF_Grid_Building_System.Scripts.Managers
{
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance;

        [Header("总统计 UI")]
        [SerializeField] private TextMeshProUGUI _totalCostText;
        [SerializeField] private TextMeshProUGUI _totalSafetyText;
        [SerializeField] private TextMeshProUGUI _totalAestheticsText;
        [SerializeField] private TextMeshProUGUI _totalEnvironmentText;
        [SerializeField] private TextMeshProUGUI _totalComfortText;
        
        [Header("达标颜色（低饱和）")]
        [SerializeField] private Color _passColor = new Color(0.58f, 0.76f, 0.58f, 1f);
        [SerializeField] private Color _failColor = new Color(0.86f, 0.60f, 0.60f, 1f);

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        public void UpdateTotalStatsUI(
            int currentCost,
            int currentSafety,
            int currentAesthetics,
            int currentEnvironment,
            int currentComfort,
            int targetCost,
            int targetSafety,
            int targetAesthetics,
            int targetEnvironment,
            int targetComfort)
        {
            if (_totalCostText != null)
            {
                _totalCostText.text = $"成本: {currentCost}/{targetCost}";
                // Cost 越大越糟：超过目标红色，否则绿色
                _totalCostText.color = currentCost > targetCost ? _failColor : _passColor;
            }

            if (_totalSafetyText != null)
            {
                _totalSafetyText.text = $"安全: {currentSafety}/{targetSafety}";
                // Safety 越大越好：未达标红色，达标绿色
                _totalSafetyText.color = currentSafety < targetSafety ? _failColor : _passColor;
            }

            if (_totalAestheticsText != null)
            {
                _totalAestheticsText.text = $"美观: {currentAesthetics}/{targetAesthetics}";
                // Aesthetics 越大越好：未达标红色，达标绿色
                _totalAestheticsText.color = currentAesthetics < targetAesthetics ? _failColor : _passColor;
            }

            if (_totalEnvironmentText != null)
            {
                _totalEnvironmentText.text = $"环境: {currentEnvironment}/{targetEnvironment}";
                // Environment 越大越好：未达标红色，达标绿色
                _totalEnvironmentText.color = currentEnvironment < targetEnvironment ? _failColor : _passColor;
            }

            if (_totalComfortText != null)
            {
                _totalComfortText.text = $"舒适: {currentComfort}/{targetComfort}";
                // Comfort 越大越好：未达标红色，达标绿色
                _totalComfortText.color = currentComfort < targetComfort ? _failColor : _passColor;
            }
        }
    }
}

