using System;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine.SceneManagement;

namespace AutoToot;

[BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    private void Awake()
    {
        Logger = base.Logger;
        Logger.LogInfo($"Loaded {PluginInfo.PLUGIN_NAME} v{PluginInfo.PLUGIN_VERSION}.");

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
            IsInGameplay = true;
        }
        else if (IsInGameplay)
        {
            Logger.LogInfo("Gameplay scene unloaded.");
            IsInGameplay = false;
            IsActive = false;
            Bot = null;
        }
    }

    public static bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            Logger.LogInfo($"{(_isActive ? "Enabled" : "Disabled")} Auto-Toot.");
        }
    }

    public static void ToggleActive()
    {
        IsActive = !_isActive;
    }
    
    public static bool IsInGameplay { get; private set; }
    
    public static Bot Bot { get; internal set; }
    
    internal new static ManualLogSource Logger { get; private set; }

    private static bool _isActive = false;
    
    private const string GameplaySceneName = "gameplay";
}