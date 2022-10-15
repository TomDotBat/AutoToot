using HarmonyLib;
using UnityEngine;

namespace AutoToot.Patches;

[HarmonyPatch(typeof(GameController), "Update")]
internal class GameControllerUpdatePatch
{
    static bool Prefix(GameController __instance,
        int ___currentnoteindex, float ___currentnotestarty, float ___currentnotestart, float ___currentnoteend, float ___currentnotepshift,
        ref bool ___noteplaying)
    {
        if (!Plugin.IsInGameplay)
            return true;

        if (Input.GetKeyDown(KeyCode.F8))
            Plugin.ToggleActive();
        
        __instance.controllermode = Plugin.IsActive; //Disables user input for us, nice shortcut!!

        if (Plugin.IsActive)
        {
            if (Plugin.Bot == null)
            {
                Plugin.Bot = new Bot(__instance);
            }
            else
            {
                Plugin.Bot.Update(
                    ___currentnoteindex, 
                    ___currentnotestarty, ___currentnotestart, ___currentnoteend, ___currentnotepshift,
                    ref ___noteplaying
                );
            }
        }

        return true;
    }
}