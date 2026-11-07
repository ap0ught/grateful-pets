using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameplay : MonoBehaviour
{
	public GridLayoutGroup grid;

	public AccessibleUIGroupRoot container;

	public Text m_MovesLabel;

	public UAP_BaseElement m_MovesLabel_Access;

	public Text[] m_GoalsLabel;

	public Image[] m_GoalsImages;

	public GameObject[] m_GoalsCheckmarks;

	public GameObject[] m_GoalsHighlightPos;

	public GameObject m_SelectionFrame;

	public AudioSource m_SFXPlayer;

	public Image m_SoundToggle;

	public Sprite m_SoundOn;

	public Sprite m_SoundOff;

	public AudioSource m_MusicPlayer;

	public AudioClip m_ActiveTile;

	public AudioClip m_SwapAborted;

	public AudioClip m_NoMatch3;

	public AudioClip m_Match3;

	public AudioClip m_GoalsMatch3;

	public AudioClip m_FallingPieces;

	public GameObject m_LevelGoalHighlightEffect;

	public Sprite[] m_GemTextures = new Sprite[gridpanel.tileTypeCount];

	private int m_CellCountX = -1;

	private int m_CellCountY = -1;

	private List<gridpanel> m_GridTiles = new List<gridpanel>();

	public bool m_MakeSquares = true;

	private int m_BaseCellSize = 85;

	private int m_BaseCellCountX = 7;

	private int m_BaseCellCountY = 11;

	public static gameplay Instance;

	private int m_MovesLeft = 15;

	private int m_MoveCount;

	private float m_GameDuration;

	private List<int> m_Cleared = new List<int>();

	private List<int> m_LevelGoals = new List<int>();

	private int m_TileTypeCount;

	private bool m_Paused;

	private string m_LevelGoalsString = "";

	private int m_MovesGained;

	private gridpanel m_SelectedTile;

	private bool m_levelGoalUpdatedWithMove;

	public static int DifficultyLevel;

	private bool m_IsPreviewingSwap;

	private float m_SwapPreviewTimer = -1f;

	private float m_SwapPreviewDuration = 0.1f;

	private int m_PreviewIndex1 = -1;

	private int m_PreviewIndex2 = -1;

	private Vector3 m_Previewposition1;

	private Vector3 m_Previewposition2;

	private gameplay()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		EnableMusic(PlayerPrefs.GetInt("Music_Enabled", 1) == 1);
	}

	public Sprite GetTileTypeSprite(int tileType)
	{
		return m_GemTextures[tileType];
	}

	public void InitBoard(int countX, int countY, int tiletypeCount, int moveCount, List<int> levelGoals)
	{
		UAP_AccessibilityManager.BlockInput(block: true);
		m_SelectionFrame.SetActive(value: false);
		m_CellCountX = countX;
		m_CellCountY = countY;
		m_MoveCount = 0;
		m_GameDuration = 0f;
		m_TileTypeCount = tiletypeCount;
		for (int i = 0; i < m_GoalsLabel.Length; i++)
		{
			m_GoalsLabel[i].gameObject.SetActive(value: false);
		}
		for (int j = 0; j < m_GoalsImages.Length; j++)
		{
			m_GoalsImages[j].gameObject.SetActive(value: false);
		}
		for (int k = 0; k < m_GoalsImages.Length; k++)
		{
			m_GoalsCheckmarks[k].SetActive(value: false);
		}
		m_LevelGoals.Clear();
		m_LevelGoals = levelGoals;
		m_Cleared.Clear();
		for (int l = 0; l < gridpanel.tileTypeCount; l++)
		{
			m_Cleared.Add(0);
		}
		while (transform.childCount > 0)
		{
			Transform child = transform.GetChild(0);
			child.SetParent(null);
			Object.DestroyImmediate(child.gameObject);
		}
		m_GridTiles.Clear();
		Vector2 cellSize = new Vector2(0f, 0f);
		cellSize.x = (float)m_BaseCellSize / (float)countX * (float)m_BaseCellCountX;
		cellSize.y = (float)m_BaseCellSize / (float)countY * (float)m_BaseCellCountY;
		if (m_MakeSquares)
		{
			if (cellSize.x < cellSize.y)
			{
				cellSize.y = cellSize.x;
			}
			else
			{
				cellSize.x = cellSize.y;
			}
		}
		grid.cellSize = cellSize;
		grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
		grid.constraintCount = countX;
		GameObject original = Resources.Load("Panel") as GameObject;
		int num = countX * countY;
		for (int m = 0; m < num; m++)
		{
			GameObject gameObject = Object.Instantiate(original);
			gameObject.transform.SetParent(transform, worldPositionStays: false);
			gameObject.GetComponent<gridpanel>().SetIndex(m, countX);
			m_GridTiles.Add(gameObject.GetComponent<gridpanel>());
		}
		m_SelectedTile = null;
		m_MovesLeft = moveCount;
		UpdateMoveLabel();
		UpdateLevelGoalsLabels();
		RandomizeTiles();
		SaveGameState();
		int num2 = 0;
		for (int n = 0; n < gridpanel.tileTypeCount; n++)
		{
			if (m_LevelGoals[n] > 0)
			{
				m_GoalsImages[num2].gameObject.SetActive(value: true);
				m_GoalsImages[num2].sprite = GetTileTypeSprite(n);
				num2++;
			}
		}
		string text = "Level Goals: ";
		int num3 = 0;
		for (int num4 = 0; num4 < gridpanel.tileTypeCount; num4++)
		{
			if (m_LevelGoals[num4] >= 0)
			{
				if (m_Cleared[num4] > m_LevelGoals[num4])
				{
					_ = m_LevelGoals[num4];
				}
				num3++;
				if (m_LevelGoalsString.Length > 0)
				{
					text += "\n ";
				}
				if (num3 == num2 && num2 > 1)
				{
					text += "And ";
				}
				text = text + m_LevelGoals[num4].ToString("0") + " " + gridpanel.GetTileTypeName(num4) + " gems, ";
			}
		}
		text += "need to be destroyed.";
		UAP_AccessibilityManager.Say("Game Started. Double Tap with two fingers to pause the game.", canBeInterrupted: false);
		UAP_AccessibilityManager.Say(text + " \nYou have " + m_MovesLeft.ToString("0") + " moves remaining.");
		UAP_AccessibilityManager.Say("Tap once with two fingers to repeat the level goals.");
		UAP_AccessibilityManager.BlockInput(block: false);
	}

	private void Start()
	{
		UAP_AccessibilityManager.RegisterOnPauseToggledCallback(OnUserPause);
		UAP_AccessibilityManager.RegisterOnTwoFingerSingleTapCallback(OnRepeatLevelGoals);
		int num = 5;
		int moveCount = 10;
		int num2 = 1;
		int maxExclusive = 15;
		if (DifficultyLevel == 1)
		{
			num = 6;
			moveCount = 15;
			num2 = 2;
			maxExclusive = 20;
		}
		else if (DifficultyLevel == 2)
		{
			num = 7;
			moveCount = 20;
			num2 = 3;
			maxExclusive = 30;
		}
		List<int> list = new List<int>();
		bool flag = false;
		list.Clear();
		for (int i = 0; i < gridpanel.tileTypeCount; i++)
		{
			list.Add(-1);
		}
		while (!flag)
		{
			int num3 = 0;
			for (int j = 0; j < num; j++)
			{
				if (list[j] < 0 && Random.Range(0, 5) < 3)
				{
					int value = Random.Range(5, maxExclusive);
					list[j] = value;
					num3++;
					if (num3 == num2)
					{
						break;
					}
				}
			}
			flag = num3 == num2;
		}
		InitBoard(6, 6, num, moveCount, list);
	}

	public void OnRepeatLevelGoals()
	{
		UAP_AccessibilityManager.Say(m_LevelGoalsString);
		UAP_AccessibilityManager.Say(m_MovesLabel_Access.m_Text);
	}

	public void OnUserPause()
	{
		if (!m_Paused)
		{
			m_Paused = true;
			m_MusicPlayer.Pause();
			transform.parent.parent.gameObject.SetActive(value: false);
			Object.Instantiate(Resources.Load("Pause Menu"));
		}
	}

	private void Update()
	{
		m_GameDuration += Time.unscaledDeltaTime;
		if (m_IsPreviewingSwap)
		{
			m_SwapPreviewTimer -= Time.unscaledDeltaTime;
			float t = 1f - m_SwapPreviewTimer / m_SwapPreviewDuration;
			gridpanel gridTile = GetGridTile(m_PreviewIndex1);
			gridpanel gridTile2 = GetGridTile(m_PreviewIndex2);
			gridTile.m_GemImage.transform.position = Vector3.Lerp(m_Previewposition1, m_Previewposition2, t);
			gridTile2.m_GemImage.transform.position = Vector3.Lerp(m_Previewposition2, m_Previewposition1, t);
		}
	}

	private int GetRandomTile()
	{
		return Random.Range(0, m_TileTypeCount);
	}

	private void RandomizeTiles()
	{
		for (int i = 0; i < m_GridTiles.Count; i++)
		{
			m_GridTiles[i].SetTileType(GetRandomTile());
		}
		List<int> list = null;
		while ((list = FindMatch3()) != null)
		{
			foreach (int item in list)
			{
				m_GridTiles[item].SetTileType(GetRandomTile());
			}
		}
		SaveGameState();
	}

	private Vector2 ConvertIndexToXYCoordinates(int index)
	{
		Vector2 result = default;
		result.y = Mathf.FloorToInt((float)index / (float)m_CellCountX);
		result.x = Mathf.FloorToInt((float)index - result.y * (float)m_CellCountX);
		return result;
	}

	private void AbortSelection()
	{
		m_SFXPlayer.Stop();
		m_SFXPlayer.PlayOneShot(m_SwapAborted);
		CancelPreview();
		UAP_AccessibilityManager.Say("Swap canceled.", canBeInterrupted: false, allowVoiceOver: false, UAP_AudioQueue.EInterrupt.All);
		m_SelectionFrame.SetActive(value: false);
		m_SelectedTile = null;
	}

	public void ActivateTile(int index)
	{
		UAP_AccessibilityManager.Say("", canBeInterrupted: false, allowVoiceOver: false, UAP_AudioQueue.EInterrupt.All);
		Vector2 vector = ConvertIndexToXYCoordinates(index);
		gridpanel gridTile = GetGridTile(index);
		if (gridTile == null || m_SelectedTile == gridTile)
		{
			AbortSelection();
			return;
		}
		if (m_SelectedTile == null)
		{
			m_SFXPlayer.clip = m_ActiveTile;
			m_SFXPlayer.loop = true;
			m_SFXPlayer.Play();
			m_SelectedTile = gridTile;
			m_SelectionFrame.SetActive(value: true);
			m_SelectionFrame.transform.position = m_SelectedTile.transform.position;
			return;
		}
		Vector2 vector2 = ConvertIndexToXYCoordinates(m_SelectedTile.GetIndex()) - vector;
		if (Mathf.Abs(vector2.x) + Mathf.Abs(vector2.y) > 1f)
		{
			AbortSelection();
			return;
		}
		UAP_AccessibilityManager.BlockInput(block: true);
		CancelPreview(swapSuccessful: true);
		int tileType = m_SelectedTile.GetTileType();
		m_SelectedTile.SetTileType(gridTile.GetTileType());
		gridTile.SetTileType(tileType);
		m_SelectedTile = null;
		m_SelectionFrame.SetActive(value: false);
		m_SFXPlayer.Stop();
		m_MovesGained = 0;
		m_MoveCount++;
		m_levelGoalUpdatedWithMove = false;
		EvaluateBoard();
	}

	private gridpanel GetGridTile(int index)
	{
		if (index < 0 || index >= m_GridTiles.Count)
		{
			return null;
		}
		return m_GridTiles[index];
	}

	private void UpdateMoveLabel()
	{
		m_MovesLabel.text = "Moves: " + m_MovesLeft.ToString("0");
		m_MovesLabel_Access.m_Text = m_MovesLeft.ToString("0") + " moves left.";
	}

	private void UpdateLevelGoalsLabels()
	{
		int num = 0;
		m_LevelGoalsString = "Level Goals: \n";
		for (int i = 0; i < gridpanel.tileTypeCount; i++)
		{
			if (m_LevelGoals[i] >= 0)
			{
				m_GoalsLabel[num].gameObject.SetActive(value: true);
				m_GoalsImages[num].gameObject.SetActive(value: true);
				int num2 = m_Cleared[i];
				if (num2 > m_LevelGoals[i])
				{
					num2 = m_LevelGoals[i];
				}
				m_GoalsLabel[num].text = num2.ToString("0") + "/" + m_LevelGoals[i].ToString("0");
				m_GoalsLabel[num].GetComponent<UAP_BaseElement>().m_Text = gridpanel.GetTileTypeName(i) + ": " + num2.ToString("0") + " of " + m_LevelGoals[i].ToString("0");
				if (num2 == m_LevelGoals[i])
				{
					m_GoalsCheckmarks[num].SetActive(value: true);
				}
				if (m_LevelGoalsString.Length > 0)
				{
					m_LevelGoalsString += "\n";
				}
				m_LevelGoalsString = m_LevelGoalsString + gridpanel.GetTileTypeName(i) + ": " + num2.ToString("0") + " of " + m_LevelGoals[i].ToString("0") + ". \n";
				num++;
			}
		}
	}

	private int GetLevelGoalIndex(int tileType)
	{
		int num = 0;
		for (int i = 0; i < gridpanel.tileTypeCount; i++)
		{
			if (m_LevelGoals[i] >= 0)
			{
				if (i == tileType)
				{
					return num;
				}
				num++;
			}
		}
		return -1;
	}

	private void EvaluateBoard()
	{
		List<int> list = FindMatch3();
		if (list != null)
		{
			int tileType = m_GridTiles[list[0]].GetTileType();
			bool flag = false;
			if (m_LevelGoals[tileType] > 0 && m_LevelGoals[tileType] > m_Cleared[tileType])
			{
				m_levelGoalUpdatedWithMove = true;
				flag = true;
				m_SFXPlayer.PlayOneShot(m_GoalsMatch3);
				int levelGoalIndex = GetLevelGoalIndex(tileType);
				if (levelGoalIndex >= 0)
				{
					GameObject gameObject = Object.Instantiate(m_LevelGoalHighlightEffect);
					gameObject.transform.SetParent(m_GoalsHighlightPos[levelGoalIndex].transform, worldPositionStays: false);
					Object.Destroy(gameObject, 1f);
				}
				else
				{
					Debug.LogWarning("Couldn't find level goal index");
				}
				UpdateLevelGoalsLabels();
			}
			else
			{
				m_SFXPlayer.PlayOneShot(m_Match3);
			}
			m_Cleared[tileType] += list.Count;
			UAP_AccessibilityManager.Say("Matched " + list.Count + " " + m_GridTiles[list[0]].GetTileTypeName() + " gems.");
			if (flag)
			{
				int num = m_LevelGoals[tileType] - m_Cleared[tileType];
				if (num > 0)
				{
					UAP_AccessibilityManager.Say("Need " + num.ToString("0") + " more.");
				}
				else
				{
					UAP_AccessibilityManager.Say(m_GridTiles[list[0]].GetTileTypeName() + " level goal completed.");
				}
			}
			if (list.Count > 3)
			{
				m_MovesGained += list.Count - 1;
			}
			GameObject original = Resources.Load("Burst") as GameObject;
			foreach (int item in list)
			{
				m_GridTiles[item].SetTileType(-1);
				GameObject gameObject2 = Object.Instantiate(original);
				gameObject2.transform.SetParent(transform.parent, worldPositionStays: true);
				gameObject2.transform.position = m_GridTiles[item].transform.position;
				Object.Destroy(gameObject2, 0.45f);
			}
			float time = 0.25f;
			Invoke("EvaluateBoard", time);
		}
		else
		{
			Invoke("DropDownTiles", 0.05f);
		}
	}

	private void DropDownTiles()
	{
		bool flag = false;
		for (int i = 0; i < m_CellCountX; i++)
		{
			for (int num = m_CellCountY - 1; num >= 0; num--)
			{
				int index = GetIndex(i, num);
				if (m_GridTiles[index].GetTileType() == -1)
				{
					flag = true;
					if (num == 0)
					{
						m_GridTiles[index].SetTileType(GetRandomTile());
					}
					else
					{
						for (int num2 = num - 1; num2 >= 0; num2--)
						{
							int index2 = GetIndex(i, num2);
							int tileType = m_GridTiles[index2].GetTileType();
							if (tileType < 0 && num2 == 0)
							{
								m_GridTiles[index].SetTileType(GetRandomTile());
								break;
							}
							if (tileType >= 0)
							{
								m_GridTiles[index].SetTileType(tileType);
								m_GridTiles[index2].SetTileType(-1);
								break;
							}
						}
					}
				}
			}
		}
		if (flag)
		{
			m_SFXPlayer.PlayOneShot(m_FallingPieces);
			float time = 0.25f;
			Invoke("EvaluateBoard", time);
		}
		else
		{
			FinishBoardEvaluation();
		}
	}

	private int GetIndex(int x, int y)
	{
		return x + y * m_CellCountX;
	}

	private void FinishBoardEvaluation()
	{
		if (!m_levelGoalUpdatedWithMove)
		{
			m_MovesLeft--;
		}
		m_levelGoalUpdatedWithMove = false;
		m_MovesLeft += m_MovesGained;
		if (m_MovesGained > 0)
		{
			UAP_AccessibilityManager.Say("Gained " + m_MovesGained.ToString("0") + " move" + ((m_MovesGained > 1) ? "s." : "."));
		}
		UAP_AccessibilityManager.Say(m_MovesLeft.ToString("0") + " moves left.");
		m_MovesGained = 0;
		UpdateMoveLabel();
		UpdateLevelGoalsLabels();
		if (m_MovesLeft <= 0)
		{
			gameover.GameWon = false;
			gameover.GameDuration = m_GameDuration;
			gameover.MoveCount = m_MoveCount;
			DestroyMyself();
			Object.Instantiate(Resources.Load("Game Over Screen"));
			return;
		}
		bool flag = true;
		for (int i = 0; i < gridpanel.tileTypeCount; i++)
		{
			if (m_LevelGoals[i] >= 0 && m_Cleared[i] < m_LevelGoals[i])
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			gameover.GameWon = true;
			gameover.GameDuration = m_GameDuration;
			gameover.MoveCount = m_MoveCount;
			DestroyMyself();
			Object.Instantiate(Resources.Load("Game Over Screen"));
		}
		else
		{
			SaveGameState();
			UAP_AccessibilityManager.BlockInput(block: false);
		}
	}

	private void SaveGameState()
	{
	}

	private List<int> FindMatch3()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < m_GridTiles.Count; i++)
		{
			Vector2 vector = ConvertIndexToXYCoordinates(i);
			int tileType = m_GridTiles[i].GetTileType();
			if (tileType < 0)
			{
				continue;
			}
			list.Clear();
			list.Add(i);
			int num = 1;
			for (int j = (int)vector.x + 1; j < m_CellCountX; j++)
			{
				int num2 = j + (int)vector.y * m_CellCountX;
				if (m_GridTiles[num2].GetTileType() != tileType)
				{
					break;
				}
				list.Add(num2);
				num++;
			}
			if (num >= 3)
			{
				int count = list.Count;
				for (int k = 0; k < count; k++)
				{
					int num3 = list[k];
					Vector2 vector2 = ConvertIndexToXYCoordinates(num3);
					if (GetTileType(vector2.x, vector2.y + 1f) == tileType && GetTileType(vector2.x, vector2.y + 2f) == tileType)
					{
						list.Add(num3 + m_CellCountX);
						list.Add(num3 + m_CellCountX + m_CellCountX);
					}
				}
				string text = "";
				{
					foreach (int item in list)
					{
						text = text + ConvertIndexToXYCoordinates(item).ToString() + " ";
					}
					return list;
				}
			}
			list.Clear();
			list.Add(i);
			num = 1;
			for (int l = (int)vector.y + 1; l < m_CellCountY; l++)
			{
				int num4 = (int)vector.x + l * m_CellCountX;
				if (m_GridTiles[num4].GetTileType() != tileType)
				{
					break;
				}
				list.Add(num4);
				num++;
			}
			if (num < 3)
			{
				continue;
			}
			int count2 = list.Count;
			for (int m = 0; m < count2; m++)
			{
				int num5 = list[m];
				bool flag = false;
				bool flag2 = false;
				Vector2 vector3 = ConvertIndexToXYCoordinates(num5);
				if (GetTileType(vector3.x - 2f, vector3.y) == tileType && GetTileType(vector3.x - 1f, vector3.y) == tileType)
				{
					list.Add(num5 - 2);
					list.Add(num5 - 1);
				}
				if (GetTileType(vector3.x + 2f, vector3.y) == tileType && GetTileType(vector3.x + 1f, vector3.y) == tileType)
				{
					list.Add(num5 + 2);
					list.Add(num5 + 1);
				}
				if ((!flag || !flag2) && GetTileType(vector3.x - 1f, vector3.y) == tileType && GetTileType(vector3.x + 1f, vector3.y) == tileType)
				{
					if (!flag)
					{
						list.Add(num5 - 1);
					}
					if (!flag2)
					{
						list.Add(num5 + 1);
					}
				}
			}
			string text2 = "";
			{
				foreach (int item2 in list)
				{
					text2 = text2 + ConvertIndexToXYCoordinates(item2).ToString() + " ";
				}
				return list;
			}
		}
		return null;
	}

	private int GetTileType(float xCoord, float yCoord)
	{
		if (xCoord < 0f || xCoord >= (float)m_CellCountX)
		{
			return -1;
		}
		if (yCoord < 0f || yCoord >= (float)m_CellCountY)
		{
			return -1;
		}
		int index = (int)xCoord + (int)yCoord * m_CellCountX;
		return m_GridTiles[index].GetTileType();
	}

	public void AbortGame()
	{
		DestroyMyself();
		Object.Instantiate(Resources.Load("Main Menu"));
	}

	private void DestroyMyself()
	{
		Object.DestroyImmediate(transform.parent.parent.gameObject);
	}

	public void ResumeGame()
	{
		m_Paused = false;
		m_MusicPlayer.UnPause();
		transform.parent.parent.gameObject.SetActive(value: true);
	}

	private void OnDestroy()
	{
		UAP_AccessibilityManager.UnregisterOnPauseToggledCallback(OnUserPause);
		UAP_AccessibilityManager.UnregisterOnTwoFingerSingleTapCallback(OnRepeatLevelGoals);
	}

	public void OnSoundToggle()
	{
		EnableMusic(!m_MusicPlayer.enabled);
	}

	private void EnableMusic(bool enable)
	{
		m_MusicPlayer.enabled = enable;
		m_SoundToggle.sprite = (enable ? m_SoundOn : m_SoundOff);
		PlayerPrefs.SetInt("Music_Enabled", enable ? 1 : 0);
		PlayerPrefs.Save();
	}

	public void PreviewDrag(int index1, int index2)
	{
		CancelPreview();
		gridpanel gridTile = GetGridTile(index1);
		gridpanel gridTile2 = GetGridTile(index2);
		if (!(gridTile == null) && !(gridTile2 == null))
		{
			m_IsPreviewingSwap = true;
			m_SwapPreviewTimer = m_SwapPreviewDuration;
			m_PreviewIndex1 = index1;
			m_PreviewIndex2 = index2;
			m_Previewposition1 = gridTile.transform.position;
			m_Previewposition2 = gridTile2.transform.position;
		}
	}

	public void CancelPreview(bool swapSuccessful = false)
	{
		if (m_IsPreviewingSwap)
		{
			m_IsPreviewingSwap = false;
			gridpanel gridTile = GetGridTile(m_PreviewIndex1);
			gridpanel gridTile2 = GetGridTile(m_PreviewIndex2);
			gridTile.m_GemImage.transform.position = m_Previewposition1;
			gridTile2.m_GemImage.transform.position = m_Previewposition2;
		}
	}
}
