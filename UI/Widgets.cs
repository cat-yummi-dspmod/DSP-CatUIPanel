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

        private static readonly Color TabNormal = new Color(0.1f, 0.1f, 0.1f, 0.68f);
        private static readonly Color TabHighlight = new Color(0.9906f, 0.5897f, 0.3691f, 0.4f);

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

        /// <summary>左侧分页，样式与 UXA 一致（戴森球编辑器轨道按钮）。</summary>
        public static UIButton CreateTabButton(Transform parent, string text, float x, float y, float width, float height, Action onClick)
        {
            UIDESwarmPanel swarmPanel = UIRoot.instance.uiGame.dysonEditor.controlPanel.hierarchy.swarmPanel;
            UIButton btn = Object.Instantiate(swarmPanel.orbitButtons[0], parent, false);
            btn.gameObject.name = "yummi-tab";
            btn.gameObject.SetActive(true);
            ResetUIButton(btn);
            StripLocalizers(btn.gameObject);
            Image bg = btn.GetComponent<Image>();
            if (bg != null)
            {
                bg.sprite = swarmPanel.buttonDefaultSprite;
                bg.enabled = true;
            }
            Transform frame = btn.transform.Find("frame");
            if (frame != null)
            {
                frame.gameObject.SetActive(false);
            }
            if (btn.transitions != null && btn.transitions.Length >= 2)
            {
                btn.transitions[0].normalColor = TabNormal;
                btn.transitions[0].highlightColorOverride = TabHighlight;
                btn.transitions[1].normalColor = new Color(1f, 1f, 1f, 0.8f);
                btn.transitions[1].highlightColorOverride = new Color(0.2f, 0.2f, 0.2f, 1f);
            }
            RectTransform rt = PlaceTopLeft(btn.transform, x, y, parent);
            rt.sizeDelta = new Vector2(width, height);
            Transform textTrans = btn.transform.Find("Text");
            if (textTrans != null)
            {
                Text t = textTrans.GetComponent<Text>();
                if (t != null)
                {
                    t.text = text;
                    t.fontSize = 16;
                    t.fontStyle = FontStyle.Normal;
                    t.horizontalOverflow = HorizontalWrapMode.Wrap;
                }
                Shadow shadow = textTrans.GetComponent<Shadow>() ?? textTrans.gameObject.AddComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0f, 0f, 0.4f);
                shadow.effectDistance = new Vector2(1f, -1f);
            }
            btn.onClick += _ => SafeInvoke(onClick, text);
            return btn;
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

        /// <summary>复选框：只拉文字区域，绝不拉宽根节点 Image（否则会变成白条）。</summary>
        public static float CreateCheckbox(Transform parent, string text, float x, float y, int fontSize, float maxRowWidth, out UIButton btn, out Image check)
        {
            UIButton src = UIRoot.instance.uiGame.buildMenu.uxFacilityCheck;
            btn = Object.Instantiate(src, parent, false);
            btn.gameObject.name = "yummi-checkbox";
            btn.gameObject.SetActive(true);
            ResetUIButton(btn);
            StripLocalizers(btn.gameObject);
            RectTransform row = PlaceTopLeft(btn.transform, x, y, parent);
            float boxW = row.sizeDelta.x;
            float boxH = row.sizeDelta.y;
            check = btn.transform.Find("checked")?.GetComponent<Image>();
            if (check != null)
            {
                PlaceTopLeft(check.transform, 0f, 0f, row);
            }

            Image boxImage = btn.GetComponent<Image>();
            Text t = btn.transform.Find("text")?.GetComponent<Text>();
            float rowH = boxH;
            if (t != null)
            {
                t.text = text;
                t.fontSize = fontSize;
                t.alignment = TextAnchor.UpperLeft;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                float labelW = Mathf.Max(60f, maxRowWidth - boxW - 6f);
                RectTransform textRt = t.rectTransform;
                textRt.anchorMin = new Vector2(0f, 1f);
                textRt.anchorMax = new Vector2(0f, 1f);
                textRt.pivot = new Vector2(0f, 1f);
                textRt.anchoredPosition = new Vector2(boxW + 4f, 0f);
                textRt.sizeDelta = new Vector2(labelW, boxH);
                rowH = Mathf.Max(boxH, t.preferredHeight + 4f);
                textRt.sizeDelta = new Vector2(labelW, rowH);
            }

            // 根节点保持方框原始尺寸，避免 Image 被拉成横条
            row.sizeDelta = new Vector2(boxW, rowH);
            if (boxImage != null)
            {
                boxImage.type = Image.Type.Simple;
            }
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
