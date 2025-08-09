using UnityEngine;
using UnityEngine.UI;

namespace TJNK.Farwander.Systems.UI
{
    public class DamagePopup : MonoBehaviour
    {
        public static void Spawn(Vector3 worldPos, int amount, Color? color = null)
        {
            var go = new GameObject("DamagePopup");
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;

            var scaler = go.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 10;

            var textGO = new GameObject("Text");
            textGO.transform.SetParent(go.transform, false);
            var txt = textGO.AddComponent<Text>();
            txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            txt.alignment = TextAnchor.MiddleCenter;
            txt.fontSize = 24;
            txt.text = amount.ToString();
            txt.color = color ?? new Color(1f, 0.3f, 0.3f, 1f);

            var rtCanvas = (RectTransform)canvas.transform;
            rtCanvas.sizeDelta = new Vector2(1.6f, 0.8f);

            var rtText = (RectTransform)textGO.transform;
            rtText.anchorMin = Vector2.zero; rtText.anchorMax = Vector2.one;
            rtText.offsetMin = rtText.offsetMax = Vector2.zero;

            var popup = go.AddComponent<DamagePopup>();
            popup._txt = txt;
            go.transform.position = worldPos + new Vector3(0f, 0.6f, 0f);
        }

        // ---- instance ----
        private Text _txt;
        private float _age;

        [SerializeField] float lifetime = 0.7f;
        [SerializeField] float riseSpeed = 1.3f;

        void Update()
        {
            _age += Time.deltaTime;
            transform.position += new Vector3(0f, riseSpeed * Time.deltaTime, 0f);

            // fade out
            if (_txt)
            {
                var c = _txt.color;
                c.a = Mathf.Clamp01(1f - (_age / lifetime));
                _txt.color = c;
            }
            if (_age >= lifetime) Destroy(gameObject);
        }
    }
}
