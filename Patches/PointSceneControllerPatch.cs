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
	Name: PointSceneControllerPatch.cs
	Project: AutoToot
	Author: Tom
	Created: 16th October 2022
*/

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