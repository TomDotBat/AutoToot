using System;
using System.Reflection;
using AutoToot.Helpers;
using BepInEx.Logging;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoToot;

public class Bot
{
    public Bot(GameController gameController)
    {
        _gameController = gameController;
        
        Type type = ((object) gameController).GetType(); //Removing the type cast will anger the compiler!!!
        _setPuppetShake = type.GetMethod("setPuppetShake", BindingFlags.NonPublic | BindingFlags.Instance);
        _playNote = type.GetMethod("playNote", BindingFlags.NonPublic | BindingFlags.Instance);
        _stopNote = type.GetMethod("stopNote", BindingFlags.NonPublic | BindingFlags.Instance);
        
        Logger.LogDebug("Captured necessary private GameController methods.");
        
        //Fetch important GameObjects
        Scene activeScene = SceneManager.GetActiveScene();
        _noteHolder = Hierarchy.FindSceneGameObjectByPath(activeScene, NotesHolderPath);
        _pointer = Hierarchy.FindSceneGameObjectByPath(activeScene, CursorPath);

        if (_noteHolder == null || _pointer == null)
        {
            Logger.LogError("Unable to locate the NotesHolder and TargetNote, Auto-Toot cannot function.");
            Plugin.IsActive = false;
        }
        else
        {
            Logger.LogDebug("Located NotesHolder and TargetNote.");
        }
    }

    public void Update(int noteIndex, float noteStartY, float noteStartTime, float noteEndTime, float notePShift, ref bool isPlaying)
    {
        if (noteIndex < 0)
            return;
        
        RectTransform noteHolderRectTransform = _noteHolder.GetComponent<RectTransform>();
        RectTransform pointerRectTransform = _pointer.GetComponent<RectTransform>();
        if (noteHolderRectTransform == null || pointerRectTransform == null)
            return;
        
        //The following code determines where the pointer should be based on decompiled code
        float zeroXPos = 60f;
        float noteHolderXPos = noteHolderRectTransform.anchoredPosition3D.x - zeroXPos;
        float time = noteHolderXPos <= 0.0 ? Mathf.Abs(noteHolderXPos) : -1f;
        
        Vector3 anchoredPosition = pointerRectTransform.anchoredPosition;
        
        anchoredPosition.y = noteStartY + EaseInOutVal(
            Mathf.Abs(1f - (noteEndTime - time) / (noteEndTime - noteStartTime)),
            0.0f, notePShift, 1f
        );
        
        pointerRectTransform.anchoredPosition = anchoredPosition;
        
        //Handle whether or not we should be tooting
        bool shouldToot = time >= noteStartTime - EarlyStart
                          && time <= noteEndTime + LateFinish;
        
        if (!isPlaying && shouldToot)
        {
            SetPuppetShake(true);
            PlayNote();
            isPlaying = true;
        }
        else if (isPlaying && !shouldToot)
        {
            SetPuppetShake(false);
            StopNote();
            isPlaying = false;
        }
    }

    private void SetPuppetShake(bool state)
    {
        _setPuppetShake.Invoke(_gameController, new object[] {state});
    }

    private void PlayNote()
    {
        _playNote.Invoke(_gameController, _noArgs);
    }
    
    private void StopNote()
    {
        _stopNote.Invoke(_gameController, _noArgs);
    }
    
    private float EaseInOutVal(float t, float b, float c, float d) //Pasted from dotpeek
    {
        t /= d / 2f;
        if (t < 1.0) return c / 2f * t * t + b;
        --t;
        return (float) (-(double) c / 2.0 * (t * (t - 2.0) - 1.0)) + b;
    }
    
    private ManualLogSource Logger => Plugin.Logger;

    private readonly GameController _gameController;
    private readonly GameObject _noteHolder;
    private readonly GameObject _pointer;

    private readonly MethodInfo _setPuppetShake;
    private readonly MethodInfo _playNote;
    private readonly MethodInfo _stopNote;
    
    private readonly object[] _noArgs = {};

    private const int EarlyStart = 8;
    private const int LateFinish = 8;
    
    private const string NotesHolderPath = "GameplayCanvas/GameSpace/NotesHolder";
    private const string CursorPath = "GameplayCanvas/GameSpace/TargetNote";
}