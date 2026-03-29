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
                _totalCostText.color = currentCost > targetCost ? Color.red : Color.green;
            }

            if (_totalSafetyText != null)
            {
                _totalSafetyText.text = $"安全: {currentSafety}/{targetSafety}";
                // Safety 越大越好：未达标红色，达标绿色
                _totalSafetyText.color = currentSafety < targetSafety ? Color.red : Color.green;
            }

            if (_totalAestheticsText != null)
            {
                _totalAestheticsText.text = $"美观: {currentAesthetics}/{targetAesthetics}";
                // Aesthetics 越大越好：未达标红色，达标绿色
                _totalAestheticsText.color = currentAesthetics < targetAesthetics ? Color.red : Color.green;
            }

            if (_totalEnvironmentText != null)
            {
                _totalEnvironmentText.text = $"环境: {currentEnvironment}/{targetEnvironment}";
                // Environment 越大越好：未达标红色，达标绿色
                _totalEnvironmentText.color = currentEnvironment < targetEnvironment ? Color.red : Color.green;
            }

            if (_totalComfortText != null)
            {
                _totalComfortText.text = $"舒适: {currentComfort}/{targetComfort}";
                // Comfort 越大越好：未达标红色，达标绿色
                _totalComfortText.color = currentComfort < targetComfort ? Color.red : Color.green;
            }
        }
    }
}

