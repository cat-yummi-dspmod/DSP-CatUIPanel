using HarmonyLib;
using YummiUIHarness.UI;

namespace YummiUIHarness
{
    internal static class Patches
    {
        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIGame), "_OnUpdate")]
        public static void UIGame_OnUpdate_Prefix()
        {
            HarnessWindow.OnGameUiUpdate();
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(GameMain), nameof(GameMain.End))]
        public static void GameMain_End_Postfix()
        {
            HarnessWindow.OnGameEnd();
        }
    }
}
