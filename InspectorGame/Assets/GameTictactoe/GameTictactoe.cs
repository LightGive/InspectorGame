using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
public class GameTictactoe : MonoBehaviour {}
#if UNITY_EDITOR
[CustomEditor(typeof(GameTictactoe))]
public class GameTictactoeEditor: Editor
{
	private const int CELL_COUNT = 3;
	private string[] markStr = new string[3];
	private int[] mass = new int[CELL_COUNT * CELL_COUNT];
	private int nowTurn = 1;
	private bool isInit = false;
	private bool isGameOver = false;

	public override void OnInspectorGUI()
	{
		if (!isInit)
			Init();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("MarubatuEditor");
		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical();
		for (int i = 0; i < CELL_COUNT; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < CELL_COUNT; j++)
			{
				var no = i * CELL_COUNT + j;
				if (GUILayout.Button(markStr[mass[no]]))
				{
					//もうマスに入っているときは入れれない
					if (mass[no] != 0)
						return;

					mass[no] = nowTurn;
					CheckClear(no);
					ChangeTurn();
					Repaint();
				}
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();
	}

	void DrawInitButton()
	{
		if (GUILayout.Button("Init"))
			Init();
	}

	/// <summary>
	/// クリア判定
	/// </summary>
	void CheckClear(int x)
	{
		//縦横斜めを調べる
		if ((((mass[0] == mass[1] && mass[1] == mass[2]) && mass[0] != 0) ||
			 ((mass[3] == mass[4] && mass[4] == mass[5]) && mass[3] != 0) ||
			 ((mass[6] == mass[7] && mass[7] == mass[8]) && mass[6] != 0) ||
			 ((mass[0] == mass[3] && mass[3] == mass[6]) && mass[0] != 0) ||
			 ((mass[1] == mass[4] && mass[4] == mass[7]) && mass[1] != 0) ||
			 ((mass[2] == mass[5] && mass[5] == mass[8]) && mass[2] != 0) ||
			 ((mass[0] == mass[4] && mass[4] == mass[8]) && mass[0] != 0) ||
			 ((mass[2] == mass[4] && mass[4] == mass[6]) && mass[2] != 0)) &&
			   mass[x] != 0)
		{
			isGameOver = true;
			if (nowTurn == 1)
				EditorUtility.DisplayDialog("Result", "○ Win!!", "OK");
			else
				EditorUtility.DisplayDialog("Result", "× Win!!", "OK");
		}

		//ゲームオーバーじゃないとき
		if (!isGameOver)
		{
			var cnt = 0;
			for (int i = 0; i < mass.Length; i++)
			{
				if (mass[i] != 0)
					cnt++;
			}

			//マスが全部埋まってるとき
			if (cnt == mass.Length)
			{
				EditorUtility.DisplayDialog("title", "GameEnd", "Retry");
				Init();
			}
		}
	}

	/// <summary>
	/// ターンを変える
	/// </summary>
	void ChangeTurn()
	{
		nowTurn = (nowTurn == 2) ? 1 : 2;
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void Init()
	{
		for (int i = 0; i < mass.Length; i++)
			mass[i] = 0;

		isInit = true;
		markStr[0] = "";
		markStr[1] = "○";
		markStr[2] = "×";
	}
}
#endif