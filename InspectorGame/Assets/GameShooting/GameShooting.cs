using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class GameShooting : MonoBehaviour {}

#if UNITY_EDITOR
[CustomEditor(typeof(GameShooting))]
public class ShootingEditor : Editor
{
	private const int ENEMY_MAX_NUM = 10;
	private const float TEXTURE_SIZE = 0.005f;
	private const float PLAYER_SPEED = 0.5f;
	private const float PLAYER_BULLET_SPEED = 2.0f;
	private const float PLAYER_BULLET_INTERVAL = 0.1f;

	private const string SAVE_KEY = "SAVE_SCORE_KEY";

	private bool isGameOver = false;
	private bool isStart = false;

	private int defeatCnt;
	private int score;
	private float now;
	private float deltaTime;
	private float prevTime;
	private float bulletTimeCnt = 0.0f;
	private float generateTimeCnt = 0.0f;

	private Rect areaRect;
	private Vector2 bulletDefPos = new Vector2(0.0f, 0.0f);
	private Vector2 playerPos;
	private Vector2 playerSize;
	private Vector2 playerVec;
	private Texture2D player;
	private Texture2D[] enemyTex = new Texture2D[2];
	private Texture2D[] bulletTex = new Texture2D[2];
	private List<Bullet> bulletList = new List<Bullet>();
	private List<Enemy> enemyList = new List<Enemy>();

	private KeyFlag keyFlag;

	/// <summary>
	/// キーのフラグ
	/// </summary>
	public enum KeyFlag
	{
		//キーフラグ
		Key_Right = 0x001,
		Key_Left = 0x002,
		Key_Up = 0x004,
		Key_Down = 0x008,
		Key_Space = 0x010,
	}

	/// <summary>
	/// 弾のクラス
	/// </summary>
	class Bullet
	{
		public Vector2 pos;
		public Vector2 vec;
		public bool isActive = false;
		public bool isPlayerBullet = false;
		public int bulletNo = 0;
		public float speed = 1.0f;

		public Bullet(Vector2 _pos, Vector2 _vec, bool _isActive, bool _isPlayerBullet, int _bulletNo, float _speed)
		{
			pos = _pos;
			vec = _vec;
			isActive = _isActive;
			isPlayerBullet = _isPlayerBullet;
			bulletNo = _bulletNo;
			speed = _speed;
		}
	}

	/// <summary>
	/// 敵のクラス
	/// </summary>
	class Enemy
	{
		public Vector2 pos;
		public Vector2 vec;
		public Vector2 toPos;
		public bool isActive = false;
		public bool isGenerateBullet = false;
		public int enemyNo = 0;
		public int hp = 0;
		public float speed = 1.0f;

		private float bulletGenerateInterval = 0.5f;
		private float bulletGenerateTimeCnt;

		//弾を生成
		public void GenerateBullet(float _timeStep)
		{
			if (isGenerateBullet)
				return;
			bulletGenerateTimeCnt += _timeStep;
			if (bulletGenerateTimeCnt < bulletGenerateInterval)
				return;

			isGenerateBullet = true;
		}

		//弾を生成した後
		public void GenerateBulletEnd()
		{
			bulletGenerateTimeCnt = 0.0f;
			isGenerateBullet = false;
		}
	}

	/// <summary>
	/// インスペクタ拡張
	/// </summary>
	public override void OnInspectorGUI()
	{
		if (!isStart)
		{
			DrawStart();
			return;
		}

		if (isGameOver)
		{
			DrawGameOver();
			return;
		}

		CalcDeltaTime();
		SetBackGround();
		InputKey();
		PlayerMove();
		MoveBullet();
		MoveEnemy();
		HitCheck();
		IsShotBullet();
		GenerateEnemy();
		RendererWindow();
		Repaint();
	}

	/// <summary>
	/// 初期化
	/// </summary>
	void Init()
	{
		//アイコンをロード
		enemyTex = new Texture2D[3];
		bulletTex = new Texture2D[2];
		player = EditorGUIUtility.FindTexture("SceneAsset Icon");
		enemyTex[0] = EditorGUIUtility.FindTexture("console.warnicon");
		enemyTex[1] = EditorGUIUtility.FindTexture("console.erroricon");
		enemyTex[2] = EditorGUIUtility.FindTexture("ViewToolOrbit");
		bulletTex[0] = EditorGUIUtility.FindTexture("PauseButton");
		bulletTex[1] = EditorGUIUtility.FindTexture("MoveTool");

		playerPos = new Vector2(0.5f, 1.0f);
		Debug.Log("Init");

		//自分の弾を生成
		for (int i = 0; i < 20; i++)
		{
			var bullet = new Bullet(bulletDefPos, Vector2.zero, false, true, 0, 1.0f);
			bulletList.Add(bullet);
		}

		//敵の弾を生成
		for (int i = 0; i < 500; i++)
		{
			var bullet = new Bullet(bulletDefPos, Vector2.zero, false, false, 1, 1.0f);
			bulletList.Add(bullet);
		}

		//敵を生成する
		for (int i = 0; i < 20; i++)
		{
			var enemy = new Enemy();
			enemy.isActive = false;
			enemy.vec = Vector2.zero;
			enemy.speed = 0.0f;
			enemyList.Add(enemy);
		}

		score = 0;
		isStart = true;
		isGameOver = false;
	}

	/// <summary>
	/// 背景の作成
	/// </summary>
	void SetBackGround()
	{
		areaRect = GUILayoutUtility.GetRect(Screen.width, 300.0f);
	}

	/// <summary>
	/// 表示する
	/// </summary>
	void RendererWindow()
	{
		//プレイヤーを表示
		Graphics.DrawTexture(GetTextureRect(playerPos, player, 1.0f), player);

		//弾を表示
		foreach (var bullet in bulletList)
		{
			//有効な弾だけ表示
			if (bullet.isActive)
				GUI.DrawTexture(GetTextureRect(bullet.pos, bulletTex[bullet.bulletNo], 1.0f), bulletTex[bullet.bulletNo]);
		}

		//敵を表示
		foreach (var enemy in enemyList)
		{
			//有効な敵だけ表示
			if (enemy.isActive)
				GUI.DrawTexture(GetTextureRect(enemy.pos, enemyTex[enemy.enemyNo], 1.0f), enemyTex[enemy.enemyNo]);
		}
	}

	/// <summary>
	/// プレイヤーが移動する
	/// </summary>
	void PlayerMove()
	{
		//次の座標を設定
		Vector2 nextPos = playerPos + playerVec * PLAYER_SPEED * deltaTime;

		//X座標とY座標を設定する
		playerPos.x = Mathf.Clamp(nextPos.x, 0.0f, 1.0f);
		playerPos.y = Mathf.Clamp(nextPos.y, 0.0f, 1.0f);
	}


	/// <summary>
	/// 弾を撃つかどうか
	/// </summary>
	void IsShotBullet()
	{
		if (bulletTimeCnt > PLAYER_BULLET_INTERVAL &&
			(keyFlag & KeyFlag.Key_Space) == KeyFlag.Key_Space)
		{
			//次に撃つまでのintervalがあり、かつキーが押されている時
			bulletTimeCnt = 0.0f;
			ShotBullet(true, playerPos, new Vector2(0, -1));
		}

		bulletTimeCnt += deltaTime;

		foreach (var e in enemyList)
		{
			if (!e.isActive)
				return;

			e.GenerateBullet(deltaTime);
			if (!e.isGenerateBullet)
				continue;

			e.GenerateBulletEnd();
			var ran = Random.Range(0, 3);
			var bulletVec = Vector2.zero;

			var bulletCnt = 3;
			if (e.enemyNo == 0)
				bulletCnt = 1;
			else if (e.enemyNo == 1)
				bulletCnt = 3;
			else if (e.enemyNo == 2)
				bulletCnt = 5;

			var angleInterval = 25.0f;
			float rad = 0.0f;
			float x = 0.0f;
			float y = 0.0f;
			float angleOffset = 0.0f;

			for (int i = 0; i < bulletCnt; i++)
			{
				switch (ran)
				{
					case 0:
						angleOffset = -20;
						break;
					case 1:
						angleOffset = 0;
						break;
					case 2:
						angleOffset = 20;
						break;
				}
				rad = ((i * angleInterval) + angleOffset) * (Mathf.PI / 180);
				x = (float)(Mathf.Sin(rad) * 0.1f);
				y = (float)(Mathf.Cos(rad) * 0.1f);
				bulletVec = new Vector2(x, y);
				ShotBullet(false, e.pos, bulletVec);
			}
		}
	}


	/// <summary>
	/// 弾を発射する
	/// </summary>
	void ShotBullet(bool _isPlayer, Vector2 _generatePos, Vector2 _vec)
	{
		foreach (Bullet bullet in bulletList)
		{
			if (bullet.isActive)
				continue;

			if (_isPlayer)
			{
				bullet.isActive = true;
				bullet.isPlayerBullet = true;
				bullet.pos = _generatePos;
				bullet.vec = _vec;
				bullet.speed = PLAYER_BULLET_SPEED;
				bullet.bulletNo = 0;
				break;
			}
			else
			{
				bullet.isActive = true;
				bullet.isPlayerBullet = false;
				bullet.pos = _generatePos;
				bullet.vec = _vec;
				bullet.speed = PLAYER_BULLET_SPEED;
				bullet.bulletNo = 1;
				break;
			}
		}
	}

	/// <summary>
	/// 敵を生成する
	/// </summary>
	void GenerateEnemy()
	{
		generateTimeCnt += deltaTime;

		var enemyCnt = 0;
		foreach (var enemy in enemyList)
		{
			if (enemy.isActive)
				enemyCnt++;
		}

		if (generateTimeCnt > 2.0f && enemyCnt < ENEMY_MAX_NUM)
		{
			//敵のリストからアクティブじゃない敵を持ってくる
			foreach (var e in enemyList)
			{
				var idx = 0;
				if (score > 2000)
					idx = 2;
				else if (score > 1000)
					idx = 1;

				//もしアクティブじゃないとき
				if (!e.isActive)
				{
					e.enemyNo = idx;
					e.isActive = true;
					e.hp = 10;
					e.speed = 0.5f;
					e.vec = new Vector2(Random.Range(-0.2f, 0.2f), 0.2f);
					var insPos = new Vector2(Random.Range(0.3f, 0.7f), 0.0f);
					e.pos = insPos;
					generateTimeCnt = 0.0f;

					break;
				}
			}
		}
	}



	/// <summary>
	/// 当たり判定
	/// </summary>
	void HitCheck()
	{
		//自弾と敵の当たり判定
		foreach (var bullet in bulletList)
		{
			if (!bullet.isActive)
				continue;

			if (bullet.isPlayerBullet)
			{
				foreach (var enemy in enemyList)
				{
					if (!enemy.isActive)
						continue;

					if (Vector2.Distance(enemy.pos, bullet.pos) < 0.05f)
					{
						//敵と弾を削除
						bullet.isActive = false;
						enemy.isActive = false;
						defeatCnt++;
						score += 100;
					}
				}
			}
			else
			{
				if (Vector2.Distance(playerPos, bullet.pos) < 0.03f)
					GameOver();
			}
		}

		//敵と自分の当たり判定
		foreach (var enemy in enemyList)
		{
			if (!enemy.isActive)
				continue;

			//敵とプレイヤーが当たった時
			if (Vector2.Distance(enemy.pos, playerPos) < 0.1f)
				GameOver();
			else
				GUI.color = Color.white;
		}
	}



	/// <summary> 
	/// 弾を移動する
	/// </summary>
	void MoveBullet()
	{
		foreach (var bullet in bulletList)
		{
			if (bullet.isActive)
			{
				//弾が移動する
				bullet.pos += bullet.vec * bullet.speed * deltaTime;

				//X座標とY座標を設定する
				if (bullet.pos.x > 1.0f ||
					bullet.pos.y > 1.0f ||
					bullet.pos.x < 0 ||
					bullet.pos.y < 0)
				{
					bullet.isActive = false;
					bullet.pos = bulletDefPos;
					bullet.vec = Vector2.zero;
				}
			}
		}
	}


	/// <summary> 
	/// 敵を移動する
	/// </summary>
	void MoveEnemy()
	{
		foreach (var enemy in enemyList)
		{
			if (enemy.isActive)
			{
				//弾が移動する
				enemy.pos += enemy.vec * enemy.speed * deltaTime;

				//敵のサイズを取得
				Vector2 eSize = new Vector2(enemyTex[enemy.enemyNo].width * TEXTURE_SIZE, enemyTex[enemy.enemyNo].height * TEXTURE_SIZE);

				//X座標とY座標を設定する
				if (enemy.pos.x > 1.0f || enemy.pos.x < 0 || enemy.pos.y > 1.0f)
				{
					enemy.isActive = false;
					enemy.pos = bulletDefPos;
					enemy.vec = Vector2.zero;
				}
			}
		}
	}



	/// <summary>
	/// テクスチャに合ったRectを返す
	/// </summary>
	/// <param name="_pos"></param>
	/// <param name="_tex"></param>
	/// <returns></returns>
	Rect GetTextureRect(Vector2 _pos, Texture2D _tex, float _scale)
	{
		Vector2 size = new Vector2(_tex.width * _scale, _tex.height * _scale);
		Vector2 pos = new Vector2(areaRect.x + (_pos.x * areaRect.width) - (size.x / 2.0f), areaRect.y + (_pos.y * areaRect.height) - (size.y / 2.0f));
		return new Rect(pos.x, pos.y, size.x, size.y);
	}

	/// <summary>
	/// ゲームオーバーになった時
	/// </summary>
	void GameOver()
	{
		isGameOver = true;
		if (EditorPrefs.GetInt(SAVE_KEY, 0) < score)
			EditorPrefs.SetInt(SAVE_KEY, score);
		UnityEditor.EditorUtility.DisplayDialog("GameOver", "You Score : " + score.ToString("00"), "OK");

	}

	/// <summary>
	/// スタート画面
	/// </summary>
	void DrawStart()
	{
		EditorGUILayout.Space();
		if (GUILayout.Button("Start"))
			Init();
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("HightScore", EditorPrefs.GetInt(SAVE_KEY, 0).ToString("000000"));
		EditorGUILayout.Space();
		Repaint();
	}

	/// <summary>
	/// ゲームオーバー画面
	/// </summary>
	void DrawGameOver()
	{
		EditorGUILayout.Space();
		if (GUILayout.Button("ReStart"))
			Init();
		EditorGUILayout.Space();
		EditorGUILayout.LabelField("HightScore", EditorPrefs.GetInt(SAVE_KEY, 0).ToString("000000"));
		EditorGUILayout.Space();
		Repaint();
	}

	/// <summary>
	/// セレクト画面を描画する
	/// </summary>
	void DrawSelectPlayer()
	{

	}

	/// <summary>
	/// キー入力
	/// 左右や上下等同時に押したときにも対応
	/// </summary>
	void InputKey()
	{
		Event e = Event.current;

		if (e.type == EventType.KeyDown)
		{
			//移動キーフラグ
			if (e.keyCode == KeyCode.D) { keyFlag |= KeyFlag.Key_Right; }
			if (e.keyCode == KeyCode.A) { keyFlag |= KeyFlag.Key_Left; }
			if (e.keyCode == KeyCode.W) { keyFlag |= KeyFlag.Key_Up; }
			if (e.keyCode == KeyCode.S) { keyFlag |= KeyFlag.Key_Down; }

			//左右同時押しの時
			if ((keyFlag & (KeyFlag.Key_Right | KeyFlag.Key_Left)) == (KeyFlag.Key_Right | KeyFlag.Key_Left))
			{
				playerVec.x = 0;
			}
			else
			{
				if ((keyFlag & KeyFlag.Key_Right) == KeyFlag.Key_Right)
					playerVec.x = 1;
				if ((keyFlag & KeyFlag.Key_Left) == KeyFlag.Key_Left)
					playerVec.x = -1;
			}

			//上下同時押しの時
			if ((keyFlag & (KeyFlag.Key_Up | KeyFlag.Key_Down)) == (KeyFlag.Key_Up | KeyFlag.Key_Down))
			{
				playerVec.y = 0;
			}
			else
			{
				if ((keyFlag & KeyFlag.Key_Up) == KeyFlag.Key_Up)
					playerVec.y = -1;
				if ((keyFlag & KeyFlag.Key_Down) == KeyFlag.Key_Down)
					playerVec.y = 1;
			}

			//ショットキーフラグ
			if (e.keyCode == KeyCode.Space)
				keyFlag |= KeyFlag.Key_Space;
		}
		else if (e.type == EventType.KeyUp)
		{
			//フラグを削除する
			if (e.keyCode == KeyCode.D) { keyFlag = keyFlag & ~KeyFlag.Key_Right; }
			if (e.keyCode == KeyCode.A) { keyFlag = keyFlag & ~KeyFlag.Key_Left; }
			if (e.keyCode == KeyCode.W) { keyFlag = keyFlag & ~KeyFlag.Key_Up; }
			if (e.keyCode == KeyCode.S) { keyFlag = keyFlag & ~KeyFlag.Key_Down; }

			//離した逆側のキーが押されていた時
			if ((keyFlag & KeyFlag.Key_Right) == KeyFlag.Key_Right)
				playerVec.x = 1;
			else if ((keyFlag & KeyFlag.Key_Left) == KeyFlag.Key_Left)
				playerVec.x = -1;
			else
				playerVec.x = 0;


			//離した逆側のキーが押されていた時
			if ((keyFlag & KeyFlag.Key_Down) == KeyFlag.Key_Down)
				playerVec.y = 1;
			else if ((keyFlag & KeyFlag.Key_Up) == KeyFlag.Key_Up)
				playerVec.y = -1;
			else
				playerVec.y = 0;

			//スペースキーが押されているとき
			if (e.keyCode == KeyCode.Space)
			{
				keyFlag = keyFlag & ~KeyFlag.Key_Space;
			}
		}
	}

	/// <summary>
	/// DeltaTimeを計算する
	/// </summary>
	void CalcDeltaTime()
	{
		now = (float)EditorApplication.timeSinceStartup;
		deltaTime = now - prevTime;
		prevTime = now;
	}

	//Enemy
	//EditorGUIUtility.FindTexture( "ViewToolOrbit" )
	//EditorGUIUtility.FindTexture( "console.erroricon" )
	//EditorGUIUtility.FindTexture( "console.warnicon" )

	//player
	//EditorGUIUtility.FindTexture( "SceneAsset Icon" )

	//playerBUllet
	//EditorGUIUtility.FindTexture( "PauseButton" )

	//enemyBUllet
	//EditorGUIUtility.FindTexture( "MoveTool" )
}
#endif