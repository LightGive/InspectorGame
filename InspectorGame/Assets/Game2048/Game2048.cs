using UnityEngine;
using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Game2048 : MonoBehaviour
{
	[ContextMenu("Delete SaveData")]
	void DeleteSaveData()
	{
		for (int i = Game1024Editor.MIN_MASS; i < Game1024Editor.MAX_MASS; i++)
		{
			for (int j = Game1024Editor.MIN_MASS; j < Game1024Editor.MAX_MASS; j++)
			{
				EditorPrefs.DeleteKey(Game1024Editor.SAVE_SCORE_KEY + i.ToString("00") + j.ToString("00"));
			}
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Game2048))]
public class Game1024Editor : Editor
{
	public const string SAVE_SCORE_KEY = "SAVE_SCORE_KEY";
	public const string SAVE_MASS_KEY = "SAVE_MASS_KEY";
    public const int MAX_MASS = 10;
	public const int MIN_MASS = 2;
	private const int MAX_NUM = 524288;
	private const int DIGIT_NUM = 6;
    private const int DEFAULT_WIDTH_NUM = 4;
	private const int BUTTON_HEIGHT = 20;
	private const int MASS_HEIGHT = 20;
	private const float TOW_PERCENET = 0.75f;
	private int[,] mass;

	public int Score
	{
		get { return score; }
		set
		{
			if (value > highScore)
			{
				highScore = value;
				EditorPrefs.SetInt(SAVE_SCORE_KEY + row.ToString("00") + colum.ToString("00"), highScore);
			}
			score = value;
		}
	}

	private int row = DEFAULT_WIDTH_NUM;
	private int colum = DEFAULT_WIDTH_NUM;
	private int score;
	private int highScore;
	private bool isStart;

	public class Mass
	{
		public int x;
		public int y;
		public Mass(int _x,int _y)
		{
			x = _x;
			y = _y;
		}
	}

	public override void OnInspectorGUI()
	{
		if (!isStart)
		{
			row = (int)EditorGUILayout.IntSlider("Row", row, MIN_MASS, MAX_MASS);
			colum = (int)EditorGUILayout.IntSlider("Colum", colum, MIN_MASS, MAX_MASS);
			if (GUILayout.Button("Start"))
			{
				isStart = true;
				Reset(row, colum);
			}
			return;
		}
		
		var windowWidth = EditorGUIUtility.currentViewWidth;


		EditorGUILayout.Space();
		var labelStyle = GUI.skin.GetStyle("Label");
		labelStyle.alignment = TextAnchor.MiddleLeft;
		labelStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Score" + "\n" + Score.ToString("000000"), labelStyle, GUILayout.Width(windowWidth / 2), GUILayout.Height(40));
		EditorGUILayout.LabelField("HighScore" + "\n" + highScore.ToString("000000"), labelStyle, GUILayout.Width(windowWidth/2), GUILayout.Height(40));
		EditorGUILayout.EndHorizontal();
		for (int i = 0; i < row; i++)
		{
			var edgeWidth = 20;
			var w = 4 * (colum - 1);
			var width = (windowWidth - edgeWidth - w) / colum;

			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < colum; j++)
			{
				var num = mass[i, j];
				var str = num == 0 ? "" : mass[i, j].ToString("0");
				var fontStyle = new GUIStyle(GUI.skin.box);
				fontStyle.fontStyle = (num == 0) ? FontStyle.Normal : FontStyle.Bold;
				GUI.color = GetNumColor(num);
				GUILayout.Box(str, fontStyle, GUILayout.Width(width), GUILayout.Height(MASS_HEIGHT));
			}
			EditorGUILayout.EndHorizontal();
		}
		GUI.color = Color.white;
		EditorGUILayout.Space();
		EditorGUI.BeginDisabledGroup(IsCanNotMove(1, 0));
		if (GUILayout.Button("Up", GUILayout.Height(BUTTON_HEIGHT)))
		{
			MoveUnit(1, 0);
		}
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.BeginHorizontal();
		EditorGUI.BeginDisabledGroup(IsCanNotMove(0, 1));
		if (GUILayout.Button("Left", GUILayout.Height(BUTTON_HEIGHT)))
		{
			MoveUnit(0, 1);
		}
		EditorGUI.EndDisabledGroup();
		EditorGUI.BeginDisabledGroup(IsCanNotMove(0, -1));
		if (GUILayout.Button("Right", GUILayout.Height(BUTTON_HEIGHT)))
		{
			MoveUnit(0, -1);
		}
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.EndHorizontal();
		EditorGUI.BeginDisabledGroup(IsCanNotMove(-1, 0));
		if (GUILayout.Button("Down", GUILayout.Height(BUTTON_HEIGHT)))
		{
			MoveUnit(-1, 0);
		}
		EditorGUI.EndDisabledGroup();
		EditorGUILayout.Space();
		
		EditorGUILayout.BeginHorizontal();
		if (GUILayout.Button("Quit", GUILayout.Height(BUTTON_HEIGHT)))
		{
			DestroyImmediate(target);
		}
		if (GUILayout.Button("Save", GUILayout.Height(BUTTON_HEIGHT)))
		{
			SaveMass();
		}
		if (GUILayout.Button("Load", GUILayout.Height(BUTTON_HEIGHT)))
		{
		}
		if (GUILayout.Button("Reset", GUILayout.Height(BUTTON_HEIGHT)))
		{
			Reset(row, colum);
		}
		EditorGUILayout.EndHorizontal();

	}

	void Reset(int _row, int _colum)
	{
		var str = "あいうえお";
		Debug.Log(str.Substring(0,1));

		Score = 0;
		highScore = EditorPrefs.GetInt(SAVE_SCORE_KEY + _row.ToString("00") + _colum.ToString("00"), 0);
		mass = new int[_row, _colum];
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < colum; j++)
			{
				mass[i, j] = 0;
			}
		}
		Pop();
	}

	void SaveMass()
	{
		var saveStr = "";

		saveStr += row.ToString("000000");
		saveStr += colum.ToString("000000");
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < colum; j++)
			{
				saveStr += mass[i, j].ToString("000000");
				mass[i, j] = 0;
			}
		}
		EditorPrefs.SetString(SAVE_MASS_KEY, saveStr);
	}

	int[,] LoadMass()
	{
		var loadStr = EditorPrefs.GetString(SAVE_MASS_KEY, "");
		int row = int.Parse(loadStr.Substring(0, DIGIT_NUM));
		int colum = int.Parse(loadStr.Substring(DIGIT_NUM, DIGIT_NUM));

		int[,] loadMass = new int[row, colum];
		for(int i = DIGIT_NUM * 2; i < (DIGIT_NUM * 2) + row; i += DIGIT_NUM)
		{
			for (int j = DIGIT_NUM * 2; j < (DIGIT_NUM * 2) + colum; j  += DIGIT_NUM)
			{

			}
		}
		return loadMass;
	}

	Color GetNumColor(int _num)
	{
		switch(_num)
		{
			case 2: return new Color32(255, 215, 0 ,255);
			case 4: return new Color32(255, 165, 0, 255);
			case 8: return new Color32(244, 164, 96, 255);
			case 16: return new Color32(255, 140, 0, 255);
			case 32: return new Color32(255, 127, 80, 255);
			case 64: return new Color32(255, 99, 71, 255);
			case 128: return new Color32(255, 69, 0, 255);
			case 256: return new Color32(255, 0, 0, 255);
			case 512: return new Color32(255, 20, 147, 255);
		}

		return Color.white;
	}

	void Pop()
	{
		List<Mass> zeroIdxList = new List<Mass>();

		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < colum; j++)
			{
				if (mass[i, j] == 0)
					zeroIdxList.Add(new Mass(i, j));
			}
		}

		if (zeroIdxList.Count == 0)
		{
			return;
		}

		var ran = Random.Range(0, zeroIdxList.Count);
		var num = Random.value > TOW_PERCENET ? 4 : 2;
		mass[zeroIdxList[ran].x, zeroIdxList[ran].y] = num;


		if (IsCanNotMove(1, 0) && IsCanNotMove(0, 1) && IsCanNotMove(-1, 0) && IsCanNotMove(0, -1))
		{
			OnInspectorGUI();
			GameOver();
		}
	}

	void GameOver()
	{
		var congraturation = (highScore == Score) ? "Congraturation!!" : "";
		var isContinue = EditorUtility.DisplayDialog("Game Over",
			"\n" +
			congraturation+
			"\n" +
			"HighScore : " + highScore.ToString("000000") + "\n" +
			"Score　　　: " + Score.ToString("000000") + "\n" + "\n" +
			"Continue?", "Yes", "No");
		if (isContinue)
		{
			Reset(row, colum);
		}
		else
		{
			DestroyImmediate(target);
		}
	}

	bool IsCanNotMove(int _iDir = 0, int _jDir = 0)
	{
		var iFirst = (_iDir >= 0) ? 0 : row - 1;
		var jFirst = (_jDir >= 0) ? 0 : colum - 1;
		var iAdd = (_iDir == 0) ? 1 : _iDir;
		var jAdd = (_jDir == 0) ? 1 : _jDir;

		for (int i = iFirst; (iAdd > 0) ? (i < row) : (i >= 0); i += iAdd)
		{
			for (int j = jFirst; (jAdd > 0) ? (j < colum) : (j >= 0); j += jAdd)
			{
				if ((j - _jDir) < 0 || (j - _jDir) >= colum || (i - _iDir) < 0 || (i - _iDir) >= row)
					continue;
				if ((mass[i - _iDir, j - _jDir] == 0 && mass[i, j] != 0) || (mass[i - _iDir, j - _jDir] == mass[i, j] && mass[i, j] != 0))
				{
					return false;
				}
			}
		}

		return true;
	}

	void MoveUnit(int _iDir = 0, int _jDir = 0)
	{
		if (_iDir == 0 && _jDir == 0)
			return;

		var iFirst = (_iDir >= 0) ? 0 : row - 1;
		var jFirst = (_jDir >= 0) ? 0 : colum - 1;
		var iAdd = (_iDir == 0) ? 1 : _iDir;
		var jAdd = (_jDir == 0) ? 1 : _jDir;

		for (int k = 0; k < 2; k++)
		{
			for (int i = iFirst; (iAdd > 0) ? (i < row) : (i >= 0); i += iAdd)
			{
				for (int j = jFirst; (jAdd > 0) ? (j < colum) : (j >= 0); j += jAdd)
				{
					if ((j - _jDir) < 0 || (j - _jDir) >= colum || (i - _iDir) < 0 || (i - _iDir) >= row)
						continue;
					if ((mass[i - _iDir, j - _jDir] == 0 && mass[i, j] != 0 && k == 0) ||
						(mass[i - _iDir, j - _jDir] == mass[i, j] && mass[i, j] != 0 && k == 1))
					{
						if (k == 1)
						{
							var addScore = (mass[i, j] == MAX_NUM) ? mass[i, j] : mass[i, j] * 2;
							mass[i, j] = addScore;
							Score += addScore;
						}

						var iCnt = (_iDir == 0) ? 0 : i;
						var jCnt = (_jDir == 0) ? 0 : j;
						while ((_jDir > 0 && jCnt < colum) || (_jDir < 0 && jCnt >= 0) || (_iDir > 0 && iCnt < row) || (_iDir < 0 && iCnt >= 0))
						{
							if (_jDir != 0)
								mass[i, jCnt - _jDir] = mass[i, jCnt];
							else
								mass[iCnt - _iDir, j] = mass[iCnt, j];
							jCnt += _jDir;
							iCnt += _iDir;
						}

						var ii = i;
						var jj = j;
						if (_iDir != 0)
							ii = (_iDir > 0) ? row - 1 : 0;
						if (_jDir != 0)
							jj = (_jDir > 0) ? colum - 1 : 0;

						mass[ii, jj] = 0;
						j -= k == 1 ? _jDir : _jDir * 2;
						i -= k == 1 ? _iDir : _iDir * 2;
					}
				}
			}
		}
		Pop();
	}
}
#endif