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
                _totalCostText.color = currentCost > targetCost ? Color.red : Color.white;
            }

            if (_totalSafetyText != null)
                _totalSafetyText.text = $"安全: {currentSafety}/{targetSafety}";

            if (_totalAestheticsText != null)
                _totalAestheticsText.text = $"美观: {currentAesthetics}/{targetAesthetics}";

            if (_totalEnvironmentText != null)
                _totalEnvironmentText.text = $"环境: {currentEnvironment}/{targetEnvironment}";

            if (_totalComfortText != null)
                _totalComfortText.text = $"舒适: {currentComfort}/{targetComfort}";
        }
    }
}

