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
	private const int WID_NUM = 12;
	private const int HEI_NUM = 23;
	private const int BLOCK_TYPE = 2;
	private const int BLOCK_SPACE = 4;
	private const int BLOCK_INIT_POS_X = 4;
	private const int BLOCK_INIT_POS_Y = 0;
	private const float INTERVAL = 100.5f;

	private int[,] mass = new int[HEI_NUM, WID_NUM];
	private int[,] myBlock = new int[BLOCK_SPACE, BLOCK_SPACE];
	private int myblockPosX = 0;
	private int myblockPosY = 0;
	private bool isStart = false;
	private bool isGameOver = false;
	private float deltaTime;
	private float prevTime;
	private float pos;
	private float timeCnt;

	private int[,,] massList = new int[BLOCK_TYPE, BLOCK_SPACE, BLOCK_SPACE]
	{
		{
			{0,5,0,0 },
			{0,5,0,0 },
			{0,5,5,0 },
			{0,5,5,0 }
		},
		{
			{0,0,0,4 },
			{0,0,0,4 },
			{0,0,0,4 },
			{4,4,4,4 }
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
		DrawButton();
		DisplayMass();
		Repaint();
	}
	
	void Init()
	{
		isStart = true;

		mass = new int[,]
		{
			{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 },
			{-1,-1,-1, 0, 0, 0, 0, 0, 0,-1,-1,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0,-1 },
			{-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1,-1 },														
		};
		myblockPosX = 4;
		myblockPosY = 0;
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
				var block = 0;
				if (myblockPosX <= j && (myblockPosX + 3) >= j && myblockPosY <= i && (myblockPosY + 3) >= i)
				{
					var x = j - myblockPosX;
					var y = i - myblockPosY;
					block = myBlock[y, x];
				}
				GUI.color = (block != (int)BlockInfo.None) ? GetColor(block) : GetColor(mass[i, j]);
				GUILayout.Box("", GUILayout.Width(w), GUILayout.Height(w));
			}
			EditorGUILayout.EndHorizontal();
		}
	}



	void InstantiateBlock()
	{
		myblockPosX = 3;
		myblockPosY = 0;

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

	void MoveCheck(int _dirX = 0, int _dirY = 0)
	{
		if (!Judge(_dirX, _dirY))
			return;

		Move(_dirX, _dirY);
	}
	
	void Rotate()
	{

	}

	bool Judge(int _dirX = 0, int _dirY = 0)
	{
		var isCanMove = true;
		for (int i = 0; i < BLOCK_SPACE; i++)
		{
			for (int j = 0; j < BLOCK_SPACE; j++)
			{
				if (!isCanMove)
					continue;
				if (myBlock[j, i] == (int)BlockInfo.None)
					continue;

				var nextPosX = myblockPosX + i + _dirX;
				var nextPosY = myblockPosY + j + _dirY;

				if (nextPosX >= WID_NUM || nextPosX < 0 || nextPosY >= HEI_NUM || nextPosY < 0)
				{
					isCanMove = false;
					continue;
				}

				if (mass[nextPosY, nextPosX] == (int)BlockInfo.Wall||
					mass[nextPosY, nextPosX] == (int)BlockInfo.Square)
					isCanMove = false;
			}
		}

		return isCanMove;
	}

	void Fall()
	{
		if (!Judge(0, 1))
			Fixation();
		else
			Move(0, 1);
	}

	void Move(int _dirX = 0, int _dirY = 0)
	{
		myblockPosX += _dirX;
		myblockPosY += _dirY;
	}

	void Fixation()
	{
		for (int i = 0; i < BLOCK_SPACE; i++)
		{
			for (int j = 0; j < BLOCK_SPACE; j++)
			{
				if (myBlock[j, i] != (int)BlockInfo.None)
					mass[myblockPosY + j, myblockPosX + i] = (int)BlockInfo.Square;
			}
		}

		InstantiateBlock();
	}

	void DrawButton()
	{
		if (GUILayout.Button(""))
			InstantiateBlock();

		if (GUILayout.Button("Rotate"))
			Rotate();

		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Left"))
			MoveCheck(-1, 0);
		if (GUILayout.Button("Right"))
			MoveCheck(1, 0);
		EditorGUILayout.EndHorizontal();
		if (GUILayout.Button("Down"))
			MoveCheck(0, 1);
	}

	Color GetColor(int _massNum)
	{
		switch (_massNum)
		{
			case -1: return Color.gray;
			case 0:  return Color.white;
			case 1:  return Color.gray;
			case 2:  return Color.red;
			case 3:  return Color.green;
			case 4:  return Color.yellow;
			case 5:  return Color.blue;
		}
		return Color.white;
	}

	float GetInterval()
	{
		return INTERVAL;
	}
}
#endif

