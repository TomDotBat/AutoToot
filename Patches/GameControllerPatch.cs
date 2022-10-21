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
    static bool Prefix(GameController __instance)
    {
        if (__instance.freeplay || __instance.paused)
            return true;

        if (Input.GetKeyDown(Plugin.Configuration.ToggleKey.Value))
            Plugin.ToggleActive();
        
        __instance.controllermode = Plugin.IsActive; //Disables user input for us, nice shortcut!!

        if (Plugin.IsActive)
	        Plugin.Bot.Update();

        return true;
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
