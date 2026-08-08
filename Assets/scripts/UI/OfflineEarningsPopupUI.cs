using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    /// Shows the "welcome back" popup when offline earnings are credited on load
    public class OfflineEarningsPopupUI : MonoBehaviour
    {
        [SerializeField] GameObject overlay;
        [SerializeField] TextMeshProUGUI messageText;
        [SerializeField] Button collectButton;

        void Awake()
        {
            if (collectButton != null)
                collectButton.onClick.AddListener(Hide);
            if (overlay != null)
                overlay.SetActive(false);
        }

        void OnEnable() => GameEvents.OfflineEarnings += Show;
        void OnDisable() => GameEvents.OfflineEarnings -= Show;

        void Show(double amount, double seconds)
        {
            if (overlay == null) return;

            if (messageText != null)
            {
                string currencyName = GameManager.Instance != null
                    ? GameManager.Instance.Balance.currencyName : "coins";
                messageText.text =
                    $"While you were away for {NumberFormatter.FormatDuration(seconds)},\n" +
                    $"your generators produced\n<b>{NumberFormatter.Format(amount)} {currencyName}</b>!";
            }

            overlay.transform.SetAsLastSibling(); // draw over everything
            overlay.SetActive(true);
        }

        public void Hide()
        {
            if (overlay != null) overlay.SetActive(false);
        }
    }
}
