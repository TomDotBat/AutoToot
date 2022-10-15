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
	Name: Hierarchy.cs
	Project: AutoToot
	Author: Tom
	Created: 15th October 2022
*/

using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace AutoToot.Helpers;

public static class Hierarchy
{
    public static GameObject FindGameObjectByPath(GameObject rootObj, string[] path)
    {
        for (int i = 0; i < rootObj.transform.childCount; i++)
        {
            GameObject childObj = rootObj.transform.GetChild(i).gameObject;
            if (childObj.name != path[0]) continue;
            if (path.Length <= 1) return childObj;

            path = path.Skip(1).ToArray();
            if (path.Length == 0) continue;
            return FindGameObjectByPath(childObj, path);
        }
        
        return null;
    }

    public static GameObject FindGameObjectByPath(GameObject rootObj, string path)
    {
        return FindGameObjectByPath(rootObj, path.Split('/'));
    }
    
    public static GameObject FindSceneGameObjectByPath(Scene scene, string[] path)
    {
        foreach (var obj in scene.GetRootGameObjects())
        {
            if (obj.name != path[0]) continue;
            
            path = path.Skip(1).ToArray();
            return FindGameObjectByPath(obj, path);
        }

        return null;
    }

    public static GameObject FindSceneGameObjectByPath(Scene scene, string path)
    {
        return FindSceneGameObjectByPath(scene, path.Split('/'));
    }
    
    public static ArrayList FindObjectsWithComponent<T>(GameObject rootObj, ArrayList list)
    {
        for (int i = 0; i < rootObj.transform.childCount; i++)
        {
            GameObject childObj = rootObj.transform.GetChild(i).gameObject;
            
            list = FindObjectsWithComponent<T>(childObj, list);
            
            if (childObj.GetComponent<T>() == null) continue;
            list.Add(childObj);
        }
        
        return list;
    }

    public static ArrayList FindObjectsWithComponent<T>(GameObject rootObj)
    {
        return FindObjectsWithComponent<T>(rootObj, new ArrayList());
    }

    public static ArrayList FindObjectsWithComponent<T>(Scene scene)
    {
        ArrayList result = new ArrayList();
        
        foreach (var obj in scene.GetRootGameObjects())
            result.AddRange(FindObjectsWithComponent<T>(obj, result));

        return result;
    }
}