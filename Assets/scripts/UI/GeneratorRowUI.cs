using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    /// One generator row. icon, name x owned, production stats, and a buy
    /// button that respects the current x1/x10/Max mode. 
    public class GeneratorRowUI : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI statsText;
        [SerializeField] Button buyButton;
        [SerializeField] TextMeshProUGUI buyLabel;
        [Tooltip("Optional. Filled with the definition's icon sprite and tint on Bind.")]
        [SerializeField] Image iconImage;

        public GeneratorDefinition Def { get; private set; }

        float nextRefresh;

        /// Points the row at a definition. fills the static visuals and hooks
        /// the buy button (idempotent).
        public void Bind(GeneratorDefinition def)
        {
            Def = def;
            if (nameText != null) nameText.text = def.displayName;
            if (iconImage != null)
            {
                if (def.icon != null) iconImage.sprite = def.icon;
                iconImage.color = def.tint;
            }
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
            if (gm == null || Def == null) return;
            var state = gm.Generators.Get(Def.id);
            if (state != null) gm.Generators.TryBuy(state);
        }

        void Update()
        {
            if (Time.unscaledTime < nextRefresh) return;
            nextRefresh = Time.unscaledTime + 0.1f;

            var gm = GameManager.Instance;
            if (gm == null || Def == null) return;
            var state = gm.Generators.Get(Def.id);
            if (state == null) return;

            if (nameText != null)
                nameText.text = state.Owned > 0 ? $"{Def.displayName}  ×{state.Owned}" : Def.displayName;
            if (statsText != null)
                statsText.text = state.Owned > 0
                    ? $"{NumberFormatter.FormatRate(gm.Generators.UnitProduction(state))} each  •  {NumberFormatter.FormatRate(gm.Generators.ProductionOf(state))}"
                    : $"{NumberFormatter.FormatRate(gm.Generators.UnitProduction(state))} each";

            int count = gm.Generators.ResolveBuyCount(state);
            double cost = gm.Generators.CostOf(state, count);
            if (buyLabel != null)
                buyLabel.text = $"Buy ×{count}\n{NumberFormatter.Format(cost)}";
            if (buyButton != null)
                buyButton.interactable = gm.Currency.CanAfford(cost);
        }
    }
}
