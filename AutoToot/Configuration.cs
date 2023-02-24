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
	Name: Configuration.cs
	Project: AutoToot
	Author: Tom
	Created: 21st October 2022
*/

using System;
using System.Reflection;
using AutoToot.Helpers;
using BepInEx.Configuration;
using UnityEngine;

namespace AutoToot;

public class Configuration
{
	internal Configuration(ConfigFile configFile)
	{
		Plugin.Logger.LogInfo($"Loading config...");

		ToggleKey = configFile.Bind("Keybinds", "ToggleKey", DefaultToggleKey,
			"The key used to toggle AutoToot on and off.");

		EaseFunction = configFile.Bind("Interpolation", "EaseFunction", DefaultEasingFunction,
			"The easing function to use for animating pointer position between notes."
			+ $"\nValid easing functions are: {String.Join(", ", GetValidEasingTypes())}."
			+ "\nPreview easing functions at: https://easings.net/");

		EarlyStart = configFile.Bind("Timing", "EarlyStart", DefaultEarlyStart,
			"Starts playing notes earlier by the given duration.");

		LateFinish = configFile.Bind("Timing", "LateFinish", DefaultLateFinish,
			"Finishes notes later by the given duration.");

		PerfectScore = configFile.Bind("Score", "PerfectScore", false,
			"Cheat the score returned by notes to achieve a perfect score.");
	}

	public void Validate()
	{
		if (typeof(Easing).GetMethod(EaseFunction.Value) == null)
		{
			Plugin.Logger.LogWarning(
				$"Easing function '{EaseFunction.Value}' does not exist, falling back to '{DefaultEasingFunction}'."
				+ $"\nValid easing functions are: {String.Join(", ", GetValidEasingTypes())}.");
			EaseFunction.Value = DefaultEasingFunction;
		}

		if (EarlyStart.Value < 0)
		{
			Plugin.Logger.LogWarning($"Early start time is less than zero, falling back to {DefaultEarlyStart}.");
			EarlyStart.Value = DefaultEarlyStart;
		}

		if (LateFinish.Value < 0)
		{
			Plugin.Logger.LogWarning($"Late finish time is less than zero, falling back to {DefaultLateFinish}.");
			EarlyStart.Value = DefaultLateFinish;
		}
	}

	private string[] GetValidEasingTypes()
	{
		MethodInfo[] methods = typeof(Easing).GetMethods();

		int methodCount = methods.Length - 4; //Last 4 methods are derived from object
		string[] types = new string[methodCount];

		for (int i = 0; i < methodCount; i++)
			types[i] = methods[i].Name;

		return types;
	}

	public ConfigEntry<KeyCode> ToggleKey { get; }
	public ConfigEntry<string> EaseFunction { get; }
	public ConfigEntry<int> EarlyStart { get; }
	public ConfigEntry<int> LateFinish { get; }
	public ConfigEntry<bool> PerfectScore { get; }

	private const KeyCode DefaultToggleKey = KeyCode.F8;
	private const string DefaultEasingFunction = "Linear";
	private const int DefaultEarlyStart = 8;
	private const int DefaultLateFinish = 8;
}