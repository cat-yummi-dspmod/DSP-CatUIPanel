using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CatUIPanel.UI
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
            var go = new GameObject("cat-body", typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(panelBg, false);
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(SidePadBody, SidePadBody);
            rt.offsetMax = new Vector2(-SidePadBody, -topInset);
            return rt;
        }

        internal const float SidePadBody = 12f;

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
            btn.gameObject.name = "cat-tab";
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
                    t.alignment = TextAnchor.MiddleCenter;
                    t.horizontalOverflow = HorizontalWrapMode.Overflow;
                    t.verticalOverflow = VerticalWrapMode.Truncate;
                    RectTransform textRt = t.rectTransform;
                    textRt.anchorMin = Vector2.zero;
                    textRt.anchorMax = Vector2.one;
                    textRt.pivot = new Vector2(0.5f, 0.5f);
                    textRt.offsetMin = new Vector2(10f, 0f);
                    textRt.offsetMax = new Vector2(-10f, 0f);
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
            label.gameObject.name = "cat-label";
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

        /// <summary>布局完成后按实际换行高度更新标签（避免面板按偏小的 preferredHeight 裁切）。</summary>
        public static void RefreshLabelHeight(Text label)
        {
            if (label == null)
            {
                return;
            }
            Canvas.ForceUpdateCanvases();
            RectTransform rt = label.rectTransform;
            float minH = label.fontSize + 8f;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, Mathf.Max(minH, label.preferredHeight + 8f));
        }

        /// <summary>同步复选框行高与右侧换行文字。方框尺寸保持原版，只改外层行。</summary>
        public static void RefreshCheckboxRow(RectTransform host)
        {
            if (host == null || host.name != "cat-checkbox")
            {
                return;
            }
            RectTransform box = host.Find("cat-checkbox-box") as RectTransform;
            Text t = host.GetComponentInChildren<Text>(true);
            if (box == null || t == null || t.gameObject.name != "text")
            {
                t = box != null ? box.Find("text")?.GetComponent<Text>() : null;
            }
            if (box == null || t == null)
            {
                return;
            }
            float boxW = box.sizeDelta.x;
            float boxH = box.sizeDelta.y;
            float labelW = t.rectTransform.sizeDelta.x;
            if (labelW <= 0f)
            {
                labelW = 200f;
            }
            float rowH = LayoutCheckboxLabel(t, boxW, boxH, labelW);
            host.sizeDelta = new Vector2(Mathf.Max(host.sizeDelta.x, boxW + 6f + labelW), rowH);
        }

        /// <returns>整行占位高度（用于纵向排版）。</returns>
        private static float LayoutCheckboxLabel(Text t, float boxW, float boxH, float labelW)
        {
            Canvas.ForceUpdateCanvases();
            RectTransform textRt = t.rectTransform;
            textRt.anchorMin = new Vector2(0f, 1f);
            textRt.anchorMax = new Vector2(0f, 1f);
            textRt.pivot = new Vector2(0f, 1f);
            textRt.anchoredPosition = new Vector2(boxW + 4f, 0f);
            float prefH = t.preferredHeight;
            bool singleLine = prefH <= boxH + 1f;
            if (singleLine)
            {
                t.alignment = TextAnchor.MiddleLeft;
                textRt.sizeDelta = new Vector2(labelW, boxH);
            }
            else
            {
                t.alignment = TextAnchor.UpperLeft;
                textRt.sizeDelta = new Vector2(labelW, prefH);
            }
            return Mathf.Max(boxH, prefH + 2f);
        }

        public static UIButton CreateButton(Transform parent, string text, float x, float y, float width, float height, int fontSize, Action onClick)
        {
            UIButton src = UIRoot.instance.uiGame.statWindow.performancePanelUI.cpuActiveButton;
            UIButton btn = Object.Instantiate(src, parent, false);
            btn.gameObject.name = "cat-button";
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

        /// <summary>
        /// 复选框：外层行不含 Image；方框保持原版尺寸放在行左侧，文字在方框右侧。
        /// 不改方框 Rect 的位置锚点，避免方框被拉成白条或跑到分页上。
        /// </summary>
        public static float CreateCheckbox(Transform parent, string text, float x, float y, int fontSize, float maxRowWidth, out UIButton btn, out Image check)
        {
            var hostGo = new GameObject("cat-checkbox", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);

            UIButton src = UIRoot.instance.uiGame.buildMenu.uxFacilityCheck;
            btn = Object.Instantiate(src, host, false);
            btn.gameObject.name = "cat-checkbox-box";
            btn.gameObject.SetActive(true);
            ResetUIButton(btn);
            StripLocalizers(btn.gameObject);

            RectTransform box = PlaceTopLeft(btn.transform, 0f, 0f, host);
            float boxW = box.sizeDelta.x;
            float boxH = box.sizeDelta.y;
            check = btn.transform.Find("checked")?.GetComponent<Image>();

            Text t = btn.transform.Find("text")?.GetComponent<Text>();
            float labelW = Mathf.Max(60f, maxRowWidth - boxW - 6f);
            float rowH = boxH;
            if (t != null)
            {
                t.raycastTarget = false;
                t.text = text;
                t.fontSize = fontSize;
                t.horizontalOverflow = HorizontalWrapMode.Wrap;
                t.verticalOverflow = VerticalWrapMode.Overflow;
                rowH = LayoutCheckboxLabel(t, boxW, boxH, labelW);
            }

            host.sizeDelta = new Vector2(boxW + 6f + labelW, rowH);
            return rowH;
        }

        /// <summary>纵向堆叠时用的行高（复选框含换行文字）。</summary>
        public static float MeasureStackRowHeight(Transform row)
        {
            if (row is not RectTransform rt)
            {
                return 0f;
            }
            if (row.name == "cat-checkbox")
            {
                Text t = row.GetComponentInChildren<Text>(true);
                RectTransform box = row.Find("cat-checkbox-box") as RectTransform;
                float boxH = box != null ? box.sizeDelta.y : 0f;
                if (t != null)
                {
                    return Mathf.Max(boxH, t.preferredHeight + 2f, rt.sizeDelta.y);
                }
            }
            return Mathf.Max(1f, rt.sizeDelta.y);
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
