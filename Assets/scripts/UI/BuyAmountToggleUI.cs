using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    /// Cycles the purchase mode: x1 -> x10 -> Max.
    public class BuyAmountToggleUI : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI label;

        void Awake()
        {
            if (button != null)
                button.onClick.AddListener(Cycle);
        }

        void Start() => Refresh();

        void Cycle()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            gm.Generators.CurrentBuyAmount = gm.Generators.CurrentBuyAmount switch
            {
                BuyAmount.One => BuyAmount.Ten,
                BuyAmount.Ten => BuyAmount.Max,
                _ => BuyAmount.One,
            };
            Refresh();
        }

        void Refresh()
        {
            var gm = GameManager.Instance;
            if (gm == null || label == null) return;

            label.text = gm.Generators.CurrentBuyAmount switch
            {
                BuyAmount.Ten => "Buy  x10",
                BuyAmount.Max => "Buy  Max",
                _ => "Buy  x1",
            };
        }
    }
}
