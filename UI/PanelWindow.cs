using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace CatUIPanel.UI
{
    /// <summary>面板窗口：外壳取自原版分拣器窗口；内容与分页都画在 panel-bg 内部，并按内容撑开边框。</summary>
    internal static class PanelWindow
    {
        private const float TitleBand = 48f;
        private const float SidePad = 20f;
        private const float OuterMargin = 30f;
        private const float TabWidth = 105f;
        private const float TabHeight = 27f;
        private const float TabSpacing = 4f;
        private const float TabLeft = 8f;
        private const float ContentMinWidth = 460f;
        private const float RowSpacing = 12f;
        private const float ButtonHeight = 30f;
        private const float BottomPad = 24f;
        private const int BodyFontSize = 15;
        private const int ButtonFontSize = 14;
        private sealed class PageView
        {
            public Page Page;
            public UIButton Tab;
            public GameObject Panel;
            public float ContentLeft;
            public readonly List<(CheckboxItem item, Image check)> Checkboxes = new List<(CheckboxItem, Image)>();
        }

        private struct LangText
        {
            public Text Label;
            public string Zh;
            public string En;
        }

        private static readonly List<LangText> langTexts = new List<LangText>();

        private static void BindText(Text label, string zh, string en)
        {
            if (label == null)
            {
                return;
            }
            label.text = L10n.Pick(zh, en);
            langTexts.Add(new LangText { Label = label, Zh = zh, En = en });
        }

        private static GameObject window;
        private static RectTransform windowRect;
        private static RectTransform bodyHost;
        private static Transform panelBgTransform;
        private static readonly List<PageView> views = new List<PageView>();
        private static int builtRevision = -1;
        private static string pendingPage;
        private static bool pending;

        public static string CurrentPageKey { get; private set; }

        public static bool IsOpen => window != null && window.activeSelf;

        private static bool InGame =>
            GameMain.instance != null && GameMain.isRunning && !GameMain.instance.isMenuDemo
            && UIRoot.instance != null && UIRoot.instance.uiGame != null;

        public static void RequestOpen(string key)
        {
            pendingPage = key;
            pending = true;
        }

        public static void Close()
        {
            pending = false;
            if (window != null)
            {
                window.SetActive(false);
            }
        }

        /// <summary>游戏语言已切换。只改已经生成的文案，不重建窗口，也不再读语言。</summary>
        public static void OnLanguageChanged()
        {
            if (langTexts.Count == 0)
            {
                return;
            }
            for (int i = 0; i < langTexts.Count; i++)
            {
                LangText slot = langTexts[i];
                if (slot.Label != null)
                {
                    slot.Label.text = L10n.Pick(slot.Zh, slot.En);
                }
            }
            FitTabColumn();
            if (IsOpen)
            {
                RefitFrameToContent();
            }
        }

        /// <summary>分页按当前文案的单行宽度拉开，避免英文标题被折成两行。</summary>
        private static void FitTabColumn()
        {
            if (views.Count == 0)
            {
                return;
            }
            float column = TabWidth;
            foreach (PageView view in views)
            {
                string title = view.Page != null ? L10n.Pick(view.Page.TitleZh, view.Page.TitleEn) : "";
                column = Mathf.Max(column, MeasureTabWidth(title) + 24f);
            }
            float contentLeft = TabLeft + column + SidePad;
            foreach (PageView view in views)
            {
                if (view.Tab != null && view.Tab.transform is RectTransform tabRt)
                {
                    tabRt.sizeDelta = new Vector2(column, TabHeight);
                }
                if (view.Panel != null && view.Panel.transform is RectTransform panelRt)
                {
                    Vector2 pos = panelRt.anchoredPosition;
                    pos.x = contentLeft;
                    panelRt.anchoredPosition = pos;
                }
            }
        }

        public static void OnGameEnd()
        {
            if (GameMain.instance != null && GameMain.instance.isMenuDemo)
            {
                return;
            }
            Close();
        }

        public static void OnGameUiUpdate()
        {
            if (!InGame)
            {
                return;
            }
            LauncherButton.Update(ToggleFromLauncher);
            if (pending)
            {
                pending = false;
                Open(pendingPage);
            }
            if (!IsOpen)
            {
                return;
            }
            if (VFInput.escape)
            {
                VFInput.UseEscape();
                Close();
                return;
            }
            SyncCheckboxes();
        }

        private static void ToggleFromLauncher()
        {
            if (IsOpen)
            {
                Close();
            }
            else
            {
                Open(CurrentPageKey);
            }
        }

        private static void Open(string key)
        {
            if (window == null || builtRevision != UIPanel.Revision)
            {
                Rebuild();
            }
            if (window == null)
            {
                return;
            }
            PageView view = views.Find(v => v.Page.Key == key) ?? (views.Count > 0 ? views[0] : null);
            if (key != null && view != null && view.Page.Key != key)
            {
                Plugin.Log.LogWarning($"OpenPage: no page \"{key}\", showing \"{view.Page.Key}\" instead.");
            }
            ShowPage(view);
            window.SetActive(true);
            window.transform.SetAsLastSibling();
            RefitFrameToContent();
            SyncCheckboxes();
        }

        private static void ShowPage(PageView current)
        {
            CurrentPageKey = current?.Page.Key;
            foreach (PageView v in views)
            {
                bool on = v == current;
                v.Panel.SetActive(on);
                v.Tab.highlighted = on;
            }
        }

        private static void SyncCheckboxes()
        {
            foreach (PageView v in views)
            {
                if (!v.Panel.activeSelf)
                {
                    continue;
                }
                foreach ((CheckboxItem item, Image check) in v.Checkboxes)
                {
                    if (check != null && check.enabled != item.Entry.Value)
                    {
                        check.enabled = item.Entry.Value;
                    }
                }
            }
        }

        private static void Rebuild()
        {
            Vector2 keepPos = windowRect != null ? windowRect.anchoredPosition : Vector2.zero;
            if (window != null)
            {
                Object.Destroy(window);
            }
            window = null;
            views.Clear();
            langTexts.Clear();
            try
            {
                Build();
                windowRect.anchoredPosition = keepPos;
                builtRevision = UIPanel.Revision;
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("Failed to build the panel window: " + e);
                if (window != null)
                {
                    Object.Destroy(window);
                }
                window = null;
                views.Clear();
                langTexts.Clear();
            }
        }

        private static void Build()
        {
            GameObject template = UIRoot.instance.uiGame.inserterWindow.gameObject;
            var holder = new GameObject("cat-holder");
            holder.SetActive(false);
            window = Object.Instantiate(template, holder.transform, false);
            Object.DestroyImmediate(window.GetComponent<UIInserterWindow>());
            // 必须先提起 panel-bg，再删其它子节点（否则 windowTrans 被删时会把 panel-bg 一起删掉）
            NestPanelBgOnRoot(window.transform);
            for (int i = window.transform.childCount - 1; i >= 0; i--)
            {
                GameObject child = window.transform.GetChild(i).gameObject;
                if (child.name != "panel-bg" && child.name != "shadow")
                {
                    Object.DestroyImmediate(child);
                }
            }
            window.name = "cat-ui-harness-window";
            window.transform.SetParent(template.transform.parent, false);
            Object.Destroy(holder);
            window.SetActive(false);

            windowRect = (RectTransform)window.transform;
            windowRect.anchorMin = new Vector2(0.5f, 0.5f);
            windowRect.anchorMax = new Vector2(0.5f, 0.5f);
            windowRect.pivot = new Vector2(0.5f, 0.5f);

            Transform panelBg = FindPanelBg(window.transform);
            if (panelBg == null)
            {
                throw new System.InvalidOperationException("panel-bg not found on inserter window template");
            }

            Text title = panelBg.Find("title-text")?.GetComponent<Text>();
            if (title != null)
            {
                Widgets.StripLocalizers(title.gameObject);
                BindText(title, "猫猫面板(F9)", "Cat Panel (F9)");
            }
            foreach (Button b in panelBg.GetComponentsInChildren<Button>(true))
            {
                b.onClick = new Button.ButtonClickedEvent();
                b.onClick.AddListener(Close);
            }

            RectTransform body = Widgets.CreateBodyHost(panelBg, TitleBand + 8f);
            // contentBottom / tabsBottom 仅作占位；最终尺寸由 RefitFrameToContent 实测

            float tabColumnWidth = TabWidth;
            foreach (Page page in UIPanel.Pages)
            {
                tabColumnWidth = Mathf.Max(tabColumnWidth, MeasureTabWidth(page.Title) + 24f);
            }

            float contentLeft = TabLeft + tabColumnWidth + SidePad;
            float contentWidth = ContentMinWidth;
            float tabsBottom = 0f;
            float contentBottom = 0f;

            if (UIPanel.Pages.Count == 0)
            {
                Text empty = Widgets.CreateLabel(body, "", 0, 0, contentWidth + contentLeft);
                BindText(empty, "还没有 mod 在这里注册页面。", "No mod has registered a page yet.");
                contentBottom = empty.rectTransform.sizeDelta.y;
            }

            foreach (Page page in UIPanel.Pages)
            {
                var view = new PageView { Page = page };
                PageView captured = view;
                view.Tab = Widgets.CreateTabButton(body, page.Title, TabLeft, tabsBottom, tabColumnWidth, TabHeight,
                    () => { ShowPage(captured); SyncCheckboxes(); });
                BindText(view.Tab.transform.Find("Text")?.GetComponent<Text>(), page.TitleZh, page.TitleEn);
                tabsBottom += TabHeight + TabSpacing;

                // 顶左锚点，直接放在分页右侧。不用拉伸留白，避免方框仍从 x=0 画到分页上。
                RectTransform panel = Widgets.PlaceTopLeft(
                    new GameObject("page-" + page.Key, typeof(RectTransform)).transform,
                    contentLeft, 0f, body);
                panel.sizeDelta = new Vector2(contentWidth, 8f);
                view.Panel = panel.gameObject;
                view.ContentLeft = 0f;
                float y = 0f;
                foreach (PageItem item in page.Items)
                {
                    float rowH = BuildItem(view, item, panel, 0f, y, contentWidth);
                    y += rowH + RowSpacing;
                }
                contentBottom = Mathf.Max(contentBottom, y);
                view.Tab.transform.SetAsLastSibling();
                views.Add(view);
            }

            bodyHost = body;
            panelBgTransform = panelBg;
            RefitFrameToContent();
        }

        /// <summary>按实际控件边界撑开窗口（与 UXA AutoFitWindowSize 思路一致）。</summary>
        private static void RefitFrameToContent()
        {
            if (windowRect == null || bodyHost == null || panelBgTransform == null)
            {
                return;
            }
            RefreshDynamicHeights(bodyHost);
            foreach (PageView v in views)
            {
                RelayoutPageStack((RectTransform)v.Panel.transform, v.ContentLeft);
            }
            AccumulateWidgetExtents(bodyHost, out float maxX, out float maxY);
            float frameW = Widgets.SidePadBody + maxX + OuterMargin;
            float topInset = TitleBand + 8f;
            float frameH = topInset + maxY + Widgets.SidePadBody + OuterMargin;
            ApplyFrameSize(windowRect, panelBgTransform, window.transform.Find("shadow"), frameW, frameH);
        }

        private static void RefreshDynamicHeights(RectTransform body)
        {
            Canvas.ForceUpdateCanvases();
            foreach (Text label in body.GetComponentsInChildren<Text>(true))
            {
                if (label.gameObject.name == "cat-label")
                {
                    Widgets.RefreshLabelHeight(label);
                }
            }
            foreach (RectTransform rt in body.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.gameObject.name == "cat-checkbox")
                {
                    Widgets.RefreshCheckboxRow(rt);
                }
            }
        }

        /// <summary>换行/复选框高度变化后，按 sibling 顺序重新累加 y，避免行与行重叠。</summary>
        private static void RelayoutPageStack(RectTransform panel, float contentLeft)
        {
            if (panel == null)
            {
                return;
            }
            float y = 0f;
            Transform panelTransform = panel.transform;
            for (int i = 0; i < panelTransform.childCount; i++)
            {
                Transform child = panelTransform.GetChild(i);
                if (!child.name.StartsWith("cat-", StringComparison.Ordinal))
                {
                    continue;
                }
                if (child is not RectTransform rt)
                {
                    continue;
                }
                float rowH = Widgets.MeasureStackRowHeight(child);
                Widgets.PlaceTopLeft(child, contentLeft, y, panelTransform);
                y += rowH + RowSpacing;
            }
        }

        private static void AccumulateWidgetExtents(RectTransform body, out float maxX, out float maxY)
        {
            maxX = 0f;
            maxY = 0f;
            foreach (RectTransform rt in body.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt == body)
                {
                    continue;
                }
                string n = rt.gameObject.name;
                if (n != "cat-checkbox" && n != "cat-button" && n != "cat-label" && n != "cat-tab")
                {
                    continue;
                }
                OffsetFromBodyTopLeft(body, rt, out float left, out float top);
                maxX = Mathf.Max(maxX, left + rt.sizeDelta.x);
                maxY = Mathf.Max(maxY, top + rt.sizeDelta.y);
            }
        }

        /// <summary>子控件的 anchoredPosition 相对各自父节点。内容列套在 page 下之后，必须累加到 body 左上角。</summary>
        private static void OffsetFromBodyTopLeft(RectTransform body, RectTransform rt, out float left, out float top)
        {
            left = 0f;
            top = 0f;
            RectTransform cur = rt;
            while (cur != null && cur != body)
            {
                left += cur.anchoredPosition.x;
                top += -cur.anchoredPosition.y;
                cur = cur.parent as RectTransform;
            }
        }

        /// <summary>若 panel-bg 在 windowTrans 等中间层里，提到根下，避免只改根 sizeDelta 边框不变。</summary>
        private static void NestPanelBgOnRoot(Transform root)
        {
            Transform bg = FindPanelBg(root);
            if (bg == null || bg.parent == root)
            {
                return;
            }
            Transform shadow = root.Find("shadow");
            if (shadow != null && shadow.parent != root)
            {
                shadow.SetParent(root, false);
            }
            bg.SetParent(root, false);
            for (int i = root.childCount - 1; i >= 0; i--)
            {
                Transform child = root.GetChild(i);
                if (child != bg && child != shadow)
                {
                    Object.DestroyImmediate(child.gameObject);
                }
            }
        }

        private static Transform FindPanelBg(Transform root)
        {
            if (root.Find("panel-bg") != null)
            {
                return root.Find("panel-bg");
            }
            foreach (Transform t in root.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "panel-bg")
                {
                    return t;
                }
            }
            return null;
        }

        /// <summary>与 UXA 相同：只改根节点尺寸，panel-bg/shadow 拉伸铺满根节点（避免单独改 panel-bg sizeDelta 拉出白边）。</summary>
        private static void ApplyFrameSize(RectTransform root, Transform panelBg, Transform shadow, float w, float h)
        {
            root.sizeDelta = new Vector2(w, h);
            StretchChildFill(panelBg);
            StretchChildFill(shadow);
        }

        private static void StretchChildFill(Transform t)
        {
            if (t is not RectTransform rt)
            {
                return;
            }
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        private static float MeasureTabWidth(string text)
        {
            Text src = UIRoot.instance.uiGame.assemblerWindow.stateText;
            Text temp = Object.Instantiate(src);
            temp.gameObject.SetActive(false);
            Widgets.StripLocalizers(temp.gameObject);
            temp.text = text;
            temp.fontSize = 16;
            temp.horizontalOverflow = HorizontalWrapMode.Overflow;
            float w = temp.preferredWidth;
            Object.Destroy(temp.gameObject);
            return w;
        }

        private static float BuildItem(PageView view, PageItem item, Transform parent, float x, float y, float contentWidth)
        {
            switch (item)
            {
                case CheckboxItem cb:
                {
                    float rowH = Widgets.CreateCheckbox(parent, "", x, y, BodyFontSize, contentWidth, out UIButton btn, out Image check);
                    BindText(btn.transform.Find("text")?.GetComponent<Text>(), cb.TextZh, cb.TextEn);
                    btn.onClick += _ => cb.Entry.Value = !cb.Entry.Value;
                    view.Checkboxes.Add((cb, check));
                    return rowH;
                }
                case ButtonItem b:
                {
                    float btnW = b.Width > 0f ? b.Width : contentWidth;
                    UIButton btn = Widgets.CreateButton(parent, "", x, y, btnW, ButtonHeight, ButtonFontSize, b.OnClick);
                    BindText(btn.transform.Find("button-text")?.GetComponent<Text>() ?? btn.GetComponentInChildren<Text>(true), b.TextZh, b.TextEn);
                    return ButtonHeight;
                }
                case LabelItem l:
                {
                    Text label = Widgets.CreateLabel(parent, "", x, y, contentWidth, BodyFontSize);
                    BindText(label, l.TextZh, l.TextEn);
                    return label.rectTransform.sizeDelta.y;
                }
                default:
                    return 0f;
            }
        }
    }
}
