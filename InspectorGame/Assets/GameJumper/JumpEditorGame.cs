using UnityEngine;
using UnityEditor;
using System.Text;
using System.Collections;

public class JumpEditorGame : MonoBehaviour {


}


#if UNITY_EDITOR

[CustomEditor(typeof(JumpEditorGame))]
public class JumpEditorGameEditor : Editor
{
	private const float FPS = 60;
	private const float FSE_TIME = 1.0f / FPS;
	private const float CURRENT_WIDTH = 5;
	private const float OFFSET_Y = 10;
	private const float GRAVITY_SCALE = -40.98f;
	private const float JUMP_SCALE = 70.0f;

	private bool isInit     = false;
	private bool isStart    = false;
	private bool isJump     = false;
	private bool isGameOver = false;
	private bool isFirstSpace = false;

	private int score =0;

	private float jumpPos = 0.0f;
	private float gravity = 0.0f;
	private float velocityY = 0.0f;
	private float deltaTime = 0.0f;
	private float fireInterval = 0.0f;
	private float before = 0.0f;

    private JumpEditorGame editorGame;

	private Texture2D[] slimeTex = new Texture2D[3];
	private Texture2D groundTex;
	private Texture2D fireTex;

	private Rect area;
	private Rect charaRect;

	//アニメーション関係
	private int animIndex = 0;
	private int animDir = 1;
	private float animTimeCnt = 0.0f;

	//炎関係
	private Fire[] fire = new Fire[3];

	private class Fire
	{
		public bool isEnable = false;
		public float pos = 0.0f;
		public Rect rect;

		public Fire()
		{
			rect= new Rect(0,0,0,0);
			isEnable=false;
			pos=0.0f;
		}
	}

	public override void OnInspectorGUI()
	{

		if (!isInit)
		{
			Init();
			return;
		}

		editorGame = target as JumpEditorGame;
		SetDeltaTime();
		SlimeAnimation();


		var width = EditorGUIUtility.currentViewWidth;
		area = GUILayoutUtility.GetRect(width, width / 2.0f);

		//枠の描画
		//GUI.color = Color.black;
		//GUI.DrawTexture(new Rect(0, area.y + OFFSET_Y, width, width), Texture2D.whiteTexture);

		//GUI.color = Color.white;
		//GUI.DrawTexture(new Rect(CURRENT_WIDTH, area.y + CURRENT_WIDTH + OFFSET_Y, width - (CURRENT_WIDTH * 2), width - (CURRENT_WIDTH * 2)), Texture2D.whiteTexture);


		if (!isStart)
		{

			var titleSize = new Vector2(110, 20);
			var titlePos = new Vector2((area.width - titleSize.x) / 2, area.y + (area.width / 10));
			var titleRect = new Rect(titlePos.x, titlePos.y, titleSize.x, titleSize.y);
			GUI.Label(titleRect, "スライムの大冒険");

			var btnSize = new Vector2((area.width / 2.8f), (area.width / 10));
			var btnPos = new Vector2((area.width - btnSize.x) / 2, titlePos.y + (area.width / 9));
			var btnRect = new Rect(btnPos.x, btnPos.y, btnSize.x, btnSize.y);

			if (GUI.Button(btnRect, "はじめる"))
			{
				isStart = true;
			}

			btnPos.y += (area.width / 9);
			btnRect = new Rect(btnPos.x, btnPos.y, btnSize.x, btnSize.y);

			if (GUI.Button(btnRect, "おわる"))
			{
				DestroyImmediate(editorGame);
			}
		}
		else if (isGameOver)
		{ 
			var titleSize = new Vector2(110, 20);
			var titlePos = new Vector2((area.width - titleSize.x) / 2, area.y + (area.width / 10));
			var titleRect = new Rect(titlePos.x, titlePos.y, titleSize.x, titleSize.y);
			GUI.Label(titleRect, "GameOver");


			var btnSize = new Vector2((area.width / 2.8f), (area.width / 10));
			var btnPos = new Vector2((area.width - btnSize.x) / 2, titlePos.y + (area.width / 9));
			var btnRect = new Rect(btnPos.x, btnPos.y, btnSize.x, btnSize.y);

			if (GUI.Button(btnRect, "リトライ"))
			{
				isStart = true;
				isFirstSpace = false;
			}


			btnPos.y += (area.width / 9);
			btnRect = new Rect(btnPos.x, btnPos.y, btnSize.x, btnSize.y);

			if (GUI.Button(btnRect, "おわる"))
			{
				DestroyImmediate(editorGame);
			}
		}
		else
		{
			DrawSlime();
			InputKey();
			Jump();
			ShotFire();
			CheckCollision();

			//var groundSize = area.width;
			//var groundPos =  new Vector2(groundSize) 
			//GUI.DrawTexture(new Rect(area.x, area.y+(area.width / 2.0f)+charaSize, area.width, area.width - (area.width / 2.0f)), groundTex);
		}

		Repaint();

	}


	/// <summary>
	/// キー入力
	/// </summary>
	void InputKey()
	{
		Event e = Event.current;
		if (e.type == EventType.KeyDown)
		{
			//ショットキーフラグ
			if (e.keyCode == KeyCode.Space && !isJump)
			{

				if (!isFirstSpace)
				{
					isFirstSpace = true;
				} 

				isJump = true;
				gravity = JUMP_SCALE;
				animIndex = 2;
			}

			//ショットキーフラグ
			if (e.keyCode == KeyCode.A)
			{
				Debug.Log("キャラ" + charaRect);
				Debug.Log("火" + fire[0].rect);
			}
		}
	}


	void Jump()
	{
		if (!isJump)
			return;

		gravity += GRAVITY_SCALE * deltaTime;
		jumpPos = Mathf.Clamp(jumpPos + (gravity * deltaTime), 0, Mathf.Infinity);


		if (jumpPos == 0)
		{
			isJump = false;
		}
	}

	void DrawSlime()
	{
		var charaSize = area.width / 9.0f;
		var charaCenter = (area.width - charaSize) / 2.0f;
		charaRect = new Rect(charaCenter, area.y + area.height - charaSize - jumpPos, charaSize, charaSize);
		GUI.DrawTexture(charaRect, slimeTex[animIndex]);
	} 

	void CheckCollision(){

		if (!isFirstSpace)
			return;

		for (int i = 0; i < fire.Length; i++)
		{
			if (!fire[i].isEnable)
				return;

			if (charaRect.xMax > fire[i].rect.x &&
				charaRect.x < fire[i].rect.xMax &&
				charaRect.yMax > fire[i].rect.y)
			{
                DestroyImmediate(editorGame);
			}
		}
	}

	void ShotFire()
	{
		float INTERVAL = 5.0f;
		float SPEED    = 0.1f;
		float SIZE     = (area.width / 9.0f) / 2.0f;

		if (!isFirstSpace)
			return;

		fireInterval += deltaTime;

		for (int i = 0; i < fire.Length; i++)
		{
			if (fire[i].isEnable)
			{
				fire[i].pos += SPEED * deltaTime;
				fire[i].rect = new Rect(area.width - (fire[i].pos * area.width), area.y + area.height - SIZE, SIZE, SIZE);
				GUI.DrawTexture(fire[i].rect, fireTex);

				if (fire[i].pos > 2)
				{
					fire[i].isEnable = false;

				}
			}
		}

		if (fireInterval < INTERVAL)
			return;

		fireInterval = 0.0f;
		for (int i = 0; i < fire.Length; i++)
		{
			if (!fire[i].isEnable)
			{
				fire[i].isEnable = true;
				fire[i].pos = 0.0f;
				break;
			}
		}
		
	}


	/// <summary>
	/// DeltaTimeを設定する
	/// </summary>
	void SetDeltaTime()
	{
		float now = (float)EditorApplication.timeSinceStartup;
		if ((now - before) > FSE_TIME)
		{
			deltaTime = now - before;
			before = now;

		}
	}

	void SlimeAnimation()
	{
		float ANIM_TIME = 0.7f;

		if (isJump)
			return;

		animTimeCnt += deltaTime;
		if (animTimeCnt > ANIM_TIME)
		{
			if ((animIndex + animDir) >= 3 || (animIndex + animDir) < 0)
				animDir = -animDir;
			animIndex += animDir;
			animTimeCnt = 0.0f;
		}
	}

	void Init()
	{
		fireTex = Resources.Load<Texture2D>("fire");
		groundTex = Resources.Load<Texture2D>("ground");
		
		//スライムのテクスチャを読み込む
		for (int i = 0; i < 3; i++)
		{
			slimeTex[i] = Resources.Load<Texture2D>("slime" + i.ToString("00"));
		}

		for (int i = 0; i < 3; i++)
		{
			fire[i] = new Fire();
		}

		isInit = true;
	}
}

#endif