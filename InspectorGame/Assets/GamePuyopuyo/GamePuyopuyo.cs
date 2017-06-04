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
	private const int INIT_X = 3;
	private const int INIT_Y = 0;

	private bool isStart = false;
	private bool isGameOver = false;
	private float deltaTime = 0.0f;
	private float prevTime = 0.0f;
	private int[,] cell = new int[WID_NUM, HEI_NUM];
	private int x = 0;
	private int y = 0;
	private float interval;
	private float timeCnt;
	private PuyoDir subPuyo;
	private Puyo mainPuyo;

	enum Puyo
	{
		Wall = -1,
		None = 0,
		Red, 
		Blue,
		Green,
		Yellow,
		Max,
	}

	enum PuyoDir
	{
		Right = 0,
		Bottom = 1,
		Left = 2,
		Top = 3,
		Max = 4,
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
		for (int i = 0; i < HEI_NUM; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < WID_NUM; j++)
			{
				GUI.color = GetCellColor(cell[j, i]);
				EditorGUILayout.LabelField(GetCellText(cell[j, i]), GUILayout.Width(15));
			}
			EditorGUILayout.EndHorizontal();
		}
	}

	void Initialize()
	{
		for (int i = 0; i < HEI_NUM; i++)
		{
			for (int j = 0; j < WID_NUM; j++)
			{
				if (j == 0 || j == WID_NUM - 1 || i == HEI_NUM-1)
					cell[j, i] = -1;
			}
		}

		Create();
		isStart = true;
	}

	void Create()
	{
		x = INIT_X;
		y = INIT_Y;
		cell[x, y] = (int)GetRandomPuyo();
		cell[x + 1, y] = (int)GetRandomPuyo();
	}

	void TimeCount()
	{
		timeCnt += deltaTime;
		if (timeCnt > interval)
		{
			timeCnt = 0.0f;
			Fall();
		}
	}

	void Fall()
	{
		var tmp = cell[x, y];
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

	string GetCellText(int _puyo)
	{
		switch ((Puyo)_puyo)
		{
			case Puyo.None: return "";
			case Puyo.Wall: return "◆";
			case Puyo.Red: 
			case Puyo.Blue:
			case Puyo.Green:
            case Puyo.Yellow: return "〇";
		}

		return "";
	}

	void Rotate()
	{
		subPuyo = (PuyoDir)(((int)subPuyo + 1) % (int)PuyoDir.Max);
	}

	Color GetCellColor(int _puyo)
	{
		switch ((Puyo)_puyo)
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

	Puyo GetSubPuyo()
	{
		switch (subPuyo)
		{
			case PuyoDir.Right:return (Puyo)cell[x + 1, y];
			case PuyoDir.Bottom: return (Puyo)cell[x, y + 1];
			case PuyoDir.Left: return (Puyo)cell[x-1, y];
			case PuyoDir.Top: return (Puyo)cell[x, y - 1];
		}
		return (Puyo)cell[x, y];
	}

	Puyo GetRandomPuyo()
	{
		return (Puyo)Random.Range(1, (int)Puyo.Max);
	}
}
#endif