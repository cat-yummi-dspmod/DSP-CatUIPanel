using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace CatUIPanel
{
    /// <summary>
    /// 右下角小猫面板的对外接口。在 Awake 里注册即可，界面会在进入游戏后按需生成。
    /// </summary>
    public static class UIPanel
    {
        public const string Guid = Plugin.Guid;

        /// <summary>接口版本，只在不兼容地修改公开接口时递增。</summary>
        public const int ApiVersion = 5;

        internal static readonly List<Page> Pages = new List<Page>();
        internal static int Revision;

        /// <summary>注册一页。<paramref name="key"/> 为稳定标识（OpenPage / F9 用）；标题在生成 UI 时按游戏语言显示。</summary>
        public static Page RegisterPage(string key, string titleZh, string titleEn)
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
            page = new Page(key, titleZh, titleEn);
            Pages.Add(page);
            Revision++;
            return page;
        }

        /// <summary>注册一页（标题中英文相同）。</summary>
        public static Page RegisterPage(string key)
        {
            return RegisterPage(key, key, key);
        }

        public static void OpenPage(string key)
        {
            UI.PanelWindow.RequestOpen(key);
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
            UI.PanelWindow.Close();
        }

        public static bool IsOpen => UI.PanelWindow.IsOpen;

        public static string CurrentPageKey => UI.PanelWindow.CurrentPageKey;

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
        /// <summary>稳定标识，与 <see cref="UIPanel.OpenPage"/> 一致。</summary>
        public string Key { get; }

        internal readonly string TitleZh;
        internal readonly string TitleEn;

        /// <summary>当前语言下的分页标题（仅 UI 构建时读取）。</summary>
        internal string Title => L10n.Pick(TitleZh, TitleEn);

        internal readonly List<PageItem> Items = new List<PageItem>();

        internal Page(string key, string titleZh, string titleEn)
        {
            Key = key;
            TitleZh = titleZh ?? key;
            TitleEn = string.IsNullOrEmpty(titleEn) ? TitleZh : titleEn;
        }

        public Page AddCheckbox(ConfigEntry<bool> entry, string labelZh, string labelEn)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            return Add(new CheckboxItem(entry, labelZh, labelEn));
        }

        public Page AddCheckbox(ConfigEntry<bool> entry, string label)
        {
            return AddCheckbox(entry, label, label);
        }

        public Page AddButton(string labelZh, string labelEn, Action onClick)
        {
            return AddButton(labelZh, labelEn, onClick, 0f);
        }

        public Page AddButton(string label, Action onClick)
        {
            return AddButton(label, label, onClick, 0f);
        }

        /// <summary>添加按钮。<paramref name="width"/> ≤ 0 时使用内容区默认全宽。</summary>
        public Page AddButton(string labelZh, string labelEn, Action onClick, float width)
        {
            if (onClick == null)
            {
                throw new ArgumentNullException(nameof(onClick));
            }
            return Add(new ButtonItem(labelZh, labelEn, onClick, width));
        }

        public Page AddButton(string label, Action onClick, float width)
        {
            return AddButton(label, label, onClick, width);
        }

        public Page AddLabel(string textZh, string textEn)
        {
            return Add(new LabelItem(textZh, textEn));
        }

        public Page AddLabel(string text)
        {
            return AddLabel(text, text);
        }

        /// <summary>水平滑条 + 右侧数字输入，绑定 <see cref="ConfigEntry{T}"/>。</summary>
        public Page AddSlider(ConfigEntry<float> entry, float min, float max, string labelZh, string labelEn, int decimalPlaces = 3)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            return Add(new SliderItem(entry, min, max, labelZh, labelEn, decimalPlaces));
        }

        public Page AddSlider(ConfigEntry<float> entry, float min, float max, string label, int decimalPlaces = 3)
        {
            return AddSlider(entry, min, max, label, label, decimalPlaces);
        }

        /// <summary>仅数字输入框（无滑条），绑定 float 配置项。</summary>
        public Page AddFloatField(ConfigEntry<float> entry, float min, float max, string labelZh, string labelEn, int decimalPlaces = 6)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            return Add(new FloatFieldItem(entry, min, max, labelZh, labelEn, decimalPlaces));
        }

        /// <summary>二选一开关：左右为纯文本（leftText / rightText，如 米 / 格），轨道固定宽度左对齐；绑定 0=左、1=右。</summary>
        public Page AddSwitch(ConfigEntry<int> entry, string captionZh, string captionEn, string leftTextZh, string leftTextEn, string rightTextZh, string rightTextEn)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            return Add(new SwitchItem(entry, captionZh, captionEn, leftTextZh, leftTextEn, rightTextZh, rightTextEn));
        }

        public Page AddSwitch(ConfigEntry<int> entry, string leftTextZh, string leftTextEn, string rightTextZh, string rightTextEn)
        {
            return AddSwitch(entry, "", "", leftTextZh, leftTextEn, rightTextZh, rightTextEn);
        }

        /// <summary>多选一：每项一行单选圆点 + 说明文字。</summary>
        public Page AddSelection(ConfigEntry<int> entry, string captionZh, string captionEn, string[] optionZh, string[] optionEn)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            if (optionZh == null || optionZh.Length == 0)
            {
                throw new ArgumentException("At least one option is required.", nameof(optionZh));
            }
            if (optionEn == null || optionEn.Length != optionZh.Length)
            {
                optionEn = optionZh;
            }
            return Add(new SelectionItem(entry, captionZh, captionEn, optionZh, optionEn));
        }

        private Page Add(PageItem item)
        {
            Items.Add(item);
            UIPanel.Revision++;
            return this;
        }
    }

    internal abstract class PageItem
    {
        public readonly string TextZh;
        public readonly string TextEn;

        /// <summary>按当前游戏语言取文案（构建 UI 时调用）。</summary>
        public string Text => L10n.Pick(TextZh, TextEn);

        protected PageItem(string zh, string en)
        {
            TextZh = zh ?? "";
            TextEn = string.IsNullOrEmpty(en) ? TextZh : en;
        }
    }

    internal sealed class CheckboxItem : PageItem
    {
        public readonly ConfigEntry<bool> Entry;

        public CheckboxItem(ConfigEntry<bool> entry, string labelZh, string labelEn) : base(labelZh, labelEn)
        {
            Entry = entry;
        }
    }

    internal sealed class ButtonItem : PageItem
    {
        public readonly Action OnClick;
        /// <summary>像素宽度；≤ 0 表示铺满内容区。</summary>
        public readonly float Width;

        public ButtonItem(string labelZh, string labelEn, Action onClick, float width) : base(labelZh, labelEn)
        {
            OnClick = onClick;
            Width = width;
        }
    }

    internal sealed class LabelItem : PageItem
    {
        public LabelItem(string textZh, string textEn) : base(textZh, textEn)
        {
        }
    }

    internal sealed class SliderItem : PageItem
    {
        public readonly ConfigEntry<float> Entry;
        public readonly float Min;
        public readonly float Max;
        public readonly int DecimalPlaces;

        public SliderItem(ConfigEntry<float> entry, float min, float max, string labelZh, string labelEn, int decimalPlaces)
            : base(labelZh, labelEn)
        {
            Entry = entry;
            Min = min;
            Max = max;
            DecimalPlaces = decimalPlaces;
        }
    }

    internal sealed class FloatFieldItem : PageItem
    {
        public readonly ConfigEntry<float> Entry;
        public readonly float Min;
        public readonly float Max;
        public readonly int DecimalPlaces;

        public FloatFieldItem(ConfigEntry<float> entry, float min, float max, string labelZh, string labelEn, int decimalPlaces)
            : base(labelZh, labelEn)
        {
            Entry = entry;
            Min = min;
            Max = max;
            DecimalPlaces = decimalPlaces;
        }
    }

    internal sealed class SwitchItem : PageItem
    {
        public readonly ConfigEntry<int> Entry;
        public readonly string CaptionZh;
        public readonly string CaptionEn;
        public readonly string LeftZh;
        public readonly string LeftEn;
        public readonly string RightZh;
        public readonly string RightEn;

        public SwitchItem(ConfigEntry<int> entry, string captionZh, string captionEn, string leftZh, string leftEn, string rightZh, string rightEn)
            : base(captionZh, captionEn)
        {
            Entry = entry;
            CaptionZh = captionZh ?? "";
            CaptionEn = captionEn ?? captionZh ?? "";
            LeftZh = leftZh ?? "";
            LeftEn = string.IsNullOrEmpty(leftEn) ? LeftZh : leftEn;
            RightZh = rightZh ?? "";
            RightEn = string.IsNullOrEmpty(rightEn) ? RightZh : rightEn;
        }
    }

    internal sealed class SelectionItem : PageItem
    {
        public readonly ConfigEntry<int> Entry;
        public readonly string CaptionZh;
        public readonly string CaptionEn;
        public readonly string[] OptionsZh;
        public readonly string[] OptionsEn;

        public SelectionItem(ConfigEntry<int> entry, string captionZh, string captionEn, string[] optionZh, string[] optionEn)
            : base(captionZh, captionEn)
        {
            Entry = entry;
            CaptionZh = captionZh ?? "";
            CaptionEn = captionEn ?? captionZh ?? "";
            OptionsZh = optionZh;
            OptionsEn = optionEn;
        }
    }
}
