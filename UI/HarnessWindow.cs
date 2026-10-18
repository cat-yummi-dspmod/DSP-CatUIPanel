using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace YummiUIHarness.UI
{
    /// <summary>面板窗口：外壳取自原版分拣器窗口；内容与分页都画在 panel-bg 内部，并按内容撑开边框。</summary>
    internal static class HarnessWindow
    {
        private const float TitleBand = 52f;
        private const float SidePad = 20f;
        private const float TabWidth = 105f;
        private const float TabHeight = 27f;
        private const float TabSpacing = 4f;
        private const float TabLeft = 8f;
        private const float ContentMinWidth = 460f;
        private const float RowSpacing = 12f;
        private const float ButtonHeight = 42f;
        private const float BottomPad = 24f;
        private const int BodyFontSize = 18;
        private sealed class PageView
        {
            public Page Page;
            public UIButton Tab;
            public GameObject Panel;
            public readonly List<(CheckboxItem item, Image check)> Checkboxes = new List<(CheckboxItem, Image)>();
        }

        private static GameObject window;
        private static RectTransform windowRect;
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
            if (window == null || builtRevision != UIHarness.Revision)
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
            try
            {
                Build();
                windowRect.anchoredPosition = keepPos;
                builtRevision = UIHarness.Revision;
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
            }
        }

        private static void Build()
        {
            GameObject template = UIRoot.instance.uiGame.inserterWindow.gameObject;
            var holder = new GameObject("yummi-holder");
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
            window.name = "yummi-ui-harness-window";
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
                title.text = Lang.Pick("Yummi 面板", "Yummi Panel");
            }
            foreach (Button b in panelBg.GetComponentsInChildren<Button>(true))
            {
                b.onClick = new Button.ButtonClickedEvent();
                b.onClick.AddListener(Close);
            }

            RectTransform body = Widgets.CreateBodyHost(panelBg, TitleBand + 8f);

            float tabColumnWidth = TabWidth;
            foreach (Page page in UIHarness.Pages)
            {
                tabColumnWidth = Mathf.Max(tabColumnWidth, Mathf.Min(MeasureTabWidth(page.Key) + 20f, 140f));
            }

            float contentLeft = TabLeft + tabColumnWidth + SidePad;
            float contentWidth = ContentMinWidth;
            float tabsBottom = 0f;
            float contentBottom = 0f;

            if (UIHarness.Pages.Count == 0)
            {
                Text empty = Widgets.CreateLabel(body, Lang.Pick("还没有 mod 在这里注册页面。", "No mod has registered a page yet."),
                    0, 0, contentWidth + contentLeft);
                contentBottom = empty.rectTransform.sizeDelta.y;
            }

            foreach (Page page in UIHarness.Pages)
            {
                var view = new PageView { Page = page };
                PageView captured = view;
                view.Tab = Widgets.CreateTabButton(body, page.Key, TabLeft, tabsBottom, tabColumnWidth, TabHeight,
                    () => { ShowPage(captured); SyncCheckboxes(); });
                tabsBottom += TabHeight + TabSpacing;

                RectTransform panel = Widgets.CreateContainer("page-" + page.Key, body);
                view.Panel = panel.gameObject;
                float y = 0f;
                foreach (PageItem item in page.Items)
                {
                    float rowH = BuildItem(view, item, panel, contentLeft, y, contentWidth);
                    y += rowH + RowSpacing;
                }
                contentBottom = Mathf.Max(contentBottom, y);
                views.Add(view);
            }

            float innerW = SidePad + contentLeft + contentWidth + SidePad;
            float innerH = Mathf.Max(tabsBottom, contentBottom) + BottomPad;
            float frameW = innerW;
            float frameH = TitleBand + 8f + innerH + SidePad;
            ApplyFrameSize(windowRect, panelBg, window.transform.Find("shadow"), frameW, frameH);
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
                    float rowH = Widgets.CreateCheckbox(parent, cb.Text, x, y, BodyFontSize, contentWidth, out UIButton btn, out Image check);
                    btn.onClick += _ => cb.Entry.Value = !cb.Entry.Value;
                    view.Checkboxes.Add((cb, check));
                    return rowH;
                }
                case ButtonItem b:
                    Widgets.CreateButton(parent, b.Text, x, y, contentWidth, ButtonHeight, BodyFontSize, b.OnClick);
                    return ButtonHeight;
                case LabelItem l:
                    return Widgets.CreateLabel(parent, l.Text, x, y, contentWidth, BodyFontSize).rectTransform.sizeDelta.y;
                default:
                    return 0f;
            }
        }
    }
}
