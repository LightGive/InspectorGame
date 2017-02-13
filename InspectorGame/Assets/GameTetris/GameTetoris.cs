using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameTetoris : MonoBehaviour{ }

#if UNITY_EDITOR
[CustomEditor(typeof(GameTetoris))]
public class GameTetorisEditor : Editor
{
	private float deltaTime;
	private float prevTime;
	private float pos;

	public override void OnInspectorGUI()
	{
		CalcDeltaTime();
		GUILayout.Box("a", GUILayout.Width(pos));
		pos += deltaTime;
		if (pos > 400)
			pos = 0;
		Repaint();
	}

	void CalcDeltaTime()
	{
		float now = (float)EditorApplication.timeSinceStartup;
		deltaTime = (now - prevTime) * 800;
		prevTime = now;
	}
}
#endif

