using System;
using System.Collections.Generic;
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
            if (row.name == "cat-checkbox" || row.name == "cat-slider-row" || row.name == "cat-float-field" || row.name == "cat-selection")
            {
                return Mathf.Max(1f, rt.sizeDelta.y);
            }
            return Mathf.Max(1f, rt.sizeDelta.y);
        }

        private const float SliderRowHeight = 56f;
        private const float SliderControlHeight = 22f;
        private const float FloatFieldRowHeight = 36f;
        private const float SelectionOptionHeight = 30f;

        /// <summary>标题 + 滑条 + 右侧输入框。</summary>
        public static float CreateSliderRow(
            Transform parent, string caption, float x, float y, float width,
            float min, float max, float value, int decimals,
            out Slider slider, out InputField input)
        {
            var hostGo = new GameObject("cat-slider-row", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);
            float captionH = 22f;
            Text cap = CreateLabel(host, caption, 0f, 0f, width, 14);
            cap.fontSize = 14;

            Slider sliderTemplate = ResolveSliderTemplate();
            if (sliderTemplate == null)
            {
                throw new InvalidOperationException("CatUIPanel: no Slider template in game UI (open a save first).");
            }
            slider = Object.Instantiate(sliderTemplate, host, false);
            slider.gameObject.name = "slider";
            slider.gameObject.SetActive(true);
            RectTransform sliderRt = (RectTransform)slider.transform;
            PlaceTopLeft(slider.transform, 0f, captionH + 4f, host);
            float inputW = 88f;
            sliderRt.sizeDelta = new Vector2(width - inputW - 8f, SliderControlHeight);
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.value = Mathf.Clamp(value, min, max);

            input = CreateNumericInput(host, width - inputW, captionH + 4f, inputW, SliderControlHeight, decimals);
            input.text = FormatFloat(value, decimals);

            host.sizeDelta = new Vector2(width, SliderRowHeight);
            return SliderRowHeight;
        }

        public static float CreateFloatFieldRow(
            Transform parent, string caption, float x, float y, float width,
            float min, float max, float value, int decimals,
            out InputField input)
        {
            var hostGo = new GameObject("cat-float-field", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);
            float labelW = Mathf.Min(width * 0.55f, 280f);
            Text cap = CreateLabel(host, caption, 0f, 0f, labelW, 14);
            cap.fontSize = 14;
            input = CreateNumericInput(host, labelW + 8f, 0f, width - labelW - 8f, FloatFieldRowHeight - 4f, decimals);
            input.text = FormatFloat(value, decimals);
            host.sizeDelta = new Vector2(width, FloatFieldRowHeight);
            return FloatFieldRowHeight;
        }

        public static float CreateSelectionBlock(
            Transform parent, string caption, float x, float y, float width,
            string[] options, int selected,
            Action<int> onPick,
            out List<UIButton> optionButtons)
        {
            optionButtons = new List<UIButton>();
            var hostGo = new GameObject("cat-selection", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);
            float cy = 0f;
            if (!string.IsNullOrEmpty(caption))
            {
                Text cap = CreateLabel(host, caption, 0f, cy, width, 14);
                cap.fontSize = 14;
                cy += cap.rectTransform.sizeDelta.y + 6f;
            }
            for (int i = 0; i < options.Length; i++)
            {
                int idx = i;
                UIButton btn = CreateButton(host, options[i], 0f, cy, width, SelectionOptionHeight, 13,
                    () => onPick(idx));
                optionButtons.Add(btn);
                cy += SelectionOptionHeight + 4f;
            }
            host.sizeDelta = new Vector2(width, cy);
            HighlightSelection(optionButtons, selected);
            return cy;
        }

        public static void HighlightSelection(List<UIButton> buttons, int selected)
        {
            if (buttons == null)
            {
                return;
            }
            for (int i = 0; i < buttons.Count; i++)
            {
                buttons[i].highlighted = i == selected;
            }
        }

        private static readonly Color NumericInputTextColor = new Color(0.82f, 0.88f, 0.95f, 1f);
        private static readonly Color NumericInputBgColor = new Color(0.08f, 0.1f, 0.14f, 0.72f);

        private static InputField ResolveInputFieldTemplate()
        {
            UIGame game = UIRoot.instance != null ? UIRoot.instance.uiGame : null;
            UIDEInspector inspector = game?.dysonEditor?.controlPanel?.inspector;
            if (inspector?.layerInfo?.input0 != null)
            {
                return inspector.layerInfo.input0;
            }
            if (inspector?.swarmInfo?.input0 != null)
            {
                return inspector.swarmInfo.input0;
            }
            if (game?.stationWindow?.nameInput != null)
            {
                return game.stationWindow.nameInput;
            }
            return null;
        }

        private static Slider ResolveSliderTemplate()
        {
            UIStationWindow w = UIRoot.instance?.uiGame?.stationWindow;
            if (w == null)
            {
                return null;
            }
            if (w.maxChargePowerSlider != null)
            {
                return w.maxChargePowerSlider;
            }
            if (w.maxMiningSpeedSlider != null)
            {
                return w.maxMiningSpeedSlider;
            }
            return null;
        }

        private static void PrepareClonedInputField(InputField field)
        {
            UIInputField uiInput = field.GetComponent<UIInputField>();
            if (uiInput != null)
            {
                Object.DestroyImmediate(uiInput);
            }
            UIButton uiBtn = field.GetComponent<UIButton>();
            if (uiBtn != null)
            {
                Object.DestroyImmediate(uiBtn);
            }
            Button uguiBtn = field.GetComponent<Button>();
            if (uguiBtn != null)
            {
                Object.DestroyImmediate(uguiBtn);
            }
            field.onValueChanged = new InputField.OnChangeEvent();
            field.onEndEdit = new InputField.EndEditEvent();
            field.interactable = true;
            field.contentType = InputField.ContentType.DecimalNumber;
            field.characterValidation = InputField.CharacterValidation.Decimal;
        }

        private static void ApplyNumericInputLook(InputField field)
        {
            if (field.placeholder != null)
            {
                field.placeholder.gameObject.SetActive(false);
                field.placeholder = null;
            }

            Image bg = field.GetComponent<Image>();
            if (bg != null)
            {
                bg.color = NumericInputBgColor;
            }

            foreach (Transform child in field.transform)
            {
                string n = child.name.ToLowerInvariant();
                if (n.Contains("frame") || n.Contains("highlight") || n.Contains("glow"))
                {
                    child.gameObject.SetActive(false);
                }
            }

            Text text = field.textComponent;
            if (text == null)
            {
                text = field.GetComponentInChildren<Text>(true);
            }
            foreach (Text t in field.GetComponentsInChildren<Text>(true))
            {
                if (t != text)
                {
                    t.gameObject.SetActive(false);
                }
            }
            if (text == null)
            {
                return;
            }
            field.textComponent = text;
            foreach (Outline outline in text.GetComponents<Outline>())
            {
                Object.DestroyImmediate(outline);
            }
            foreach (Shadow shadow in text.GetComponents<Shadow>())
            {
                Object.DestroyImmediate(shadow);
            }
            Text refText = UIRoot.instance.uiGame.assemblerWindow.stateText;
            text.font = refText.font;
            text.fontStyle = FontStyle.Normal;
            text.fontSize = 14;
            text.color = NumericInputTextColor;
            text.alignment = TextAnchor.MiddleRight;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Truncate;
            text.supportRichText = false;
            text.raycastTarget = true;
            RectTransform trt = text.rectTransform;
            trt.anchorMin = Vector2.zero;
            trt.anchorMax = Vector2.one;
            trt.pivot = new Vector2(0.5f, 0.5f);
            trt.offsetMin = new Vector2(6f, 2f);
            trt.offsetMax = new Vector2(-6f, -2f);
            trt.localScale = Vector3.one;
        }

        private static InputField CreateNumericInput(Transform parent, float x, float y, float w, float h, int decimals)
        {
            InputField template = ResolveInputFieldTemplate();
            if (template == null)
            {
                throw new InvalidOperationException("CatUIPanel: no InputField template in game UI (enter a game first).");
            }
            InputField field = Object.Instantiate(template, parent, false);
            field.gameObject.name = "cat-input";
            field.gameObject.SetActive(true);
            StripLocalizers(field.gameObject);
            PrepareClonedInputField(field);
            ApplyNumericInputLook(field);
            RectTransform rt = PlaceTopLeft(field.transform, x, y, parent);
            rt.sizeDelta = new Vector2(w, h);
            if (field.textComponent == null)
            {
                var textGo = new GameObject("text", typeof(RectTransform), typeof(Text));
                textGo.transform.SetParent(field.transform, false);
                Text text = textGo.GetComponent<Text>();
                text.font = UIRoot.instance.uiGame.assemblerWindow.stateText.font;
                text.supportRichText = false;
                RectTransform trt = text.rectTransform;
                trt.anchorMin = Vector2.zero;
                trt.anchorMax = Vector2.one;
                trt.offsetMin = new Vector2(6f, 2f);
                trt.offsetMax = new Vector2(-6f, -2f);
                field.textComponent = text;
                ApplyNumericInputLook(field);
            }
            field.text = "0";
            return field;
        }

        public static string FormatFloat(float v, int decimals)
        {
            if (decimals <= 0)
            {
                return Mathf.RoundToInt(v).ToString();
            }
            return v.ToString("F" + decimals);
        }

        public static bool TryParseFloat(string text, out float value)
        {
            return float.TryParse(text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out value);
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
