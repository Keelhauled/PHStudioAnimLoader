using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Harmony;
using BepInEx.Logging;
using HarmonyLib;
using System.Collections;

namespace StudioAnimLoader
{
    [BepInProcess("PlayHomeStudio64bit")]
    [BepInProcess("PlayHomeStudio32bit")]
    [BepInPlugin(GUID, Name, Version)]
    public class StudioAnimLoader : BaseUnityPlugin
    {
        public const string GUID = "studioanimloader";
        public const string Name = "StudioAnimLoader";
        public const string Version = "0.0.2.0";

        internal static new ManualLogSource Logger;

        internal static ConfigEntry<string> InfoDir { get; set; }
        internal static ConfigEntry<string> OtherGameDir { get; set; }
        internal static ConfigEntry<string> GroupSuffix { get; set; }
        internal static ConfigEntry<int> GroupOffset { get; set; }
        internal static ConfigEntry<bool> Overwrite { get; set; }

        private void Awake()
        {
            Logger = base.Logger;

            InfoDir = Config.Bind("General", "InfoDir", "StudioAnimLoader");
            OtherGameDir = Config.Bind("General", "OtherGameDir", "");
            GroupSuffix = Config.Bind("General", "GroupSuffix", "[MOD]");
            GroupOffset = Config.Bind("General", "GroupOffset", 100);
            Overwrite = Config.Bind("General", "Overwrite", false);

            HarmonyWrapper.PatchAll(GetType());
        }

        [HarmonyPostfix, HarmonyPatch(typeof(StudioScene), "Start")]
        private static void StudioStart(ref object __result)
        {
            __result = new[] { __result, AddLoaderComponent() }.GetEnumerator();

            IEnumerator AddLoaderComponent()
            {
                Chainloader.ManagerObject.AddComponent<LoaderComponent>();
                yield break;
            }
        }
    }
}