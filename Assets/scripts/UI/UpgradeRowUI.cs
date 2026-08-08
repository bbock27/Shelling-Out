using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    public class UpgradeRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI descriptionText;
        [SerializeField] Button buyButton;
        [SerializeField] TextMeshProUGUI buyLabel;

        public UpgradeDefinition Def { get; private set; }

        float nextRefresh;

        /// Points the row at a definition: fills the static visuals and hooks
        /// the buy button (idempotent).
        public void Bind(UpgradeDefinition def)
        {
            Def = def;
            if (nameText != null) nameText.text = def.displayName;
            if (descriptionText != null) descriptionText.text = def.description;
            if (buyLabel != null) buyLabel.text = NumberFormatter.Format(def.cost);
            if (buyButton != null)
            {
                buyButton.onClick.RemoveListener(OnBuyClicked);
                buyButton.onClick.AddListener(OnBuyClicked);
            }
            nextRefresh = 0f; // refresh on the next Update
        }

        void OnBuyClicked()
        {
            var gm = GameManager.Instance;
            if (gm != null && Def != null) gm.Upgrades.TryBuy(Def);
        }

        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.1f;

            var gm = GameManager.Instance;
            if (gm == null || Def == null) return;

            if (buyLabel != null)
                buyLabel.text = NumberFormatter.Format(Def.cost);
            if (buyButton != null)
                buyButton.interactable = gm.Currency.CanAfford(Def.cost);
        }
    }
}
