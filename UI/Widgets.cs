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
            if (row.name == "cat-checkbox" || row.name == "cat-slider-row" || row.name == "cat-float-field"
                || row.name == "cat-selection" || row.name == "cat-switch-row")
            {
                return Mathf.Max(1f, rt.sizeDelta.y);
            }
            return Mathf.Max(1f, rt.sizeDelta.y);
        }

        private const float SliderRowCaptionHeight = 22f;
        private const float SliderControlHeight = 24f;
        private const float SliderBarWidth = 168f;
        private const float SliderInputWidth = 76f;
        private const float SliderConfirmWidth = 36f;
        private const float SliderControlGap = 2f;
        internal const float SliderComboWidth = SliderBarWidth + SliderControlGap + SliderInputWidth + SliderControlGap + SliderConfirmWidth;
        private const float FloatFieldInputWidth = 96f;
        private const float SwitchRowHeight = 20f;
        private const float SwitchTrackHeight = 11f;
        internal const float SwitchTrackWidth = 34f;
        private const float SwitchKnobSize = 9f;
        private const float SwitchLabelGap = 4f;
        private const float RadioBoxWidth = 22f;

        private static float MeasureTextWidth(string text, int fontSize)
        {
            Text src = UIRoot.instance.uiGame.assemblerWindow.stateText;
            Text temp = Object.Instantiate(src);
            temp.gameObject.SetActive(false);
            StripLocalizers(temp.gameObject);
            temp.text = text ?? "";
            temp.fontSize = fontSize;
            temp.horizontalOverflow = HorizontalWrapMode.Overflow;
            float w = temp.preferredWidth;
            Object.Destroy(temp.gameObject);
            return w;
        }

        /// <summary>标题 + 一行（左对齐固定宽度）：滑条 | 数字框 | 确认。</summary>
        public static float CreateSliderRow(
            Transform parent, string caption, float x, float y, float width,
            float min, float max, float value, int decimals,
            out Slider slider, out InputField input, out UIButton confirm)
        {
            var hostGo = new GameObject("cat-slider-row", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);
            float cy = 0f;
            float hostW = Mathf.Min(width, SliderComboWidth);
            if (!string.IsNullOrEmpty(caption))
            {
                Text cap = CreateLabel(host, caption, 0f, cy, width, 14);
                cap.fontSize = 14;
                cy += SliderRowCaptionHeight + 4f;
            }

            float sliderW = SliderBarWidth;
            float inputW = SliderInputWidth;
            float confirmW = SliderConfirmWidth;
            float sx = 0f;

            slider = CreatePanelSlider(host);
            slider.gameObject.name = "slider";
            RectTransform sliderRt = (RectTransform)slider.transform;
            PlaceTopLeft(slider.transform, sx, cy, host);
            sliderRt.sizeDelta = new Vector2(sliderW, SliderControlHeight);
            slider.minValue = min;
            slider.maxValue = max;
            slider.wholeNumbers = false;
            slider.onValueChanged = new Slider.SliderEvent();
            float clampedInit = Mathf.Clamp(value, min, max);
            slider.SetValueWithoutNotify(clampedInit);

            float ix = sx + sliderW + SliderControlGap;
            input = CreateNumericInput(host, ix, cy, inputW, SliderControlHeight, decimals, clampedInit);

            float cx = ix + inputW + SliderControlGap;
            string ok = L10n.Pick("确认", "OK");
            confirm = CreateButton(host, ok, cx, cy, confirmW, SliderControlHeight, 12, () => { });
            confirm.gameObject.name = "cat-slider-confirm";

            float rowH = cy + SliderControlHeight;
            host.sizeDelta = new Vector2(hostW, rowH);
            return rowH;
        }

        /// <summary>说明文字（左，可换行）+ 右对齐数字框。</summary>
        public static float CreateFloatFieldRow(
            Transform parent, string caption, float x, float y, float width,
            float min, float max, float value, int decimals,
            out InputField input)
        {
            var hostGo = new GameObject("cat-float-field", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);
            float inputW = FloatFieldInputWidth;
            float labelW = Mathf.Max(80f, width - inputW - SliderControlGap);
            Text cap = CreateLabel(host, caption, 0f, 0f, labelW, 14);
            cap.fontSize = 14;
            cap.alignment = TextAnchor.MiddleLeft;
            Canvas.ForceUpdateCanvases();
            float labelH = Mathf.Max(SliderControlHeight, cap.preferredHeight + 6f);
            cap.rectTransform.sizeDelta = new Vector2(labelW, labelH);

            float inputY = (labelH - SliderControlHeight) * 0.5f;
            input = CreateNumericInput(host, width - inputW, inputY, inputW, SliderControlHeight, decimals, value);

            host.sizeDelta = new Vector2(width, labelH);
            return labelH;
        }

        /// <summary>左/右纯文本 + 固定宽度轨道（0=左，1=右），整组左对齐。</summary>
        public static float CreateSwitchRow(
            Transform parent, string caption, float x, float y, float width,
            string leftLabel, string rightLabel, int value,
            Action<int> onPick,
            out RectTransform knob)
        {
            var hostGo = new GameObject("cat-switch-row", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);
            float cy = 0f;
            if (!string.IsNullOrEmpty(caption))
            {
                Text cap = CreateLabel(host, caption, 0f, cy, width, 14);
                cap.fontSize = 14;
                cy += SliderRowCaptionHeight + 4f;
            }

            float leftW = Mathf.Max(14f, MeasureTextWidth(leftLabel, 14) + 2f);
            Text leftText = CreateLabel(host, leftLabel, 0f, cy + 2f, leftW, 14);
            leftText.fontSize = 14;
            leftText.alignment = TextAnchor.MiddleLeft;
            leftText.raycastTarget = false;
            leftText.rectTransform.sizeDelta = new Vector2(leftW, SwitchRowHeight);

            float trackX = leftW + SwitchLabelGap;
            float trackW = SwitchTrackWidth;
            float trackY = cy + (SwitchRowHeight - SwitchTrackHeight) * 0.5f;
            var trackGo = new GameObject("track", typeof(RectTransform), typeof(Image));
            RectTransform trackRt = PlaceTopLeft(trackGo.transform, trackX, trackY, host);
            trackRt.sizeDelta = new Vector2(trackW, SwitchTrackHeight);
            Image trackImg = trackGo.GetComponent<Image>();
            ApplyTrackSprite(trackImg);
            trackImg.raycastTarget = true;

            Slider sliderRef = ResolveSliderTemplate();
            Image handleImg = null;
            if (sliderRef != null && sliderRef.handleRect != null)
            {
                handleImg = sliderRef.handleRect.GetComponent<Image>();
            }
            var knobGo = new GameObject("knob", typeof(RectTransform), typeof(Image));
            RectTransform knobRt = (RectTransform)knobGo.transform;
            knob = knobRt;
            knobRt.SetParent(trackRt, false);
            knobRt.sizeDelta = new Vector2(SwitchKnobSize, SwitchKnobSize);
            Image knobImage = knobGo.GetComponent<Image>();
            if (handleImg != null && handleImg.sprite != null)
            {
                knobImage.sprite = handleImg.sprite;
                knobImage.type = handleImg.type;
            }
            knobImage.color = new Color(0.75f, 0.82f, 0.92f, 0.95f);
            knobImage.raycastTarget = false;
            PlaceSwitchKnob(knobRt, value, trackW);

            float rightX = trackX + trackW + SwitchLabelGap;
            float rightW = Mathf.Max(20f, MeasureTextWidth(rightLabel, 14) + 4f);
            Text rightText = CreateLabel(host, rightLabel, rightX, cy + 2f, rightW, 14);
            rightText.fontSize = 14;
            rightText.alignment = TextAnchor.MiddleLeft;
            rightText.raycastTarget = false;
            rightText.rectTransform.sizeDelta = new Vector2(rightW, SwitchRowHeight);

            int current = value == 0 ? 0 : 1;
            void Pick(int v)
            {
                current = v == 0 ? 0 : 1;
                PlaceSwitchKnob(knobRt, current, trackW);
                ApplySwitchLabelTone(leftText, current == 0);
                ApplySwitchLabelTone(rightText, current == 1);
                SafeInvoke(() => onPick(current), "switch");
            }

            ApplySwitchLabelTone(leftText, current == 0);
            ApplySwitchLabelTone(rightText, current == 1);

            UIButton trackHit = CreateButton(host, "", trackX, trackY, trackW, SwitchTrackHeight, 12,
                () => Pick(current == 0 ? 1 : 0));
            trackHit.gameObject.name = "switch-track";
            Image trackBtnImg = trackHit.GetComponent<Image>();
            if (trackBtnImg != null)
            {
                trackBtnImg.color = new Color(0f, 0f, 0f, 0.01f);
            }
            foreach (Text t in trackHit.GetComponentsInChildren<Text>(true))
            {
                t.text = "";
            }

            float groupW = rightX + rightW;
            float rowH = cy + SwitchRowHeight;
            host.sizeDelta = new Vector2(groupW, rowH);
            return rowH;
        }

        private static void ApplySwitchLabelTone(Text label, bool active)
        {
            if (label == null)
            {
                return;
            }
            label.color = active
                ? new Color(1f, 1f, 1f, 0.78f)
                : LabelColor;
        }

        public static void PlaceSwitchKnob(RectTransform knob, int value, float trackW)
        {
            if (knob == null)
            {
                return;
            }
            float pad = Mathf.Max(5f, SwitchKnobSize * 0.55f);
            float x = value == 0 ? pad : Mathf.Max(pad, trackW - pad);
            knob.anchorMin = new Vector2(0f, 0.5f);
            knob.anchorMax = new Vector2(0f, 0.5f);
            knob.pivot = new Vector2(0.5f, 0.5f);
            knob.anchoredPosition = new Vector2(x, 0f);
        }

        private static void ApplyTrackSprite(Image trackImg)
        {
            trackImg.color = new Color(0.06f, 0.08f, 0.11f, 0.85f);
            Slider sliderRef = ResolveSliderTemplate();
            if (sliderRef != null)
            {
                Image bg = sliderRef.GetComponent<Image>();
                if (bg != null && bg.sprite != null)
                {
                    trackImg.sprite = bg.sprite;
                    trackImg.type = bg.type;
                }
            }
        }

        /// <summary>单选圆点 + 换行说明，每项一行。</summary>
        public static float CreateRadioSelectionBlock(
            Transform parent, string caption, float x, float y, float width,
            string[] options, int selected,
            Action<int> onPick,
            out List<UIButton> optionRows,
            out List<Image> radioMarks)
        {
            optionRows = new List<UIButton>();
            radioMarks = new List<Image>();
            var hostGo = new GameObject("cat-selection", typeof(RectTransform));
            RectTransform host = PlaceTopLeft(hostGo.transform, x, y, parent);
            float cy = 0f;
            if (!string.IsNullOrEmpty(caption))
            {
                Text cap = CreateLabel(host, caption, 0f, cy, width, 14);
                cap.fontSize = 14;
                cy += cap.rectTransform.sizeDelta.y + 6f;
            }

            UIButton src = UIRoot.instance.uiGame.buildMenu.uxFacilityCheck;
            float labelW = Mathf.Max(60f, width - RadioBoxWidth - 6f);
            for (int i = 0; i < options.Length; i++)
            {
                int idx = i;
                var rowGo = new GameObject("cat-radio-row", typeof(RectTransform));
                RectTransform row = PlaceTopLeft(rowGo.transform, 0f, cy, host);

                UIButton box = Object.Instantiate(src, row, false);
                box.gameObject.name = "cat-radio-box";
                box.gameObject.SetActive(true);
                ResetUIButton(box);
                StripLocalizers(box.gameObject);
                PlaceTopLeft(box.transform, 0f, 0f, row);
                Image mark = box.transform.Find("checked")?.GetComponent<Image>();
                radioMarks.Add(mark);
                Text rowText = box.transform.Find("text")?.GetComponent<Text>();
                if (rowText != null)
                {
                    rowText.raycastTarget = false;
                    rowText.text = options[i];
                    rowText.fontSize = 13;
                    rowText.horizontalOverflow = HorizontalWrapMode.Wrap;
                    rowText.verticalOverflow = VerticalWrapMode.Overflow;
                    rowText.alignment = TextAnchor.UpperLeft;
                    RectTransform textRt = rowText.rectTransform;
                    textRt.anchorMin = new Vector2(0f, 1f);
                    textRt.anchorMax = new Vector2(0f, 1f);
                    textRt.pivot = new Vector2(0f, 1f);
                    textRt.anchoredPosition = new Vector2(RadioBoxWidth + 4f, 0f);
                    textRt.sizeDelta = new Vector2(labelW, 40f);
                    Canvas.ForceUpdateCanvases();
                    float prefH = rowText.preferredHeight;
                    float rowH = Mathf.Max(box.GetComponent<RectTransform>().sizeDelta.y, prefH + 2f);
                    textRt.sizeDelta = new Vector2(labelW, prefH);
                    row.sizeDelta = new Vector2(width, rowH);
                    cy += rowH + 6f;
                }
                else
                {
                    row.sizeDelta = new Vector2(width, 24f);
                    cy += 30f;
                }

                box.onClick += _ => SafeInvoke(() => onPick(idx), options[i]);
                optionRows.Add(box);
            }

            host.sizeDelta = new Vector2(width, cy);
            HighlightRadioSelection(radioMarks, selected);
            return cy;
        }

        public static void HighlightRadioSelection(List<Image> marks, int selected)
        {
            if (marks == null)
            {
                return;
            }
            for (int i = 0; i < marks.Count; i++)
            {
                if (marks[i] != null)
                {
                    marks[i].enabled = i == selected;
                }
            }
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

        /// <summary>面板专用滑条（清空预制体监听，避免原版 UI 逻辑吞事件）。</summary>
        private static Slider CreatePanelSlider(Transform parent)
        {
            Slider template = ResolveSliderTemplate();
            Slider s;
            if (template != null)
            {
                s = Object.Instantiate(template, parent, false);
                s.gameObject.SetActive(true);
                s.onValueChanged = new Slider.SliderEvent();
            }
            else
            {
                s = CreateBasicSlider(parent);
            }
            return s;
        }

        public static void SetNumericInputDisplay(InputField field, float value, int decimals)
        {
            if (field == null)
            {
                return;
            }
            string s = FormatFloat(value, decimals);
            field.SetTextWithoutNotify(s);
            if (field.textComponent != null)
            {
                field.textComponent.text = s;
            }
        }

        public static void AttachSliderRowDriver(
            GameObject host, Slider slider, InputField input,
            float min, float max, int decimals, System.Action<float> onValue)
        {
            var driver = host.AddComponent<SliderRowDriver>();
            driver.Slider = slider;
            driver.Input = input;
            driver.Min = min;
            driver.Max = max;
            driver.Decimals = decimals;
            driver.OnValue = onValue;
        }

        private static Slider CreateBasicSlider(Transform parent)
        {
            Slider reference = ResolveSliderTemplate();
            var root = new GameObject("slider", typeof(RectTransform), typeof(Slider));
            root.transform.SetParent(parent, false);
            RectTransform rootRt = (RectTransform)root.transform;
            rootRt.anchorMin = new Vector2(0f, 1f);
            rootRt.anchorMax = new Vector2(0f, 1f);
            rootRt.pivot = new Vector2(0f, 1f);

            Image refBg = reference != null ? reference.GetComponent<Image>() : null;
            Image refFill = reference?.fillRect != null ? reference.fillRect.GetComponent<Image>() : null;
            Image refHandle = reference?.handleRect != null ? reference.handleRect.GetComponent<Image>() : null;

            GameObject bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
            RectTransform bgRt = (RectTransform)bgGo.transform;
            bgRt.SetParent(root.transform, false);
            StretchFill(bgRt);
            Image bgImg = bgGo.GetComponent<Image>();
            if (refBg != null && refBg.sprite != null)
            {
                bgImg.sprite = refBg.sprite;
                bgImg.type = refBg.type;
            }
            bgImg.color = new Color(0.08f, 0.1f, 0.12f, 0.9f);

            GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
            RectTransform fillAreaRt = (RectTransform)fillArea.transform;
            fillAreaRt.SetParent(root.transform, false);
            StretchFill(fillAreaRt, 4f, 4f);

            GameObject fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            RectTransform fillRt = (RectTransform)fillGo.transform;
            fillRt.SetParent(fillArea.transform, false);
            StretchFill(fillRt);
            Image fillImg = fillGo.GetComponent<Image>();
            if (refFill != null && refFill.sprite != null)
            {
                fillImg.sprite = refFill.sprite;
                fillImg.type = refFill.type;
                fillImg.color = refFill.color;
            }
            else
            {
                fillImg.color = new Color(0.35f, 0.55f, 0.75f, 0.85f);
            }

            GameObject handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
            RectTransform handleAreaRt = (RectTransform)handleArea.transform;
            handleAreaRt.SetParent(root.transform, false);
            StretchFill(handleAreaRt, 4f, 4f);

            GameObject handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
            RectTransform handleRt = (RectTransform)handleGo.transform;
            handleRt.SetParent(handleArea.transform, false);
            handleRt.sizeDelta = new Vector2(14f, 18f);
            Image handleImg = handleGo.GetComponent<Image>();
            if (refHandle != null && refHandle.sprite != null)
            {
                handleImg.sprite = refHandle.sprite;
                handleImg.type = refHandle.type;
                handleImg.color = refHandle.color;
            }
            else
            {
                handleImg.color = new Color(0.8f, 0.86f, 0.94f, 1f);
            }

            Slider slider = root.GetComponent<Slider>();
            slider.fillRect = fillRt;
            slider.handleRect = handleRt;
            slider.targetGraphic = handleImg;
            slider.direction = Slider.Direction.LeftToRight;
            return slider;
        }

        private static void StretchFill(RectTransform rt, float padX = 0f, float padY = 0f)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(padX, padY);
            rt.offsetMax = new Vector2(-padX, -padY);
        }

        private static readonly Color NumericInputTextColor = new Color(1f, 1f, 1f, 0.58f);
        private static readonly Color NumericInputBgColor = new Color(0.04f, 0.05f, 0.07f, 0.5f);

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

        private static Sprite ResolveNumericInputBgSprite()
        {
            InputField template = ResolveInputFieldTemplate();
            Image img = template != null ? template.GetComponent<Image>() : null;
            if (img != null && img.sprite != null)
            {
                return img.sprite;
            }
            Slider slider = ResolveSliderTemplate();
            if (slider != null)
            {
                Image fill = slider.fillRect != null ? slider.fillRect.GetComponent<Image>() : null;
                if (fill != null && fill.sprite != null)
                {
                    return fill.sprite;
                }
            }
            return null;
        }

        private static void StripTextEffects(Text text)
        {
            if (text == null)
            {
                return;
            }
            foreach (Outline outline in text.GetComponents<Outline>())
            {
                Object.DestroyImmediate(outline);
            }
            foreach (Shadow shadow in text.GetComponents<Shadow>())
            {
                Object.DestroyImmediate(shadow);
            }
        }

        private static InputField CreateNumericInput(Transform parent, float x, float y, float w, float h, int decimals,
            float initialValue)
        {
            if (UIRoot.instance?.uiGame?.assemblerWindow?.stateText == null)
            {
                throw new InvalidOperationException("CatUIPanel: game UI not ready (enter a save first).");
            }

            var go = new GameObject("cat-input", typeof(RectTransform), typeof(Image), typeof(InputField));
            RectTransform rt = PlaceTopLeft(go.transform, x, y, parent);
            rt.sizeDelta = new Vector2(w, h);

            Image bg = go.GetComponent<Image>();
            Sprite bgSprite = ResolveNumericInputBgSprite();
            if (bgSprite != null)
            {
                bg.sprite = bgSprite;
                bg.type = Image.Type.Sliced;
            }
            bg.color = NumericInputBgColor;
            bg.raycastTarget = true;

            Text refText = UIRoot.instance.uiGame.assemblerWindow.stateText;
            Text text = Object.Instantiate(refText, go.transform, false);
            text.gameObject.name = "text";
            text.gameObject.SetActive(true);
            StripLocalizers(text.gameObject);
            StripTextEffects(text);
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

            InputField field = go.GetComponent<InputField>();
            field.textComponent = text;
            field.targetGraphic = bg;
            field.contentType = InputField.ContentType.DecimalNumber;
            field.characterValidation = InputField.CharacterValidation.Decimal;
            field.customCaretColor = true;
            field.caretColor = new Color(1f, 1f, 1f, 0.55f);
            field.selectionColor = new Color(0.2f, 0.28f, 0.36f, 0.35f);
            field.onValueChanged = new InputField.OnChangeEvent();
            field.onEndEdit = new InputField.EndEditEvent();
            SetNumericInputDisplay(field, initialValue, decimals);
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
