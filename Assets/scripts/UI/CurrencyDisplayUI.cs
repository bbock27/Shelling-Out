using TMPro;
using UnityEngine;

namespace ShellingOut
{
    /// Header readout: current currency and production per second.
    public class CurrencyDisplayUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI amountText;
        [SerializeField] TextMeshProUGUI rateText;

        void Update()
        {
            var gm = GameManager.Instance;
            if (gm == null || amountText == null) return;

            amountText.text = $"{NumberFormatter.Format(gm.Currency.Current)} {gm.Balance.currencyName}";
            if (rateText != null)
                rateText.text = NumberFormatter.FormatRate(gm.Generators.TotalProductionPerSecond);
        }
    }
}
