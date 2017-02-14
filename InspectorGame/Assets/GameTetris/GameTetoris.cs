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
	private const int WID_NUM = 10;
	private const int HEI_NUM = 20;

	private int[,] mass = new int[WID_NUM, HEI_NUM];
	private float deltaTime;
	private float prevTime;
	private float pos;

	public override void OnInspectorGUI()
	{
		CalcDeltaTime();
		Repaint();
	}
	
	void Init()
	{

	}

	void DisplayMass()
	{
		var windowWidth = EditorGUIUtility.currentViewWidth;

		for (int i = 0; i < WID_NUM; i++)
		{
			for (int j = 0; j < HEI_NUM; j++)
			{
				mass[i, j] = 0;
			}
		}
	}

	void CalcDeltaTime()
	{
		float now = (float)EditorApplication.timeSinceStartup;
		deltaTime = (now - prevTime) * 800;
		prevTime = now;
	}
}
#endif

