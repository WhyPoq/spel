using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Constants
{
#if UNITY_EDITOR
    public static string packsDirectory = Application.dataPath + "/Packs/";
#else
    public static string packsDirectory = Application.persistentDataPath + "/Packs/";
#endif

    public static string packExtention = ".txt";

    public static Dictionary<int, int> levels = new Dictionary<int, int>
    {
        {1, 1},
        {2, 2},
        {3, 7},
        {4, 14},
        {5, 30},
        {6, 90},
        {7, -1}
    };

}
