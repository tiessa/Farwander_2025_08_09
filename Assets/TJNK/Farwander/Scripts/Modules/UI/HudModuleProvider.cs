using TJNK.Farwander.Core;
using UnityEngine;
using UnityEngine.UI;

namespace TJNK.Farwander.Modules.UI
{
    /// <summary>Minimal HUD bootstrap: creates a Canvas and a simple text log panel. No subscribers wired yet.</summary>
    public sealed class HudModuleProvider : ModuleProvider
    {
        public override string ModuleId { get { return "UI"; } }

        public override void Bind(GameCore core)
        {
            base.Bind(core);
            BootstrapHud();
        }

        private void BootstrapHud()
        {
            var go = new GameObject("HUD_Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.layer = LayerMask.NameToLayer("UI");
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600, 900);

            var panel = new GameObject("LogPanel", typeof(Image));
            panel.transform.SetParent(go.transform, false);
            var img = panel.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0.5f);
            var rt = panel.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(0.4f, 0.25f);
            rt.offsetMin = new Vector2(10, 10);
            rt.offsetMax = new Vector2(10, 10);

            var textGo = new GameObject("LogText", typeof(Text));
            textGo.transform.SetParent(panel.transform, false);
            var txt = textGo.GetComponent<Text>();
            txt.text = "Farwander HUD ready";
            txt.alignment = TextAnchor.UpperLeft;
            txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            var trt = textGo.GetComponent<RectTransform>();
            trt.anchorMin = new Vector2(0, 0);
            trt.anchorMax = new Vector2(1, 1);
            trt.offsetMin = new Vector2(8, 8);
            trt.offsetMax = new Vector2(-8, -8);
        }
    }
}
