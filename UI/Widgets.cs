using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace YummiUIHarness.UI
{
    /// <summary>从原版界面克隆控件，保证字体和外观与游戏一致。</summary>
    internal static class Widgets
    {
        public static readonly Color LabelColor = new Color(1f, 1f, 1f, 0.6f);

        public static RectTransform PlaceTopLeft(Transform t, float left, float top, Transform parent)
        {
            var rt = (RectTransform)t;
            if (parent != null)
            {
                rt.SetParent(parent, false);
            }
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(0f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.localScale = Vector3.one;
            rt.anchoredPosition3D = new Vector3(left, -top, 0f);
            return rt;
        }

        public static RectTransform CreateContainer(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(parent, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            return rt;
        }

        /// <summary>panel-bg 内标题栏下方的内容区。</summary>
        public static RectTransform CreateBodyHost(Transform panelBg, float topInset)
        {
            var go = new GameObject("yummi-body", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(panelBg, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(SidePadBody, SidePadBody);
            rt.offsetMax = new Vector2(-SidePadBody, -topInset);
            return rt;
        }

        private const float SidePadBody = 12f;

        public static void StripLocalizers(GameObject go)
        {
            foreach (Localizer l in go.GetComponentsInChildren<Localizer>(true))
            {
                Object.DestroyImmediate(l);
            }
        }

        public static void ResetUIButton(UIButton btn)
        {
            btn.highlighted = false;
            btn.data = 0;
            btn.tips = default;
            if (btn.button != null)
            {
                btn.button.onClick = new Button.ButtonClickedEvent();
                btn.button.enabled = true;
                btn.button.interactable = true;
            }
        }

        /// <summary>控件在父级坐标系下占用的宽度（左上锚点）。</summary>
        public static float MeasureWidth(RectTransform rt)
        {
            if (rt == null)
            {
                return 0f;
            }
            float w = rt.sizeDelta.x;
            foreach (RectTransform child in rt)
            {
                if (!child.gameObject.activeSelf)
                {
                    continue;
                }
                float right = child.anchoredPosition.x + child.sizeDelta.x * (1f - child.pivot.x);
                w = Mathf.Max(w, right);
            }
            return w;
        }

        public static Text CreateLabel(Transform parent, string text, float x, float y, float width, int fontSize = 18)
        {
            Text src = UIRoot.instance.uiGame.assemblerWindow.stateText;
            Text label = Object.Instantiate(src, parent, false);
            label.gameObject.name = "yummi-label";
            label.gameObject.SetActive(true);
            StripLocalizers(label.gameObject);
            label.text = text;
            label.color = LabelColor;
            label.alignment = TextAnchor.MiddleLeft;
            label.fontSize = fontSize;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            RectTransform rt = PlaceTopLeft(label.transform, x, y, parent);
            float minH = fontSize + 8f;
            rt.sizeDelta = new Vector2(width, minH);
            rt.sizeDelta = new Vector2(width, Mathf.Max(minH, label.preferredHeight + 8f));
            return label;
        }

        public static UIButton CreateButton(Transform parent, string text, float x, float y, float width, float height, int fontSize, Action onClick)
        {
            UIButton src = UIRoot.instance.uiGame.statWindow.performancePanelUI.cpuActiveButton;
            UIButton btn = Object.Instantiate(src, parent, false);
            btn.gameObject.name = "yummi-button";
            btn.gameObject.SetActive(true);
            ResetUIButton(btn);
            StripLocalizers(btn.gameObject);
            RectTransform rt = PlaceTopLeft(btn.transform, x, y, parent);
            rt.sizeDelta = new Vector2(width, height);
            Text t = btn.transform.Find("button-text")?.GetComponent<Text>() ?? btn.GetComponentInChildren<Text>(true);
            if (t != null)
            {
                t.text = text;
                t.fontSize = fontSize;
            }
            btn.onClick += _ => SafeInvoke(onClick, text);
            return btn;
        }

        public static float CreateCheckbox(Transform parent, string text, float x, float y, int fontSize, float maxRowWidth, out UIButton btn, out Image check)
        {
            UIButton src = UIRoot.instance.uiGame.buildMenu.uxFacilityCheck;
            btn = Object.Instantiate(src, parent, false);
            btn.gameObject.name = "yummi-checkbox";
            btn.gameObject.SetActive(true);
            ResetUIButton(btn);
            StripLocalizers(btn.gameObject);
            RectTransform row = PlaceTopLeft(btn.transform, x, y, parent);
            check = btn.transform.Find("checked")?.GetComponent<Image>();
            float boxW = row.sizeDelta.x;
            Text t = btn.transform.Find("text")?.GetComponent<Text>();
            float rowH = row.sizeDelta.y;
            if (t != null)
            {
                t.text = text;
                t.fontSize = fontSize;
                t.alignment = TextAnchor.MiddleLeft;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                float labelW = Mathf.Max(80f, maxRowWidth - boxW - 8f);
                t.rectTransform.sizeDelta = new Vector2(labelW, rowH);
                rowH = Mathf.Max(row.sizeDelta.y, t.preferredHeight + 6f);
                t.rectTransform.sizeDelta = new Vector2(labelW, rowH);
            }
            row.sizeDelta = new Vector2(maxRowWidth, rowH);
            return rowH;
        }

        public static void SafeInvoke(Action action, string what)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Plugin.Log.LogError($"Callback of \"{what}\" threw: {e}");
            }
        }
    }
}
