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
	Name: AutoIndicator.cs
	Project: AutoToot
	Author: Tom
	Created: 16th October 2022
*/

using UnityEngine;
using UnityEngine.UI;

namespace AutoToot;

public class AutoIndicator : MonoBehaviour
{
    private void Start()
    {
        gameObject.name = GameObjectName;
        
        Vector3 modifiedPosition = transform.position;
        modifiedPosition.y = YPosition;
        transform.position = modifiedPosition;
        
        _foregroundText = transform.Find(ForegroundObjectName).GetComponent<Text>();
        _shadowText = GetComponent<Text>();

        _foregroundText.text = "";
        _shadowText.text = "";
    }

    private void Update()
    {
        bool shouldShow = Plugin.IsActive;
        _foregroundText.text = shouldShow ? IndicatorText : "";
        _shadowText.text = shouldShow ? IndicatorText : "";
    }

    private const string GameObjectName = "AutoToot Indicator";
    private const string IndicatorText = "AutoToot Enabled";

    private const float YPosition = -4.7322f;
    
    private const string ForegroundObjectName = "maxcombo_text";

    private Text _foregroundText;
    private Text _shadowText;
}