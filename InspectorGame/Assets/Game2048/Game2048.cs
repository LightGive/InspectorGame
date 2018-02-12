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
        EditorPrefs.DeleteKey(Game2048Editor.SAVE_ISSTART_KEY);
        EditorPrefs.DeleteKey(Game2048Editor.SAVE_CELL_KEY);
        for (int i = Game2048Editor.MIN_CELL; i < Game2048Editor.MAX_CELL; i++)
		{
			for (int j = Game2048Editor.MIN_CELL; j < Game2048Editor.MAX_CELL; j++)
			{
				EditorPrefs.DeleteKey(Game2048Editor.SAVE_SCORE_KEY + i.ToString("00") + j.ToString("00"));
			}
		}
	}
}

#if UNITY_EDITOR
[CustomEditor(typeof(Game2048))]
public class Game2048Editor : Editor
{
    public const string SAVE_SCORE_KEY = "SAVE_SCORE_KEY";
    public const string SAVE_CELL_KEY = "SAVE_CELL_KEY";
    public const string SAVE_ISSTART_KEY = "SAVE_ISSTART_KEY";
    private const string SAVE_FORMAT = "000000";
    private const char SPLIT_CHAR = ',';
    public const int MAX_CELL = 8;
    public const int MIN_CELL = 2;

	private const int MAX_NUM = 524288;
	private const int DIGIT_NUM = 6;
    private const int DEFAULT_WIDTH_NUM = 4;
	private const int BUTTON_HEIGHT = 20;
	private const int MASS_HEIGHT = 20;
	private const float TOW_PERCENET = 0.75f;
	private int[,] cell;

    public int HighScore
    {
        get 
        {
            return EditorPrefs.GetInt(SAVE_SCORE_KEY + row.ToString("00") + colum.ToString("00"), 0);
        }
        set
        {
            if (value > EditorPrefs.GetInt(SAVE_SCORE_KEY + row.ToString("00") + colum.ToString("00"), 0))
                EditorPrefs.SetInt(SAVE_SCORE_KEY + row.ToString("00") + colum.ToString("00"), value);
        }

    }

	public int Score
	{
		get { return score; }
		set
		{
			score = value;
            HighScore = score;
		}
	}

    private bool IsStart
    {
        get
        {
            var str = EditorPrefs.GetString(SAVE_ISSTART_KEY);
            return (str == "True");
        }
        set
        {
            EditorPrefs.SetString(SAVE_ISSTART_KEY, value.ToString());
        }
    }

	private int row = DEFAULT_WIDTH_NUM;
	private int colum = DEFAULT_WIDTH_NUM;
	private int score;
	private int highScore;
    private bool isGameStart;
    private bool isGameOver = false;

	public class Cell
	{
		public int x;
		public int y;
		public Cell(int _x,int _y)
		{
			x = _x;
			y = _y;
		}
	}

    void OnEnable()
    {
        isGameStart = IsStart;
        if (isGameStart)
        {
            cell = LoadCell();
        }
    }

    public override void OnInspectorGUI()
	{
        if (!isGameStart)
        {
            GameStartDisplay();
            return;
        } 
        if(isGameOver)
        {
            GameOverDisplay();
            return;
        }

		var windowWidth = EditorGUIUtility.currentViewWidth;
		EditorGUILayout.Space();
		var labelStyle = GUI.skin.GetStyle("Label");
		labelStyle.alignment = TextAnchor.MiddleLeft;
		labelStyle.fontStyle = FontStyle.Bold;

		EditorGUILayout.BeginHorizontal();
        Score = CalcScore();
		EditorGUILayout.LabelField("Score" + "\n" + Score.ToString(SAVE_FORMAT), labelStyle, GUILayout.Width(windowWidth / 2), GUILayout.Height(40));
        EditorGUILayout.LabelField("HighScore" + "\n" + HighScore.ToString(SAVE_FORMAT), labelStyle, GUILayout.Width(windowWidth/2), GUILayout.Height(40));
		EditorGUILayout.EndHorizontal();
		for (int i = 0; i < row; i++)
		{
			var edgeWidth = 20;
			var w = 4 * (colum - 1);
			var width = (windowWidth - edgeWidth - w) / colum;

			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < colum; j++)
			{
				var num = cell[i, j];
				var str = num == 0 ? "" : cell[i, j].ToString("0");
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
		if (GUILayout.Button("Title", GUILayout.Height(BUTTON_HEIGHT)))
		{
            OnTitleButtonDown();
        }
		if (GUILayout.Button("Reset", GUILayout.Height(BUTTON_HEIGHT)))
		{
            Reset(row, colum);
		}
		EditorGUILayout.EndHorizontal();
	}

    void GameStartDisplay()
    {
        var labelStyle = GUI.skin.GetStyle("Label");
        labelStyle.alignment = TextAnchor.MiddleLeft;
        labelStyle.fontStyle = FontStyle.Bold;
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Game Setting", labelStyle);
        row = (int)EditorGUILayout.IntSlider("Row", row, MIN_CELL, MAX_CELL);
        colum = (int)EditorGUILayout.IntSlider("Colum", colum, MIN_CELL, MAX_CELL);
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("HighScore", labelStyle);
        EditorGUILayout.LabelField(HighScore.ToString(SAVE_FORMAT));
        EditorGUILayout.Space();
        if (GUILayout.Button("Start"))
        {
            OnStartButtonDown();
        }
    }

    void GameOverDisplay()
    {
        var labelStyle = GUI.skin.GetStyle("Label");
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontStyle = FontStyle.Bold;
        EditorGUILayout.LabelField(
            "Game Over" + "\n" + "\n" +
            "Youre Score : " + Score.ToString(SAVE_FORMAT) + "\n" +
            "High Score : " + HighScore.ToString(SAVE_FORMAT) + "\n",
            labelStyle, GUILayout.Height(80));

        EditorGUILayout.BeginHorizontal();
        if(GUILayout.Button("Retry"))
        {
            Reset(row, colum);
        }
        if(GUILayout.Button("Title"))
        {
            IsStart = false;
            isGameStart = false;
        }
        EditorGUILayout.EndHorizontal();
    }

    void OnStartButtonDown()
    {
        isGameStart = true;
        IsStart = true;
        Reset(row, colum);
    }

    void OnTitleButtonDown()
    {
        isGameStart = false;
        IsStart = false;
        OnInspectorGUI();
    }

	void Reset(int _row, int _colum)
	{
        isGameOver = false;
		Score = 0;
		cell = new int[_row, _colum];
		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < colum; j++)
			{
				cell[i, j] = 0;
			}
		}
		Pop();
	}

	void SaveCell()
	{
        string saveStr = row.ToString(SAVE_FORMAT) + SPLIT_CHAR + colum.ToString(SAVE_FORMAT) + SPLIT_CHAR;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < colum; j++)
            {
                saveStr += cell[i, j].ToString(SAVE_FORMAT);
                saveStr += SPLIT_CHAR;
            }
        }
        EditorPrefs.SetString(SAVE_CELL_KEY, saveStr);
	}

    int[,] LoadCell()
    {
        string loadStr = EditorPrefs.GetString(SAVE_CELL_KEY, "");
        if (string.IsNullOrEmpty(loadStr))
            return null;

        string[] loadSingleCell = loadStr.Split(SPLIT_CHAR);
        int r = int.Parse(loadSingleCell[0]);
        int c = int.Parse(loadSingleCell[1]);
        int[,] loadCell = new int[r, c];

        row = r;
        colum = c;

        for (int i = 0; i < r; i++)
        {
            for (int j = 0; j < c; j++)
            {
                var str = loadSingleCell[((i * c) + j) + 2];
                loadCell[i, j] = int.Parse(str);
            }
        }
        return loadCell;
    }


	Color GetNumColor(int _num)
	{
		switch(_num)
		{
            case 0: return new Color32(255, 255, 255, 255);
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
		List<Cell> zeroIdxList = new List<Cell>();

		for (int i = 0; i < row; i++)
		{
			for (int j = 0; j < colum; j++)
			{
				if (cell[i, j] == 0)
					zeroIdxList.Add(new Cell(i, j));
			}
		}

		if (zeroIdxList.Count == 0)
		{
			return;
		}

		var ran = Random.Range(0, zeroIdxList.Count);
		var num = Random.value > TOW_PERCENET ? 4 : 2;
        cell[zeroIdxList[ran].x, zeroIdxList[ran].y] = num;
        if (IsCanNotMove(1, 0) && IsCanNotMove(0, 1) && IsCanNotMove(-1, 0) && IsCanNotMove(0, -1))
        {
            isGameOver = true;
        }
        else
        {
            SaveCell();
        }
	}

    int CalcScore()
    {
        var sum = 0;
        for (int i = 0; i < row; i++)
        {
            for (int j = 0; j < colum; j++)
            {
                sum += cell[i, j];
            }
        }
        return sum;
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
					if ((cell[i - _iDir, j - _jDir] == 0 && cell[i, j] != 0 && k == 0) ||
						(cell[i - _iDir, j - _jDir] == cell[i, j] && cell[i, j] != 0 && k == 1))
					{
						if (k == 1)
						{
							var addScore = (cell[i, j] == MAX_NUM) ? cell[i, j] : cell[i, j] * 2;
							cell[i, j] = addScore;
						}

						var iCnt = (_iDir == 0) ? 0 : i;
						var jCnt = (_jDir == 0) ? 0 : j;
						while ((_jDir > 0 && jCnt < colum) || (_jDir < 0 && jCnt >= 0) || (_iDir > 0 && iCnt < row) || (_iDir < 0 && iCnt >= 0))
						{
							if (_jDir != 0)
								cell[i, jCnt - _jDir] = cell[i, jCnt];
							else
								cell[iCnt - _iDir, j] = cell[iCnt, j];
							jCnt += _jDir;
							iCnt += _iDir;
						}

						var ii = i;
						var jj = j;
						if (_iDir != 0)
							ii = (_iDir > 0) ? row - 1 : 0;
						if (_jDir != 0)
							jj = (_jDir > 0) ? colum - 1 : 0;

						cell[ii, jj] = 0;
						j -= k == 1 ? _jDir : _jDir * 2;
						i -= k == 1 ? _iDir : _iDir * 2;
					}
				}
			}
		}
		Pop();
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
                if ((cell[i - _iDir, j - _jDir] == 0 && cell[i, j] != 0) || (cell[i - _iDir, j - _jDir] == cell[i, j] && cell[i, j] != 0))
                {
                    return false;
                }
            }
        }
        return true;
    }
}
#endif