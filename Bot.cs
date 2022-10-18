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
	Name: Bot.cs
	Project: AutoToot
	Author: Tom
	Created: 15th October 2022
*/

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
        
        Type type = typeof(GameController);
        _setPuppetShake = type.GetMethod("setPuppetShake", BindingFlags.NonPublic | BindingFlags.Instance);
        _playNote = type.GetMethod("playNote", BindingFlags.NonPublic | BindingFlags.Instance);
        _stopNote = type.GetMethod("stopNote", BindingFlags.NonPublic | BindingFlags.Instance);
        _outOfBreath = type.GetField("outofbreath", BindingFlags.NonPublic | BindingFlags.Instance);

        FieldInfo puppetField = type.GetField("puppet_humanc", BindingFlags.NonPublic | BindingFlags.Instance);
        if (puppetField == null)
        {
            Logger.LogError("Unable to retrieve HumanPuppetController, the character will not move.");
        }
        else
        {
            _humanPuppetController = puppetField.GetValue(_gameController) as HumanPuppetController;
        }

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

    public void Update(int noteIndex, float noteStartY, float noteEndY, float noteStartTime, float noteEndTime, float notePShift, ref bool isPlaying)
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
        
        noteStartTime -= EarlyStart;
        noteEndTime += LateFinish;
        
        //Handle whether or not we should be tooting
        bool shouldToot = !IsOutOfBreath()
                          && time >= noteStartTime
                          && time <= noteEndTime;

        Vector2 pointerPosition = pointerRectTransform.anchoredPosition;

        if (isPlaying)
        {
            pointerPosition.y = noteStartY + EaseInOutVal(
                Mathf.Abs(1f - (noteEndTime - time) / (noteEndTime - noteStartTime)),
                0.0f, notePShift, 1f
            );
        }
        else
        {
            float progressToNextNote = 1f - (noteStartTime - time) / (noteStartTime - _lastNoteEndTime);
            pointerPosition.y = Mathf.Lerp( _lastNoteEndY, noteStartY, progressToNextNote);
        }
        
        if (!isPlaying && shouldToot)
        {
            SetPuppetShake(true);
            PlayNote();
            isPlaying = true;
            
            _lastNoteEndTime = noteEndTime;
            _lastNoteEndY = noteEndY;
        }
        else if (isPlaying && !shouldToot)
        {
            SetPuppetShake(false);
            StopNote();
            isPlaying = false;
        }
        
        pointerRectTransform.anchoredPosition = pointerPosition;
        DoPuppetControl(-pointerPosition.y / GameCanvasSize * 2);
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
    
    private void DoPuppetControl(float vp)
    {
        _humanPuppetController.doPuppetControl(vp);
    }

    private bool IsOutOfBreath()
    {
        return (bool) _outOfBreath.GetValue(_gameController);
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
    private readonly HumanPuppetController _humanPuppetController;
    private readonly GameObject _noteHolder;
    private readonly GameObject _pointer;

    private readonly MethodInfo _setPuppetShake;
    private readonly MethodInfo _playNote;
    private readonly MethodInfo _stopNote;
    private readonly FieldInfo _outOfBreath;

    private readonly object[] _noArgs = {};

    private const int EarlyStart = 8;
    private const int LateFinish = 8;
    
    private const float GameCanvasSize = 450f;

    private float _lastNoteEndTime;
    private float _lastNoteEndY;

    private const string NotesHolderPath = "GameplayCanvas/GameSpace/NotesHolder";
    private const string CursorPath = "GameplayCanvas/GameSpace/TargetNote";
}