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
	private Puyo[,] cell = new Puyo[HEI_NUM, WID_NUM];
	
	enum Puyo
	{
		Wall = -1,
		None = 0,
		Red, 
		Blue,
		Green,
		Yellow
	}

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
		for (int i = 0; i < WID_NUM; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < HEI_NUM; j++)
			{
				GUILayout.Label(GetCellText(cell[i, j]));
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

	string GetCellText(Puyo _puyo)
	{
		switch (_puyo)
		{
			case Puyo.None: return "";
			case Puyo.Wall: return "■";
			case Puyo.Red: 
			case Puyo.Blue:
			case Puyo.Green:
            case Puyo.Yellow: return "●";
		}

		return "";
	}

	Color GetCellColor(Puyo _puyo)
	{
		switch (_puyo)
		{
			case Puyo.Wall: return Color.white;
			case Puyo.None: return Color.white;
			case Puyo.Red:  return Color.red;
			case Puyo.Blue: return Color.blue;
			case Puyo.Green: return Color.green;
			case Puyo.Yellow: return Color.yellow;
		}

		return Color.white;
	}
}
#endif