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

using System.Security.Permissions;
using BepInEx.Logging;
using UnityEngine;

#pragma warning disable CS0618
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]

namespace AutoToot;

public class Bot
{
    public Bot(GameController gameController)
    {
        _gameController = gameController;
        _humanPuppetController = gameController.puppet_humanc;

        _noteHolderPosition = GameObject.Find(NotesHolderPath)?.GetComponent<RectTransform>();
        _pointer = GameObject.Find(CursorPath)?.GetComponent<RectTransform>();

        if (_noteHolderPosition == null || _pointer == null)
        {
            Logger.LogError("Unable to locate the NotesHolder and Pointer, Auto-Toot cannot function.");
            Plugin.IsActive = false;
        }
        else
        {
            Logger.LogDebug("Located NotesHolder and Pointer.");
        }
    }

    public void Update()
    {
	    if (_gameController.currentnoteindex > -1)
	    {
		    float currentTime = GetTime();
    
		    float noteStartTime = _gameController.currentnotestart - EarlyStart;
		    float noteEndTime = _gameController.currentnoteend + LateFinish;

		    float pointerY = GetPointerY(currentTime, noteStartTime, noteEndTime);

		    Vector2 pointerPosition = _pointer.anchoredPosition;
		    pointerPosition.y = pointerY;
		    _pointer.anchoredPosition = pointerPosition;

		    HandleTooting(pointerY, currentTime, noteStartTime, noteEndTime);
		    _humanPuppetController.doPuppetControl(-pointerY / GameCanvasSize * 2);
	    }
    }

    private float GetTime()
    {
        float noteHolderX = _noteHolderPosition.anchoredPosition3D.x - NotesHolderZeroOffset;
        return noteHolderX <= 0f ? Mathf.Abs(noteHolderX) : -1f;
    }

    private float GetPointerY(float currentTime, float noteStartTime, float noteEndTime)
    {
	    if (_gameController.noteplaying)
	    {
		    return _gameController.currentnotestarty + _gameController.easeInOutVal(
			    Mathf.Abs(1f - (noteEndTime - currentTime) / (noteEndTime - noteStartTime)),
			    0f, _gameController.currentnotepshift, 1f
		    );
	    }

	    return Mathf.Lerp( _lastNoteEndY, _gameController.currentnotestarty, 
		    1f - (noteStartTime - currentTime) / (noteStartTime - _lastNoteEndTime));
	}

    private bool ShouldToot(float currentTime, float noteStartTime, float noteEndTime)
    {
	    return !_gameController.outofbreath
	           && currentTime >= noteStartTime
	           && currentTime <= noteEndTime;
    }

    private void OnTootStateChange(bool isTooting)
    {
	    _gameController.setPuppetShake(isTooting);
	    _gameController.noteplaying = isTooting;

	    if (isTooting) _gameController.playNote();
	    else _gameController.stopNote();
    }

    private void HandleTooting(float pointerY, float currentTime, float noteStartTime, float noteEndTime)
    {
	    bool shouldToot = ShouldToot(currentTime, noteStartTime, noteEndTime);

	    if (!_gameController.noteplaying && shouldToot)
	    {
		    OnTootStateChange(true);
		    _lastNoteEndTime = noteEndTime;
		    _lastNoteEndY = pointerY;
	    }
	    else if (_gameController.noteplaying && !shouldToot)
	    {
		    OnTootStateChange(false);
	    }
    }

    private ManualLogSource Logger => Plugin.Logger;

    private float _lastNoteEndTime;
    private float _lastNoteEndY;

    private readonly GameController _gameController;
    private readonly HumanPuppetController _humanPuppetController;
    private readonly RectTransform _noteHolderPosition;
    private readonly RectTransform _pointer;

    private const int EarlyStart = 8;
    private const int LateFinish = 8;

    private const float GameCanvasSize = 450f;
    private const float NotesHolderZeroOffset = 60f;
    
    private const string NotesHolderPath = "GameplayCanvas/GameSpace/NotesHolder";
    private const string CursorPath = "GameplayCanvas/GameSpace/TargetNote";
}