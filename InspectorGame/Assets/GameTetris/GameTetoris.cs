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
	private const int WALL_NUM = 2;
	private const int WID_NUM = 10;
	private const int HEI_NUM = 20;
	private const int MYMASS_NUM = 4;

	private int[,] mass = new int[WID_NUM, HEI_NUM];
	private int[,] myMass = new int[MYMASS_NUM, MYMASS_NUM];
	private bool isStart = false;
	private float deltaTime;
	private float prevTime;
	private float pos;

	public enum BlockInfo
	{
		Wall=-1,
		None=0,
		Square=1,
	}

	public override void OnInspectorGUI()
	{
		if(!isStart)
		{
			if (GUILayout.Button("Start"))
			{
				Init();
			}
			return;
		}

		CalcDeltaTime();
		Repaint();
	}
	
	void Init()
	{
		isStart = true;

		for (int i = 0; i < WID_NUM; i++)
		{
			for (int j = 0; j < HEI_NUM; j++)
			{
				mass[i, j] = 0;
			}
		}
	}

	void DisplayMass()
	{
		var windowWidth = EditorGUIUtility.currentViewWidth;

		for (int i = 0; i < WID_NUM; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < HEI_NUM; j++)
			{
				GUI.color = GetColor(mass[i, j]);
				GUILayout.Box("");
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	Color GetColor(int _massNum)
	{
		switch (_massNum)
		{
			case 0: return Color.white;
			case 1: return Color.blue;
			case 2: return Color.red;
			case 3: return Color.green;
			case 4: return Color.yellow;
		}

		return Color.white;
	}

	void CalcDeltaTime()
	{
		float now = (float)EditorApplication.timeSinceStartup;
		deltaTime = (now - prevTime) * 800;
		prevTime = now;
	}
}
#endif

