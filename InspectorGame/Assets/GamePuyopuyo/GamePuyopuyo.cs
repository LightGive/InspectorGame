using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GamePuyopuyo : MonoBehaviour{}

#if UNITY_EDITOR
[CustomEditor(typeof(GamePuyopuyo))]
public class GamePuyopuyoEditor: Editor
{
	private const int WID_NUM = 8;
	private const int HEI_NUM = 13;

	private bool isStart = false;
	private bool isGameOver = false;
	private float deltaTime = 0.0f;
	private float prevTime = 0.0f;
	private int[,] mass = new int[HEI_NUM, WID_NUM];
	
	public override void OnInspectorGUI()
	{
		if (!isStart)
		{
			DrawStart();
			return;
		}

		if (isGameOver)
		{
			//DrawGameover();
			return;
		}

		CalcDeltaTime();
		DisplayMass();
		Repaint();
	}

	void DisplayMass()
	{
		var windowWidth = EditorGUIUtility.currentViewWidth;
		var w = (windowWidth / WID_NUM) - 6;
		for (int i = 0; i < HEI_NUM; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < WID_NUM; j++)
			{

			}
			EditorGUILayout.EndHorizontal();
		}
	}

	void Initialize()
	{
		//mass = new int[,]
		//{
		//	{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
		//	{-1,-1,-1, 0, 0, 0, 0, 0, 0,-1,-1,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
		//	{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },
		//};
	}

	void DrawStart()
	{
		if (GUILayout.Button("Start"))
			Initialize();
		EditorGUILayout.Space();
		//EditorGUILayout.LabelField("HightScore", EditorPrefs.GetInt(SAVE_KEY, 0).ToString("000000"));
	}

	void CalcDeltaTime()
	{
		float now = (float)EditorApplication.timeSinceStartup;
		deltaTime = (now - prevTime) * 800;
		prevTime = now;
	}
}
#endif