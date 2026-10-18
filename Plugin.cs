using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;

namespace YummiUIHarness
{
    [BepInPlugin(Guid, "YummiUIHarness", Version)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Guid = "com.yummi.dsp.yummiuiharness";
        public const string Version = "0.1.4";

        private static readonly Harmony harmony = new Harmony(Guid);
        internal static ManualLogSource Log;
        internal static ConfigEntry<float> LauncherOffsetX;
        internal static ConfigEntry<float> LauncherOffsetY;

        private void Awake()
        {
            Log = Logger;
            LauncherOffsetX = Config.Bind("Launcher", "OffsetX", 0f,
                "Horizontal offset of the cat button relative to the screenshot button. | 小猫按钮相对截图按钮的水平偏移。");
            LauncherOffsetY = Config.Bind("Launcher", "OffsetY", 4f,
                "Gap between the cat button and the screenshot button below it. | 小猫按钮与下方截图按钮的间距。");
            harmony.PatchAll(typeof(Patches));
        }

        private void OnDestroy()
        {
            harmony.UnpatchSelf();
        }
    }
}
