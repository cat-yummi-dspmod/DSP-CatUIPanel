using System.Globalization;

namespace YummiUIPanel
{
    /// <summary>
    /// 中英文文案。语言只在 <see cref="Refresh"/> 时读一次游戏设置，之后 <see cref="Pick"/> 只用缓存。
    /// </summary>
    public static class L10n
    {
        private static int cachedLcid = int.MinValue;

        /// <summary>重新读取游戏语言。语言没变时返回 false，不会在 <see cref="Pick"/> 里反复读。</summary>
        public static bool Refresh()
        {
            int next = ReadLcid();
            if (cachedLcid == next)
            {
                return false;
            }
            cachedLcid = next;
            return true;
        }

        public static string Pick(string zh, string en)
        {
            if (cachedLcid == int.MinValue)
            {
                Refresh();
            }
            if (string.IsNullOrEmpty(zh))
            {
                return en ?? "";
            }
            if (string.IsNullOrEmpty(en))
            {
                return zh;
            }
            return cachedLcid == 2052 ? zh : en;
        }

        private static int ReadLcid()
        {
            try
            {
                if (Localization.Loaded)
                {
                    return Localization.CurrentLanguageLCID;
                }
                return DSPGame.globalOption.languageLCID;
            }
            catch
            {
                // ignored
            }
            return (CultureInfo.CurrentUICulture.LCID & 0x3FF) == 0x04 ? 2052 : 1033;
        }
    }
}
