using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class gridpanel : MonoBehaviour
{
	private enum EVecDir
	{
		None,
		Up,
		Left,
		Right,
		Down
	}

	public Image m_GemImage;

	public Sprite[] m_BGTexture = new Sprite[2];

	private GameObject m_Tile;

	private UAP_BaseElement m_AccessibilityHelper;

	private Button m_Button;

	private int m_Index = -1;

	private int m_xWidth = -1;

	private int m_TileType = -1;

	private static string[] typeList = new string[7] { "Ruby", "Blue", "Emerald", "Purple", "Gold", "Orange", "Crystal" };

	public static int tileTypeCount = 7;

	public Sprite[] m_GemTextures = new Sprite[tileTypeCount];

	private int m_BGType;

	private EVecDir m_LastPreviewDir;

	public string GetTileTypeName()
	{
		if (m_TileType >= 0 && m_TileType < typeList.Length)
		{
			return typeList[m_TileType];
		}
		return "invalid";
	}

	public static string GetTileTypeName(int tileType)
	{
		if (tileType >= 0 && tileType < typeList.Length)
		{
			return typeList[tileType];
		}
		return "invalid";
	}

	private void SetBGType(int bgIndex = 0)
	{
		m_BGType = bgIndex;
		if (m_Button != null)
		{
			m_Button.GetComponent<Image>().sprite = m_BGTexture[bgIndex];
		}
	}

	public void SetTileType(int tileType)
	{
		m_TileType = tileType;
		if (m_Tile == null)
		{
			m_Tile = Object.Instantiate(Resources.Load("Button")) as GameObject;
			m_Tile.transform.SetParent(transform, worldPositionStays: false);
			m_Tile.transform.SetAsFirstSibling();
			m_AccessibilityHelper = m_Tile.GetComponentInChildren<UAP_BaseElement>();
			m_AccessibilityHelper.m_TryToReadLabel = false;
			m_Button = m_Tile.GetComponentInChildren<Button>();
			m_Button.onClick.AddListener(OnButtonPress);
			EventTrigger eventTrigger = m_Button.gameObject.AddComponent<EventTrigger>();
			EventTrigger.Entry entry = new EventTrigger.Entry();
			entry.eventID = EventTriggerType.EndDrag;
			entry.callback.AddListener((BaseEventData data) =>
			{
				OnDragEndDelegate((PointerEventData)data);
			});
			eventTrigger.triggers.Add(entry);
			EventTrigger.Entry entry2 = new EventTrigger.Entry();
			entry2.eventID = EventTriggerType.Drag;
			entry2.callback.AddListener((BaseEventData data) =>
			{
				OnDragUpdateDelegate((PointerEventData)data);
			});
			eventTrigger.triggers.Add(entry2);
			EventTrigger.Entry entry3 = new EventTrigger.Entry();
			entry3.eventID = EventTriggerType.PointerDown;
			entry3.callback.AddListener((BaseEventData data) =>
			{
				OnPointerDownDelegate((PointerEventData)data);
			});
			eventTrigger.triggers.Add(entry3);
			EventTrigger.Entry entry4 = new EventTrigger.Entry();
			entry4.eventID = EventTriggerType.PointerUp;
			entry4.callback.AddListener((BaseEventData data) =>
			{
				OnPointerUpDelegate((PointerEventData)data);
			});
			eventTrigger.triggers.Add(entry4);
			SetBGType(m_BGType);
		}
		if (tileType < 0)
		{
			m_AccessibilityHelper.m_Text = "none";
			m_GemImage.sprite = null;
			m_GemImage.gameObject.SetActive(value: false);
		}
		else
		{
			m_AccessibilityHelper.m_Text = GetTileTypeName();
			m_GemImage.sprite = m_GemTextures[tileType];
			m_GemImage.gameObject.SetActive(value: true);
			m_Button.gameObject.name = GetTileTypeName();
			gameObject.name = GetTileTypeName();
		}
	}

	public int GetTileType()
	{
		return m_TileType;
	}

	public void SetIndex(int index, int xWidth)
	{
		m_Index = index;
		m_xWidth = xWidth;
		if (xWidth % 2 == 1)
		{
			SetBGType(index % 2);
			return;
		}
		int num = index;
		bool flag = true;
		while (num >= xWidth)
		{
			flag = !flag;
			num -= xWidth;
		}
		if (flag)
		{
			index++;
		}
		SetBGType(index % 2);
	}

	public int GetIndex()
	{
		return m_Index;
	}

	public void OnPointerDownDelegate(PointerEventData data)
	{
		if (!UAP_AccessibilityManager.IsEnabled())
		{
			gameplay.Instance.ActivateTile(m_Index);
		}
	}

	public void OnPointerUpDelegate(PointerEventData data)
	{
		m_LastPreviewDir = EVecDir.None;
		if (!UAP_AccessibilityManager.IsEnabled() && ((data.pointerEnter != null) ? gameObject.GetComponent<gridpanel>() : null) == this && !data.dragging)
		{
			gameplay.Instance.ActivateTile(m_Index);
		}
	}

	public void OnDragUpdateDelegate(PointerEventData data)
	{
		if (UAP_AccessibilityManager.IsEnabled())
		{
			return;
		}
		Vector2 normalized = (data.position - data.pressPosition).normalized;
		EVecDir eVecDir = GetVectorDirection(normalized);
		if (IsSameTile(data.pointerEnter))
		{
			eVecDir = EVecDir.None;
		}
		if (eVecDir != m_LastPreviewDir)
		{
			m_LastPreviewDir = eVecDir;
			if (eVecDir != EVecDir.None)
			{
				gameplay.Instance.PreviewDrag(m_Index, GetNeighbourIndex(eVecDir));
			}
			else
			{
				gameplay.Instance.CancelPreview();
			}
		}
	}

	public void OnDragEndDelegate(PointerEventData data)
	{
		if (!UAP_AccessibilityManager.IsEnabled())
		{
			Vector2 normalized = (data.position - data.pressPosition).normalized;
			EVecDir vectorDirection = GetVectorDirection(normalized);
			if (IsSameTile(data.pointerEnter))
			{
				gameplay.Instance.ActivateTile(m_Index);
			}
			else
			{
				gameplay.Instance.ActivateTile(GetNeighbourIndex(vectorDirection));
			}
		}
	}

	private bool IsSameTile(GameObject pointerEnter)
	{
		if (pointerEnter != null && pointerEnter.transform.parent != null)
		{
			gridpanel component = pointerEnter.transform.parent.gameObject.GetComponent<gridpanel>();
			if (component != null && component == this)
			{
				return true;
			}
		}
		return false;
	}

	private int GetNeighbourIndex(EVecDir dir)
	{
		return dir switch
		{
			EVecDir.Up => m_Index - m_xWidth, 
			EVecDir.Down => m_Index + m_xWidth, 
			EVecDir.Left => m_Index - 1, 
			EVecDir.Right => m_Index + 1, 
			_ => m_Index, 
		};
	}

	private EVecDir GetVectorDirection(Vector2 vector)
	{
		if (Mathf.Abs(vector.x) > Mathf.Abs(vector.y))
		{
			if (vector.x > 0f)
			{
				return EVecDir.Right;
			}
			return EVecDir.Left;
		}
		if (vector.y > 0f)
		{
			return EVecDir.Up;
		}
		return EVecDir.Down;
	}

	public void OnButtonPress()
	{
		if (UAP_AccessibilityManager.IsEnabled())
		{
			gameplay.Instance.ActivateTile(m_Index);
		}
	}
}
