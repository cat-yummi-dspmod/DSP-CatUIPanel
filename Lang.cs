using System.Globalization;

namespace YummiUIHarness
{
    internal static class Lang
    {
        public static bool IsChinese
        {
            get
            {
                int lcid;
                try
                {
                    lcid = Localization.Loaded ? Localization.CurrentLanguageLCID : CultureInfo.CurrentUICulture.LCID;
                }
                catch
                {
                    lcid = CultureInfo.CurrentUICulture.LCID;
                }
                return (lcid & 0x3FF) == 0x04;
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
