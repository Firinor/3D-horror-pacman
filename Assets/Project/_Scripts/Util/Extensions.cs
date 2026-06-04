using System.Collections.Generic;
using UnityEngine;

public static class Extensions
{
    public static T GetRandom<T>(this List<T> list)
    {
        int index = Random.Range(0, list.Count);
        return  list[index];
    }
    
    public static T PullRandom<T>(this List<T> list)
    {
        int index = Random.Range(0, list.Count);
        T result = list[index];
        list.Remove(result);
        return result;
    }
    
    public static void Shuffle<T>(this List<T> list)
    {
        int n = list.Count;
        while (n > 1) {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static void ClearAllChildren(this Transform parent)
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Object.DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
}