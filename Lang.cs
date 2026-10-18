using System.Globalization;

namespace YummiUIHarness
{
    /// <summary>按游戏语言在中英文文案间切换（在 UI 生成时解析，勿在 mod Awake 里 Pick 后存死）。</summary>
    public static class L10n
    {
        public static bool IsChinese
        {
            get
            {
                try
                {
                    if (Localization.Loaded)
                    {
                        return Localization.isZHCN;
                    }
                    return DSPGame.globalOption.languageLCID == 2052;
                }
                catch
                {
                    // ignored
                }
                return (CultureInfo.CurrentUICulture.LCID & 0x3FF) == 0x04;
            }
        }

        public static string Pick(string zh, string en)
        {
            if (string.IsNullOrEmpty(zh))
            {
                return en ?? "";
            }
            if (string.IsNullOrEmpty(en))
            {
                return zh;
            }
            return IsChinese ? zh : en;
        }
    }
}
