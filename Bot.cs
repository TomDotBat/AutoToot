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

        _noteHolder = GameObject.Find(NotesHolderPath)?.GetComponent<RectTransform>();
        _pointer = GameObject.Find(CursorPath)?.GetComponent<RectTransform>();

        if (_noteHolder == null || _pointer == null)
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
        if (_gameController.currentnoteindex < 0)
            return;

        //The following code determines where the pointer should be based on decompiled code
        float zeroXPos = 60f;
        float noteHolderXPos = _noteHolder.anchoredPosition3D.x - zeroXPos;
        float time = noteHolderXPos <= 0.0 ? Mathf.Abs(noteHolderXPos) : -1f;

        float noteStartTime = _gameController.currentnotestart - EarlyStart;
        float noteEndTime = _gameController.currentnoteend + LateFinish;

        //Handle whether or not we should be tooting
        bool shouldToot = !_gameController.outofbreath
                          && time >= noteStartTime
                          && time <= noteEndTime;

        Vector2 pointerPosition = _pointer.anchoredPosition;

        if (_gameController.noteplaying)
        {
            pointerPosition.y = _gameController.currentnotestarty + _gameController.easeInOutVal(
                Mathf.Abs(1f - (noteEndTime - time) / (noteEndTime - noteStartTime)),
                0.0f, _gameController.currentnotepshift, 1f
            );
        }
        else
        {
            float progressToNextNote = 1f - (noteStartTime - time) / (noteStartTime - _lastNoteEndTime);
            pointerPosition.y = Mathf.Lerp( _lastNoteEndY, _gameController.currentnotestarty, progressToNextNote);
        }

        if (!_gameController.noteplaying && shouldToot)
        {
            _gameController.setPuppetShake(true);
            _gameController.playNote();
            _gameController.noteplaying = true;

            _lastNoteEndTime = noteEndTime;
            _lastNoteEndY = _gameController.currentnoteendy;
        }
        else if (_gameController.noteplaying && !shouldToot)
        {
            _gameController.setPuppetShake(false);
            _gameController.stopNote();
            _gameController.noteplaying = false;
        }

        _pointer.anchoredPosition = pointerPosition;
        _humanPuppetController.doPuppetControl(-pointerPosition.y / GameCanvasSize * 2);
    }

    private ManualLogSource Logger => Plugin.Logger;

    private readonly GameController _gameController;
    private readonly HumanPuppetController _humanPuppetController;
    private readonly RectTransform _noteHolder;
    private readonly RectTransform _pointer;

    private const int EarlyStart = 8;
    private const int LateFinish = 8;

    private const float GameCanvasSize = 450f;

    private float _lastNoteEndTime;
    private float _lastNoteEndY;

    private const string NotesHolderPath = "GameplayCanvas/GameSpace/NotesHolder";
    private const string CursorPath = "GameplayCanvas/GameSpace/TargetNote";
}