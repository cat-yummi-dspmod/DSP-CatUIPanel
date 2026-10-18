using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace YummiUIHarness
{
    /// <summary>
    /// 右下角小猫面板的对外接口。在 Awake 里注册即可，界面会在进入游戏后按需生成。
    /// </summary>
    public static class UIHarness
    {
        public const string Guid = Plugin.Guid;

        /// <summary>接口版本，只在不兼容地修改公开接口时递增。</summary>
        public const int ApiVersion = 2;

        internal static readonly List<Page> Pages = new List<Page>();
        internal static int Revision;

        /// <summary>
        /// 注册一页（左侧分页）。<paramref name="key"/> 是唯一标识，也是分页上显示的文字；重复注册返回同一页。
        /// </summary>
        public static Page RegisterPage(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Page key must not be empty", nameof(key));
            }
            Page page = FindPage(key);
            if (page != null)
            {
                return page;
            }
            page = new Page(key);
            Pages.Add(page);
            Revision++;
            return page;
        }

        public static void OpenPage(string key)
        {
            UI.HarnessWindow.RequestOpen(key);
        }

        public static void TogglePage(string key)
        {
            if (IsOpen && CurrentPageKey == key)
            {
                Close();
            }
            else
            {
                OpenPage(key);
            }
        }

        public static void Close()
        {
            UI.HarnessWindow.Close();
        }

        public static bool IsOpen => UI.HarnessWindow.IsOpen;

        public static string CurrentPageKey => UI.HarnessWindow.CurrentPageKey;

        internal static Page FindPage(string key)
        {
            foreach (Page page in Pages)
            {
                if (page.Key == key)
                {
                    return page;
                }
            }
            return null;
        }
    }

    /// <summary>面板中的一页，控件从上到下依次排列。</summary>
    public sealed class Page
    {
        /// <summary>唯一键，也是左侧分页标题。</summary>
        public string Key { get; }

        internal readonly List<PageItem> Items = new List<PageItem>();

        internal Page(string key)
        {
            Key = key;
        }

        public Page AddCheckbox(ConfigEntry<bool> entry, string label)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            return Add(new CheckboxItem(entry, label));
        }

        public Page AddButton(string label, Action onClick)
        {
            if (onClick == null)
            {
                throw new ArgumentNullException(nameof(onClick));
            }
            return Add(new ButtonItem(label, onClick));
        }

        public Page AddLabel(string text)
        {
            return Add(new LabelItem(text));
        }

        private Page Add(PageItem item)
        {
            Items.Add(item);
            UIHarness.Revision++;
            return this;
        }
    }

    internal abstract class PageItem
    {
        public readonly string Text;

        protected PageItem(string text)
        {
            Text = text ?? "";
        }
    }

    internal sealed class CheckboxItem : PageItem
    {
        public readonly ConfigEntry<bool> Entry;

        public CheckboxItem(ConfigEntry<bool> entry, string label) : base(label)
        {
            Entry = entry;
        }
    }

    internal sealed class ButtonItem : PageItem
    {
        public readonly Action OnClick;

        public ButtonItem(string label, Action onClick) : base(label)
        {
            OnClick = onClick;
        }
    }

    internal sealed class LabelItem : PageItem
    {
        public LabelItem(string text) : base(text)
        {
        }
    }
}
