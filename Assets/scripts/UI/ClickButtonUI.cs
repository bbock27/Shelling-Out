using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ShellingOut
{
    /// clicking sifts for shells. The sieve shakes, shells of the
    /// best unlocked type burst out, and a floating "+N" shows the gain.
    /// The optional label shows income per sift and the current shell type.
    public class ClickButtonUI : MonoBehaviour
    {
        [SerializeField] Button button;
        [SerializeField] TextMeshProUGUI label;
        [Tooltip("Color of the floating \"+N\" text spawned per sift.")]
        [SerializeField] Color floatingTextColor = new Color(0.949f, 0.784f, 0.475f);

        Coroutine shakeRoutine;
        Vector2 basePosition;
        Quaternion baseRotation;
        bool baseCaptured;
        bool sieveSpriteApplied;

        void Awake()
        {
            if (button != null)
                button.onClick.AddListener(OnClicked);
        }

        /// Sieve art comes from the data layer (GameBalance.sieveSprite);
        /// applied once, as soon as the GameManager is available.
        void ApplySieveSprite()
        {
            if (sieveSpriteApplied) return;
            var gm = GameManager.Instance;
            if (gm == null || button == null || button.image == null) return;
            if (gm.Balance.sieveSprite != null)
                button.image.sprite = gm.Balance.sieveSprite;
            sieveSpriteApplied = true;
        }

        void Update()
        {
            ApplySieveSprite();

            var gm = GameManager.Instance;
            if (gm == null || label == null) return;

            var shell = gm.CurrentShellType;
            label.text = shell != null
                ? $"+{NumberFormatter.Format(gm.ClickPower)} per sift  {shell.displayName}"
                : $"+{NumberFormatter.Format(gm.ClickPower)} per sift";
        }

        void OnClicked()
        {
            var gm = GameManager.Instance;
            if (gm == null) return;

            double gained = gm.Click();
            SpawnFloatingText(gained);
            SpawnShellBurst(gm.CurrentShellType);

            if (shakeRoutine != null)
            {
                StopCoroutine(shakeRoutine);
                RestoreBase();
            }
            shakeRoutine = StartCoroutine(Shake());
        }

        IEnumerator Shake()
        {
            var rt = (RectTransform)button.transform;
            if (!baseCaptured)
            {
                basePosition = rt.anchoredPosition;
                baseRotation = rt.localRotation;
                baseCaptured = true;
            }

            const float duration = 0.3f;
            for (float e = 0; e < duration; e += Time.deltaTime)
            {
                float damp = 1f - e / duration;
                rt.localRotation = baseRotation * Quaternion.Euler(0f, 0f, Mathf.Sin(e * 70f) * 8f * damp);
                rt.anchoredPosition = basePosition + new Vector2(
                    Mathf.Sin(e * 55f) * 12f * damp,
                    Mathf.Sin(e * 90f) * 5f * damp);
                yield return null;
            }

            RestoreBase();
            shakeRoutine = null;
        }

        void RestoreBase()
        {
            if (!baseCaptured) return;
            var rt = (RectTransform)button.transform;
            rt.localRotation = baseRotation;
            rt.anchoredPosition = basePosition;
        }

        /// A few shells of the sifted type pop out of the sieve
        void SpawnShellBurst(ShellTypeDefinition shellType)
        {
            int count = Random.Range(3, 6);
            for (int i = 0; i < count; i++)
            {
                var go = new GameObject("SiftShell", typeof(RectTransform));
                go.transform.SetParent(button.transform.parent, false);
                var img = go.AddComponent<Image>();
                // No sprite on the type = plain rect tinted by the type color.
                img.sprite = shellType != null ? shellType.sprite : null;
                img.color = shellType != null ? shellType.tint : Color.white;
                img.preserveAspect = true;
                img.raycastTarget = false;
                go.AddComponent<LayoutElement>().ignoreLayout = true;

                var rt = (RectTransform)go.transform;
                rt.sizeDelta = new Vector2(84, 68);
                rt.anchoredPosition = ((RectTransform)button.transform).anchoredPosition +
                    new Vector2(Random.Range(-60f, 60f), 40f);
                StartCoroutine(FlingShell(rt, img));
            }
        }

        IEnumerator FlingShell(RectTransform rt, Image img)
        {
            var velocity = new Vector2(Random.Range(-280f, 280f), Random.Range(380f, 620f));
            float spin = Random.Range(-260f, 260f);
            Color baseColor = img.color;
            const float duration = 0.75f;

            Vector2 pos = rt.anchoredPosition;
            for (float e = 0; e < duration; e += Time.deltaTime)
            {
                velocity += Vector2.down * 1600f * Time.deltaTime;
                pos += velocity * Time.deltaTime;
                rt.anchoredPosition = pos;
                rt.Rotate(0f, 0f, spin * Time.deltaTime);

                float k = e / duration;
                img.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f - k * k);
                yield return null;
            }
            Destroy(rt.gameObject);
        }

        void SpawnFloatingText(double amount)
        {
            var go = new GameObject("FloatText", typeof(RectTransform));
            go.transform.SetParent(button.transform.parent, false);
            var text = go.AddComponent<TextMeshProUGUI>();
            text.text = "+" + NumberFormatter.Format(amount);
            text.fontSize = 46f;
            text.color = floatingTextColor;
            text.alignment = TextAlignmentOptions.Center;
            text.fontStyle = FontStyles.Bold;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            // Don't let a LayoutGroup on the parent capture and reposition it.
            go.AddComponent<LayoutElement>().ignoreLayout = true;
            text.rectTransform.sizeDelta = new Vector2(400, 80);
            text.rectTransform.anchoredPosition =
                ((RectTransform)button.transform).anchoredPosition +
                new Vector2(Random.Range(-140f, 140f), 180f);
            StartCoroutine(FloatAndFade(text));
        }

        IEnumerator FloatAndFade(TextMeshProUGUI text)
        {
            float dur = 0.8f;
            Vector2 start = text.rectTransform.anchoredPosition;
            Color c = text.color;
            for (float e = 0; e < dur; e += Time.deltaTime)
            {
                float k = e / dur;
                text.rectTransform.anchoredPosition = start + Vector2.up * (130f * k);
                text.color = new Color(c.r, c.g, c.b, 1f - k * k);
                yield return null;
            }
            Destroy(text.gameObject);
        }
    }
}
