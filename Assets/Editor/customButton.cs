using UnityEngine;
using System.Collections;
using UnityEditor;

public class customButton : Editor
{
    [MenuItem("Tools/Сохранить выборку")]
    private static void NewMenuOption()
    {
        if (TempShaderTest.instance) {
            TempShaderTest.instance.Save();
        }
    }

}