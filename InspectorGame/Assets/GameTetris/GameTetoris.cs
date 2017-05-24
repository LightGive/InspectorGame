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
	private const int BLOCK_TYPE = 1;
	private const int BLOCK_SPACE = 4;

	private const float INTERVAL = 1000.5f;

	private int[,] mass = new int[HEI_NUM, WID_NUM];

	private int[,] myblockPos = new int[3, 0];
	private int[,] myBlock = new int[BLOCK_SPACE, BLOCK_SPACE];
	private bool isStart = false;
	private float deltaTime;
	private float prevTime;
	private float pos;
	private float timeCnt;

	private int[,,] massList = new int[BLOCK_TYPE, BLOCK_SPACE, BLOCK_SPACE]
	{
		{
			{1,1,1,1 },
			{1,1,1,1 },
			{1,1,1,1 },
			{1,1,1,1 }
		},
	};

	public enum BlockInfo
	{
		Wall = -1,
		None = 0,
		Square = 1,
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
		TimeCount();
		DisplayMass();
		Repaint();
	}
	
	void Init()
	{
		isStart = true;

		for (int i = 0; i < HEI_NUM; i++)
		{
			for (int j = 0; j < WID_NUM; j++)
			{
				mass[i, j] = 0;
			}
		}

		mass[0, 5] = 1;

	}

	void DisplayMass()
	{
		var windowWidth = EditorGUIUtility.currentViewWidth;
		var w = (windowWidth / WID_NUM) - 8;

		for (int i = 0; i < HEI_NUM; i++)
		{
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < WID_NUM; j++)
			{
				GUI.color = GetColor(mass[i, j]);
				GUILayout.Box("", GUILayout.Width(w), GUILayout.Height(w));
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

	void InstantiateBlock()
	{
		int blockNo = Random.Range(0, BLOCK_TYPE);
		myBlock = new int[BLOCK_SPACE, BLOCK_SPACE];

		for(int i = 0; i < BLOCK_SPACE; i++)
		{
			for (int j = 0; j < BLOCK_SPACE; j++)
			{
				myBlock[i, j] = massList[blockNo, i, j];
			}
		}
	}

	void CalcDeltaTime()
	{
		float now = (float)EditorApplication.timeSinceStartup;
		deltaTime = (now - prevTime) * 800;
		prevTime = now;
	}

	void TimeCount()
	{
		timeCnt += deltaTime;
		if (timeCnt > GetInterval())
		{
			timeCnt = 0.0f;
			Fall();
		}
	}

	void Fall()
	{
		for (int i = HEI_NUM-1; i >=0; i--)
		{
			for (int j = 0; j < WID_NUM; j++)
			{
				if (mass[i, j] == 0)
					continue;

				if (i + 1 >= HEI_NUM)
					continue;

				var tmp = mass[i + 1, j];
				mass[i + 1, j] = mass[i, j];
				mass[i, j] = tmp;
			}
		}
	}

	float GetInterval()
	{
		return INTERVAL;
	}
}
#endif

