using HarmonyLib;
using CatUIPanel.UI;

namespace CatUIPanel
{
    internal static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIGame), "_OnUpdate")]
        public static void UIGame_OnUpdate_Prefix()
        {
            PanelWindow.OnGameUiUpdate();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.End))]
        public static void GameMain_End_Postfix()
        {
            PanelWindow.OnGameEnd();
        }

        /// <summary>只在游戏广播换语言时进来一次。Pick 不再读语言。</summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Localization), nameof(Localization.NotifyLanguageChange))]
        public static void Localization_NotifyLanguageChange_Postfix()
        {
            if (!L10n.Refresh())
            {
                return;
            }
            try
            {
                LauncherButton.ApplyLanguage();
                PanelWindow.OnLanguageChanged();
            }
            catch (System.Exception e)
            {
                Plugin.Log.LogError("Language refresh failed: " + e);
            }
        }
    }
}
