using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameTictactoe : MonoBehaviour {}
#if UNITY_EDITOR
[CustomEditor(typeof(GameTictactoe))]
public class GameTictactoeEditor : Editor
{
	private const int CELL_COUNT = 3;
	private int[] cell = new int[CELL_COUNT * CELL_COUNT];
	private int nowTurn = 1;
	private bool isInit = false;
	private bool isGameOver = false;

	private enum Mark
	{
		None = 0,
		Tic  = 1,
		Tac  = 2
	}

	public override void OnInspectorGUI()
	{
		if (!isInit)
			Init();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("GameTictactoe");
		EditorGUILayout.Space();

		EditorGUI.BeginDisabledGroup(isGameOver);
		EditorGUILayout.BeginVertical();
		for (int i = 0; i < CELL_COUNT; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < CELL_COUNT; j++)
			{
				var no = i * CELL_COUNT + j;

				EditorGUI.BeginDisabledGroup(cell[no] != (int)Mark.None);
				if (GUILayout.Button(GetMarkStrint(cell[no])))
				{
					if (cell[no] != 0)
						return;

					cell[no] = nowTurn;
					CheckClear(no);
					ChangeTurn();
					Repaint();
				}
				EditorGUI.EndDisabledGroup();
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
		EditorGUI.EndDisabledGroup();
	}

	void DrawInitButton()
	{
		if (GUILayout.Button("Init"))
			Init();
	}

	void CheckClear(int x)
	{
		if ((((cell[0] == cell[1] && cell[1] == cell[2]) && cell[0] != 0) ||
			 ((cell[3] == cell[4] && cell[4] == cell[5]) && cell[3] != 0) ||
			 ((cell[6] == cell[7] && cell[7] == cell[8]) && cell[6] != 0) ||
			 ((cell[0] == cell[3] && cell[3] == cell[6]) && cell[0] != 0) ||
			 ((cell[1] == cell[4] && cell[4] == cell[7]) && cell[1] != 0) ||
			 ((cell[2] == cell[5] && cell[5] == cell[8]) && cell[2] != 0) ||
			 ((cell[0] == cell[4] && cell[4] == cell[8]) && cell[0] != 0) ||
			 ((cell[2] == cell[4] && cell[4] == cell[6]) && cell[2] != 0)) &&
			   cell[x] != 0)
		{
			isGameOver = true;
			if (nowTurn == 1)
				EditorUtility.DisplayDialog("Result", "○ Win!!", "OK");
			else
				EditorUtility.DisplayDialog("Result", "× Win!!", "OK");
		}

		if (!isGameOver)
		{
			var cnt = 0;
			for (int i = 0; i < cell.Length; i++)
			{
				if (cell[i] != 0)
					cnt++;
			}

			if (cnt == cell.Length)
			{
				EditorUtility.DisplayDialog("title", "GameEnd", "Retry");
				Init();
			}
		}
	}

	void ChangeTurn()
	{
		nowTurn = (nowTurn == 2) ? 1 : 2;
	}

	void Init()
	{
		cell = new int[CELL_COUNT * CELL_COUNT];
		for (int i = 0; i < cell.Length; i++)
			cell[i] = 0;
		isInit = true;
	}

	private string GetMarkStrint(int _no)
	{
		switch (_no)
		{
			case 0: return "";
			case 1: return "●";
			case 2: return "×";
			default: return "";
		}
	}
}
#endif