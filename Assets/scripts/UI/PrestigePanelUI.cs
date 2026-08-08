using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    /// Shows pearl count/bonus and the prestige button.
    public class PrestigePanelUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI infoText;
        [SerializeField] Button prestigeButton;
        [SerializeField] TextMeshProUGUI buttonLabel;

        float nextRefresh;

        void Awake()
        {
            if (prestigeButton != null)
                prestigeButton.onClick.AddListener(OnPrestigeClicked);
        }

        void OnPrestigeClicked()
        {
            var gm = GameManager.Instance;
            if (gm != null) gm.Prestige.TryPrestige();
        }

        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.2f;

            var gm = GameManager.Instance;
            if (gm == null || infoText == null) return;

            var prestige = gm.Prestige;
            var balance = gm.Balance;

            double bonusPercent = prestige.Pearls * balance.pearlBonusPerUnit * 100.0;
            infoText.text =
                $"{balance.premiumCurrencyName}: {NumberFormatter.Format(prestige.Pearls)}  (+{bonusPercent:0}% production)\n" +
                $"<size=70%>All-time: {NumberFormatter.Format(gm.Currency.TotalLifetime)} {balance.currencyName.ToLower()}</size>";

            double pending = prestige.PendingGain;
            if (pending >= 1)
            {
                buttonLabel.text = $"Prestige\n+{NumberFormatter.Format(pending)} {balance.premiumCurrencyName}";
                prestigeButton.interactable = true;
            }
            else
            {
                double needed = prestige.RequirementFor(1);
                buttonLabel.text = $"Prestige\n<size=65%>at {NumberFormatter.Format(needed)} lifetime</size>";
                prestigeButton.interactable = false;
            }
        }
    }
}
