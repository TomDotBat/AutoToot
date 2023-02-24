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
	Name: Plugin.cs
	Project: AutoToot
	Author: Tom
	Created: 15th October 2022
*/

using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoToot;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
	    Logger = base.Logger;
        Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}.");

        Configuration = new Configuration(Config);
        Configuration.Validate();
        
        Harmony harmony = new Harmony(PluginInfo.PLUGIN_GUID);
        harmony.PatchAll();

        SceneManager.sceneLoaded += OnSceneLoaded;
        Logger.LogDebug("Added sceneLoaded delegate.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (mode == LoadSceneMode.Additive)
            return;

        Logger.LogDebug($"Level {scene.buildIndex} ({scene.name}) was loaded.");

        if (scene.name == GameplaySceneName)
        {
            Logger.LogInfo("Gameplay scene loaded.");
            _isInGameplay = true;
            WasAutoUsed = false;
        }
        else if (_isInGameplay)
        {
            Logger.LogInfo("Gameplay scene unloaded.");
            _isInGameplay = false;
            IsActive = false;
            Bot = null;
        }
    }

    private static void SetupBot()
    {
	    GameObject gameControllerObject = GameObject.Find(GameControllerPath);
	    if (gameControllerObject == null)
	    {
		    Logger.LogError("Unable to find the GameController object, Auto-Toot cannot function.");
	    }
	    else
	    {
		    GameController gameController = gameControllerObject.GetComponent<GameController>();
		    if (gameController == null)
		    {
			    Logger.LogError("Unable to retrieve the GameController component, Auto-Toot cannot function.");
		    }
		    else
		    {
			    Bot = new Bot(gameController);
		    }
	    }
    }

    public static void ToggleActive() => IsActive = !_isActive;

    public static bool IsActive
    {
        get => _isActive;
        set
        {
	        if (_isActive == value) return;

	        if (value)
	        {
		        SetupBot();
		        WasAutoUsed = true;
	        }

	        _isActive = value;
	        Logger.LogInfo($"{(_isActive ? "Enabled" : "Disabled")} Auto-Toot.");
        }
    }

    public static bool WasAutoUsed { get; private set; }
    
    public static Bot Bot { get; private set; }
    public static Configuration Configuration { get; private set; }
    
    internal new static ManualLogSource Logger { get; private set; }

    private static bool _isActive;
    private static bool _isInGameplay;
    
    private const string GameplaySceneName = "gameplay";
    private const string GameControllerPath = "GameController";
}