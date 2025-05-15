using Distance.ReplayIntensifies.Randomizer;
using HarmonyLib;
using System.IO;

namespace Distance.ReplayIntensifies.Patches
{
    [HarmonyPatch(typeof(SplashScreenLogic), "Start")]
    internal static class SplashScreenLogic__Start
    {
        [HarmonyPostfix]
        internal static void CreateSettingList()
        {
            string fileName = "Car Weights.json";
            string filePath = Path.Combine(System.Reflection.Assembly.GetCallingAssembly().Location, fileName);

            if (!File.Exists(filePath))
            {
                Mod.Instance.SetCarWeights(RandomCarType.AllowedVanillaCarNames, null, 1f);
                Mod.Instance.SetCarWeights(RandomCarType.CustomCarNames, null, 1f);
            }
        }
    }
}
