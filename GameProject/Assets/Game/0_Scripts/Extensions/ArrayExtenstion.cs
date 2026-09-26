using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ArrayExtenstion
{
    public static T[] Shuffle<T>(this T[] array)
    {
        if(array.Length == 0) return default;
        for (int i = 0; i < array.Length; i++)
        {
            int r_value = UnityEngine.Random.Range(0, array.Length);

            //스위치
            T temp = array[i];
            array[i] = array[r_value];
            array[r_value] = temp;
        }
        return array;
    }
}