using System.Reflection;
using HarmonyLib;

namespace AutoToot.Patches;

[HarmonyPatch(typeof(PointSceneController), "Start")]
internal class PointSceneControllerStartPatch
{
    static void Prefix()
    {
        if (Plugin.WasAutoUsed) --GlobalVariables.localsave.tracks_played;
    }
}

[HarmonyPatch(typeof(PointSceneController), "updateSave")]
internal class PointSceneControllerUpdateSavePatch
{
    static bool Prefix()
    {
        return !Plugin.WasAutoUsed;
    }
}

[HarmonyPatch(typeof(PointSceneController), "checkScoreCheevos")]
internal class PointSceneControllerAchievementsCheckPatch
{
    static bool Prefix()
    {
        return !Plugin.WasAutoUsed;
    }
}

[HarmonyPatch(typeof(PointSceneController), "doCoins")]
internal class PointSceneControllerDoCoinsPatch
{
    static bool Prefix(PointSceneController __instance)
    {
        if (Plugin.WasAutoUsed)
        {
            MethodInfo invoke = typeof(PointSceneController).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
            
            if (invoke != null)
            {
                invoke.Invoke(__instance, new object[] {"showContinue", 0.75f});
                return false;
            }
            
            Plugin.Logger.LogError("Failed to retrieve Invoke method from PointSceneController.");
        }
        
        return true;
    }
}