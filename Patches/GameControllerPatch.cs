/*
    COPYRIGHT NOTICE:
	© 2022 Thomas O'Sullivan - All rights reserved.

	Permission is hereby granted, free of charge, to any person obtaining a copy
	of this software and associated documentation files (the "Software"), to deal
	in the Software without restriction, including without limitation the rights
	to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
	copies of the Software, and to permit persons to whom the Software is
	furnished to do so, subject to the following conditions:

	The above copyright notice and this permission notice shall be included in all
	copies or substantial portions of the Software.

	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
	IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
	FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
	AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
	LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
	OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
	SOFTWARE.

	FILE INFORMATION:
	Name: GameControllerPatch.cs
	Project: AutoToot
	Author: Tom
	Created: 15th October 2022
*/

using System.Security.Permissions;
using HarmonyLib;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace AutoToot.Patches;

[HarmonyPatch(typeof(GameController), nameof(GameController.Start))]
internal class GameControllerStartPatch
{
    static void Postfix()
    {
        GameObject comboObject = GameObject.Find(ComboPath);
        if (comboObject == null)
        {
            Plugin.Logger.LogError("Unable to find combo text, the auto toot indicator will not be present.");
        }
        else
        {
            GameObject autoIndicator = Object.Instantiate(comboObject, comboObject.transform);
            autoIndicator.AddComponent<AutoIndicator>();

            GameObject parent = GameObject.Find(ParentPath);
            if (parent == null)
            {
                Plugin.Logger.LogError("Unable to find the UIHolder to re-parent the indicator, placement may be wrong.");
            }
            else
            {
                autoIndicator.transform.parent = parent.transform;
            }
        }
    }

    private const string ComboPath = "GameplayCanvas/UIHolder/maxcombo/maxcombo_shadow";
    private const string ParentPath = "GameplayCanvas/UIHolder";
}

[HarmonyPatch(typeof(GameController), nameof(GameController.Update))]
internal class GameControllerUpdatePatch
{
    static void Postfix(GameController __instance)
    {
        if (__instance.freeplay || __instance.paused)
            return;

        if (Input.GetKeyDown(Plugin.Configuration.ToggleKey.Value))
            Plugin.ToggleActive();

        __instance.controllermode = Plugin.IsActive; //Disables user input for us, nice shortcut!!

        if (Plugin.IsActive)
        {
            Plugin.Bot.Update();
            if (Plugin.Bot.shouldPlayPerfect)
            {
                __instance.released_button_between_notes = true; // no need to release toot between notes because some pepega maps have 2 notes on the same frame
                __instance.breathcounter = 0f;
            }
        }
    }
}

[HarmonyPatch(typeof(GameController), nameof(GameController.isNoteButtonPressed))]
internal class GameControllerIsNoteButtonPressedPatch
{
    static void Postfix(ref bool __result)
    {
        if (Plugin.IsActive)
            __result = Plugin.Bot.isTooting;
    }
}

[HarmonyPatch(typeof(GameController), nameof(GameController.getScoreAverage))]
internal class GameControllerGetScoreAveragePatch
{
    static void Prefix(GameController __instance)
    {
        if (Plugin.IsActive && Plugin.Bot.shouldPlayPerfect)
        {
            __instance.notescoreaverage = 100f;
        }
    }
}

[HarmonyPatch(typeof(GameController), nameof(GameController.doScoreText))]
internal class GameControllerDoScoreTextPatch
{
    static void Prefix(object[] __args)
    {
        if (Plugin.IsActive && Plugin.Bot.shouldPlayPerfect)
        {
            __args[0] = 4; // note tally, 4 being perfect
            __args[1] = 100f; // note score average just to make sure xd
        }
    }
}


[HarmonyPatch(typeof(GameController), nameof(GameController.pauseRetryLevel))]
internal class GameControllerRetryPatch
{
    static void Postfix(GameController __instance)
    {
        if (Plugin.IsActive) Plugin.IsActive = false;
    }
}
