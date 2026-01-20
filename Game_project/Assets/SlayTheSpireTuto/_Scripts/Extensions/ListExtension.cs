using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ListExtension
{
    public static T Draw<T>(this List<T> list)
    {
        if (list.Count == 0) return default;
        T t = list[0];
        list.Remove(t);
        return t;
    }
    public static List<T> Shuffle<T>(this List<T> list)
    {
        if (list.Count == 0) return default;
        T[] array = new T[list.Count];
        for (int i = 0; i < array.Length; i++)
        {
            int r = UnityEngine.Random.Range(0, list.Count);
            array[i] = list[r];
            list.RemoveAt(r);
        }
        return array.ToList();
    }
}
