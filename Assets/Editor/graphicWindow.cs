using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class graphicWindow : EditorWindow
{
	float[] points;
    [MenuItem("Tools/График")]
    public static void ShowMyEditor()
    {
        // This method is called when the user selects the menu item in the Editor
        EditorWindow wnd = GetWindow<graphicWindow>();
        wnd.titleContent = new GUIContent("График");
        //public AnimationCurve ac;

    }
	public void Update()
	{
		if (TempShaderTest.instance == null) return;
		Repaint();
	}
	void OnGUI() {
		if (TempShaderTest.instance == null) return;
		EditorGUILayout.BeginVertical();
		points = TempShaderTest.instance.GetNowDataU();
		var rect = new Rect(0, 0, position.width, position.height);
		EditorGUI.DrawRect(rect, Color.black);

		float yMin = -100; 
		float yMax = TempShaderTest.instance.centerTemperature;
		float coef = position.width / points.Length;
		float step = 1 / position.width; 

		Vector3 prevPos = new Vector3(0, points[0], 0);
		int counter = 1;
		try
		{
			for (float t = step; t < 1; t += step)
			{
				if (counter >= points.Length) break;
				Vector3 pos = new Vector3(t* coef, points[counter++], 0);
				UnityEditor.Handles.DrawLine(
					new Vector3(rect.xMin + prevPos.x * rect.width, rect.yMax - ((prevPos.y - yMin) / (yMax - yMin)) * rect.height, 0),
					new Vector3(rect.xMin + pos.x * rect.width, rect.yMax - ((pos.y - yMin) / (yMax - yMin)) * rect.height, 0));

				prevPos = pos;
			}
		}
		catch (Exception e) {
			Debug.Log(counter);
		}

		EditorGUILayout.EndVertical();
	}

	//float curveFunc(float t) {
	//	return Mathf.Sin(t * 2 * Mathf.PI);
	//}

}
