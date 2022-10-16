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
using AutoToot.Helpers;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
    static void Postfix(PointSceneController __instance)
    {
	    if (!Plugin.WasAutoUsed)
		    return;
    
        Scene activeScene = SceneManager.GetActiveScene();
        
        GameObject coin = Hierarchy.FindSceneGameObjectByPath(activeScene, CoinPath);
        if (coin == null)
        {
	        Plugin.Logger.LogError("Unable to find toots coin, it may be visible still.");
        }
        else
        {
	        coin.SetActive(false);
        }
        
        //Many sacrifices were made to make this to compile
        GameObject tootsObject = Hierarchy.FindSceneGameObjectByPath(activeScene, TootsTextPath);
        if (tootsObject == null)
        {
	        Plugin.Logger.LogError("Unable to find toots text, AutoToot indicator will not be present.");
        }
        else
        {
	        __instance.tootstext.text = "AutoTooted Play";
	        
	        Vector3 textPosition = tootsObject.transform.position;
	        textPosition.x = TootsTextXPosition;
	        tootsObject.transform.position = textPosition;
        }
        
        MethodInfo invoke = typeof(PointSceneController).GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
        if (invoke == null)
        {
	        Plugin.Logger.LogError("Failed to retrieve Invoke method from PointSceneController.");
        }
        else
        {
	        invoke.Invoke(__instance, new object[] {"showContinue", 0.75f});
        }
    }

    private const string CoinPath = "Canvas/coins+continue/coingroup/coin";
    private const string TootsTextPath = "Canvas/coins+continue/coingroup/Text";
    private const float TootsTextXPosition = -0.246f;
}