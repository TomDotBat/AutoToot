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