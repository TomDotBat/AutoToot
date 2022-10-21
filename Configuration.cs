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

using BepInEx.Configuration;

namespace AutoToot;

public class Configuration
{
    internal Configuration(ConfigFile configFile)
    {
        Plugin.Logger.LogInfo($"Loading config...");

        EarlyStart = configFile.Bind("Timing", "EarlyStart", 8,
            "Starts playing notes earlier by the given duration.");

        LateFinish = configFile.Bind("Timing", "LateFinish", 8,
            "Finishes notes later by the given duration.");
    }
    
    public ConfigEntry<int> EarlyStart { get; }
    public ConfigEntry<int> LateFinish { get; }
}