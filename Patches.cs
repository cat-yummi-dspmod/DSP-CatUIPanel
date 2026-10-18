using HarmonyLib;
using YummiUIPanel.UI;

namespace YummiUIPanel
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
    }
}
