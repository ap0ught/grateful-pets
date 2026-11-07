using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible UI Group Root")]
public class AccessibleUIGroupRoot : MonoBehaviour
{
	public enum EUIElement
	{
		EUndefined,
		EButton,
		ELabel,
		EToggle,
		ESlider,
		ETextEdit,
		EDropDown
	}

	public class Accessible_UIElement
	{
		public EUIElement m_Type;

		public UAP_BaseElement m_Object;

		public Vector2 m_Pos = new Vector2(0f, 0f);

		public int m_PositionOrder = -1;

		public int m_SecondaryOrder;

		public bool AllowsVoiceOver()
		{
			return m_Object.m_AllowVoiceOver;
		}

		public bool ReadType()
		{
			return m_Object.m_ReadType;
		}

		public void CalculatePositionOrder(UAP_BaseElement uiElement, int backupIndex)
		{
			GameObject gameObject = uiElement.gameObject;
			if (uiElement.m_ManualPositionParent == null)
			{
				AccessibleUIGroupRoot uIGroupContainer = uiElement.GetUIGroupContainer();
				uiElement.m_ManualPositionParent = ((uIGroupContainer != null) ? uIGroupContainer.gameObject : null);
			}
			bool flag = uiElement.m_ManualPositionOrder >= 0 && uiElement.m_ManualPositionParent != null;
			if (flag)
			{
				gameObject = uiElement.m_ManualPositionParent;
			}
			Vector2 anchorMin = default;
			Vector2 anchorMax = default;
			Vector2 centerPos = default;
			bool flag2 = false;
			bool flag3 = false;
			RectTransform component = gameObject.GetComponent<RectTransform>();
			if (component != null)
			{
				Transform transform = component;
				RectTransform t = component;
				while (transform.parent != null)
				{
					if (transform.parent.gameObject.GetComponent<ScrollRect>() != null)
					{
						t = transform.parent.gameObject.GetComponent<RectTransform>();
						flag3 = true;
						break;
					}
					transform = transform.parent;
				}
				GetAbsoluteAnchors(t, out anchorMin, out anchorMax, out centerPos);
				flag2 = true;
			}
			if (flag2)
			{
				Vector3 vector = centerPos;
				m_Pos.x = vector.x;
				m_Pos.y = vector.y;
				m_Pos.Scale(new Vector2(1f / (float)Screen.width, 1f / (float)Screen.height));
				m_PositionOrder = (int)(10f * (m_Pos.x + 1000f * (1f - m_Pos.y)));
				if (flag)
				{
					m_PositionOrder += uiElement.m_ManualPositionOrder;
				}
				m_Pos.Scale(new Vector2(Screen.width, Screen.height));
				gameObject = uiElement.gameObject;
				if (flag3)
				{
					Vector2 vector2 = gameObject.transform.TransformPoint(new Vector3(0.5f, 0.5f, 0f));
					m_SecondaryOrder = (int)(10f * (vector2.x + 1000f * (1f - vector2.y)));
				}
			}
			else if (uiElement is UAP_BaseElement_3D)
			{
				if (uiElement.m_ManualPositionOrder >= 0)
				{
					m_PositionOrder = uiElement.m_ManualPositionOrder;
				}
				else
				{
					m_PositionOrder = backupIndex;
				}
				m_Pos = gameObject.transform.position;
			}
			else
			{
				Debug.LogWarning("[Accessibility] Could not find any UI transforms on UI element " + gameObject.name + " that the accessibility plugin can understand - ordering UI elements by manual position index (if present) or order of initialization.");
				if (uiElement.m_ManualPositionOrder >= 0)
				{
					m_PositionOrder = uiElement.m_ManualPositionOrder;
				}
				else
				{
					m_PositionOrder = backupIndex;
				}
				m_Pos = gameObject.transform.position;
			}
		}

		public Accessible_UIElement(UAP_BaseElement item, EUIElement type, int index)
		{
			m_Type = type;
			m_Object = item;
			if (item.GetTargetGameObject().GetComponentInParent<ScrollRect>() != null)
			{
				item.m_IsInsideScrollView = true;
			}
			CalculatePositionOrder(item, index);
			m_Object.m_PositionOrder = m_PositionOrder;
			m_Object.m_SecondaryOrder = m_SecondaryOrder;
			m_Object.m_Pos = m_Pos;
		}
	}

	public bool m_PopUp;

	public bool m_AllowExternalJoining;

	public bool m_AutoRead;

	public int m_Priority;

	public string m_ContainerName = "";

	[FormerlySerializedAs("m_IsNGUILocalizationKey")]
	public bool m_IsLocalizationKey;

	public bool m_2DNavigation;

	public bool m_ConstrainToContainerUp;

	public bool m_ConstrainToContainerDown;

	public bool m_ConstrainToContainerLeft;

	public bool m_ConstrainToContainerRight;

	public bool m_AllowTouchExplore = true;

	private bool m_HasStarted;

	private bool m_RefreshNextFrame;

	private bool m_ActivateContainerNextFrame;

	[Tooltip("This causes a 2 frame delay before the interface is accessible, but solves issues with screens that perform automatic UI elements ordering at start - such as any dynamically built UI, expanding scroll views, horizontal grids etc")]
	public bool m_DoubleCheckUIElementsPositions = true;

	private bool m_NeedsRefreshBeforeActivation = true;

	private List<Accessible_UIElement> m_AllElements = new List<Accessible_UIElement>();

	private UAP_BaseElement m_CurrentStartItem;

	private int m_CurrentItemIndex;

	public bool IsConstrainedToContainer(UAP_AccessibilityManager.ESDirection direction)
	{
		return direction switch
		{
			UAP_AccessibilityManager.ESDirection.ELeft => m_ConstrainToContainerLeft, 
			UAP_AccessibilityManager.ESDirection.EUp => m_ConstrainToContainerUp, 
			UAP_AccessibilityManager.ESDirection.ERight => m_ConstrainToContainerRight, 
			UAP_AccessibilityManager.ESDirection.EDown => m_ConstrainToContainerDown, 
			_ => false, 
		};
	}

	public static void GetAbsoluteAnchors(RectTransform t, out Vector2 anchorMin, out Vector2 anchorMax, out Vector2 centerPos, bool stopAtScrollView = false)
	{
		centerPos = t.TransformPoint(0.5f, 0.5f, 0f);
		anchorMin = t.anchorMin;
		anchorMax = t.anchorMax;
		Vector2 scale = new Vector3(1f / (float)Screen.width, 1f / (float)Screen.height, 0f);
		Vector2 vector = t.TransformPoint(0f, 0f, 0f);
		Vector2 vector2 = t.TransformPoint(1f, 1f, 0f);
		vector.Scale(scale);
		vector2.Scale(scale);
		Transform parent = t.parent;
		while (parent != null && (!parent.gameObject.GetComponent<Canvas>() || !(parent.parent == null)) && (!stopAtScrollView || !(parent.gameObject.GetComponent<ScrollRect>() != null)))
		{
			RectTransform component = parent.gameObject.GetComponent<RectTransform>();
			parent = parent.parent;
			if (!(component == null))
			{
				anchorMin.x = component.anchorMin.x + anchorMin.x * (component.anchorMax.x - component.anchorMin.x);
				anchorMin.y = component.anchorMin.y + anchorMin.y * (component.anchorMax.y - component.anchorMin.y);
				anchorMax.x = component.anchorMin.x + anchorMax.x * (component.anchorMax.x - component.anchorMin.x);
				anchorMax.y = component.anchorMin.y + anchorMax.y * (component.anchorMax.y - component.anchorMin.y);
			}
		}
	}

	public void CheckForRegister(UAP_BaseElement item)
	{
		if (!m_HasStarted)
		{
			return;
		}
		bool flag = true;
		foreach (Accessible_UIElement allElement in m_AllElements)
		{
			if (allElement.m_Object == item)
			{
				flag = false;
				if (item.m_ForceStartHere && item.IsElementActive())
				{
					m_CurrentStartItem = item;
				}
				break;
			}
		}
		if (flag)
		{
			RefreshContainer();
		}
	}

	public void SetAsStartItem(UAP_BaseElement item)
	{
		foreach (Accessible_UIElement allElement in m_AllElements)
		{
			if (allElement.m_Object == item)
			{
				if (item.m_ForceStartHere && item.IsElementActive())
				{
					m_CurrentStartItem = item;
					m_RefreshNextFrame = true;
				}
				return;
			}
		}
		Register_Item(item);
	}

	public void UnRegister(UAP_BaseElement item)
	{
		foreach (Accessible_UIElement allElement in m_AllElements)
		{
			if (allElement.m_Object == item)
			{
				m_AllElements.Remove(allElement);
				UAP_AccessibilityManager.ElementRemoved(allElement);
				break;
			}
		}
	}

	public void RefreshContainer()
	{
		bool flag = false;
		if (m_CurrentStartItem != null && m_CurrentItemIndex >= 0 && m_AllElements[m_CurrentItemIndex].m_Object == m_CurrentStartItem)
		{
			flag = true;
		}
		int count = m_AllElements.Count;
		m_AllElements.Clear();
		List<UAP_BaseElement> list = new List<UAP_BaseElement>();
		list.AddRange(gameObject.GetComponentsInChildren<UAP_BaseElement>());
		foreach (UAP_BaseElement item in list)
		{
			Register_Item(item);
		}
		if (count == 0 && m_AllElements.Count > 0)
		{
			ResetToStart();
			m_ActivateContainerNextFrame = true;
		}
		else if (flag)
		{
			ResetToStart();
		}
	}

	private void ActivateContainer_Internal()
	{
		UAP_AccessibilityManager.ActivateContainer(this, activate: true);
		m_ActivateContainerNextFrame = false;
	}

	private void Register_Item(UAP_BaseElement item)
	{
		EUIElement type = item.m_Type;
		AccessibleUIGroupRoot accessibleUIGroupRoot = null;
		Transform parent = item.transform;
		accessibleUIGroupRoot = parent.gameObject.GetComponent<AccessibleUIGroupRoot>();
		while (accessibleUIGroupRoot == null && parent.parent != null)
		{
			parent = parent.parent;
			accessibleUIGroupRoot = parent.gameObject.GetComponent<AccessibleUIGroupRoot>();
		}
		if (accessibleUIGroupRoot != this)
		{
			return;
		}
		if (accessibleUIGroupRoot == null)
		{
			Debug.LogError("[Accessibility] A UI element tried to register with " + gameObject.name + " but not only am I not the right container, there seems to be no container in it's parent hierarchy.");
			return;
		}
		Accessible_UIElement accessible_UIElement = new Accessible_UIElement(item, type, m_AllElements.Count);
		int positionOrder = accessible_UIElement.m_PositionOrder;
		int secondaryOrder = accessible_UIElement.m_SecondaryOrder;
		int count = m_AllElements.Count;
		bool flag = false;
		for (int i = 0; i < count; i++)
		{
			if (positionOrder < m_AllElements[i].m_PositionOrder)
			{
				m_AllElements.Insert(i, accessible_UIElement);
				flag = true;
				break;
			}
			if (positionOrder == m_AllElements[i].m_PositionOrder && m_AllElements[i].m_SecondaryOrder - secondaryOrder > 0)
			{
				m_AllElements.Insert(i, accessible_UIElement);
				flag = true;
				break;
			}
		}
		if (!flag)
		{
			m_AllElements.Add(accessible_UIElement);
		}
		if (item.m_ForceStartHere)
		{
			m_CurrentStartItem = item;
		}
	}

	private void OnEnable()
	{
		if (m_HasStarted)
		{
			RefreshContainer();
			ResetToStart();
			if (m_AllElements.Count > 0)
			{
				m_ActivateContainerNextFrame = true;
			}
		}
	}

	private void Start()
	{
		m_HasStarted = true;
		RefreshContainer();
		ResetToStart();
		if (m_AllElements.Count > 0)
		{
			m_ActivateContainerNextFrame = true;
		}
	}

	public void ResetToStart()
	{
		m_CurrentItemIndex = 0;
		if (!(m_CurrentStartItem != null))
		{
			return;
		}
		for (int i = 0; i < m_AllElements.Count; i++)
		{
			if (m_AllElements[i].m_Object == m_CurrentStartItem)
			{
				m_CurrentItemIndex = i;
				break;
			}
		}
	}

	private void OnDisable()
	{
		UAP_AccessibilityManager.ActivateContainer(this, activate: false);
	}

	private void OnDestroy()
	{
		UAP_AccessibilityManager.ActivateContainer(this, activate: false);
	}

	public Accessible_UIElement GetCurrentElement(bool rollOverAllowed)
	{
		m_CurrentItemIndex = FindFirstActiveItemIndex(m_CurrentItemIndex, rollOverAllowed);
		if (m_CurrentItemIndex < 0 || m_AllElements.Count == 0)
		{
			return null;
		}
		return m_AllElements[m_CurrentItemIndex];
	}

	private int FindFirstActiveItemIndex(int startIndex, bool rollOverAllowed)
	{
		if (m_RefreshNextFrame)
		{
			RefreshContainer();
		}
		if (m_AllElements.Count == 0)
		{
			return -1;
		}
		if (startIndex < 0)
		{
			startIndex = 0;
		}
		if (startIndex >= m_AllElements.Count)
		{
			if (!rollOverAllowed)
			{
				return -1;
			}
			startIndex = 0;
		}
		for (int i = 0; i < m_AllElements.Count; i++)
		{
			int num = i + startIndex;
			if (num >= m_AllElements.Count)
			{
				if (!rollOverAllowed)
				{
					return -1;
				}
				num -= m_AllElements.Count;
			}
			if (m_AllElements[num].m_Object == null)
			{
				RefreshContainer();
				return FindFirstActiveItemIndex(startIndex, rollOverAllowed);
			}
			if (m_AllElements[num].m_Object.IsElementActive())
			{
				return num;
			}
		}
		return -1;
	}

	private int FindPreviousActiveItemIndex(int startIndex, bool rollOverAllowed)
	{
		if (m_RefreshNextFrame)
		{
			RefreshContainer();
		}
		if (m_AllElements.Count == 0)
		{
			return -1;
		}
		if (startIndex < 0)
		{
			if (!rollOverAllowed)
			{
				return -1;
			}
			startIndex = m_AllElements.Count - 1;
		}
		if (startIndex >= m_AllElements.Count)
		{
			startIndex = m_AllElements.Count - 1;
		}
		for (int num = m_AllElements.Count; num > 0; num--)
		{
			int num2 = num + startIndex - m_AllElements.Count;
			if (num2 < 0)
			{
				if (!rollOverAllowed)
				{
					return -1;
				}
				num2 += m_AllElements.Count;
			}
			if (m_AllElements[num2].m_Object == null)
			{
				RefreshContainer();
				return FindFirstActiveItemIndex(startIndex, rollOverAllowed);
			}
			if (m_AllElements[num2].m_Object.IsElementActive())
			{
				return num2;
			}
		}
		return -1;
	}

	public bool IncrementCurrentItem(bool rollOverAllowed)
	{
		int num = FindFirstActiveItemIndex(m_CurrentItemIndex + 1, rollOverAllowed);
		if (num < 0)
		{
			return false;
		}
		if (num <= m_CurrentItemIndex)
		{
			if (rollOverAllowed)
			{
				m_CurrentItemIndex = num;
			}
			return false;
		}
		m_CurrentItemIndex = num;
		return true;
	}

	public bool DecrementCurrentItem(bool rollOverAllowed)
	{
		int num = FindPreviousActiveItemIndex(m_CurrentItemIndex - 1, rollOverAllowed);
		if (num < 0)
		{
			return false;
		}
		if (num >= m_CurrentItemIndex)
		{
			if (rollOverAllowed)
			{
				m_CurrentItemIndex = num;
			}
			return false;
		}
		m_CurrentItemIndex = num;
		return true;
	}

	public bool MoveFocus2D(UAP_AccessibilityManager.ESDirection direction)
	{
		Vector2 pos = m_AllElements[m_CurrentItemIndex].m_Pos;
		int num = -1;
		float num2 = -1f;
		for (int i = 0; i < m_AllElements.Count; i++)
		{
			if (i == m_CurrentItemIndex || !m_AllElements[i].m_Object.IsElementActive())
			{
				continue;
			}
			Vector2 vector = m_AllElements[i].m_Pos - pos;
			float num3 = 1.1f;
			bool flag = false;
			switch (direction)
			{
			case UAP_AccessibilityManager.ESDirection.ELeft:
				if (vector.x < 0f && 0f - vector.x > Mathf.Abs(vector.y) * num3)
				{
					flag = true;
				}
				break;
			case UAP_AccessibilityManager.ESDirection.ERight:
				if (vector.x > 0f && vector.x > Mathf.Abs(vector.y) * num3)
				{
					flag = true;
				}
				break;
			case UAP_AccessibilityManager.ESDirection.EUp:
				if (vector.y > 0f && vector.y > Mathf.Abs(vector.x) * num3)
				{
					flag = true;
				}
				break;
			case UAP_AccessibilityManager.ESDirection.EDown:
				if (vector.y < 0f && 0f - vector.y > Mathf.Abs(vector.x) * num3)
				{
					flag = true;
				}
				break;
			}
			if (flag)
			{
				float num4 = vector.SqrMagnitude();
				if (num < 0 || num4 < num2)
				{
					num = i;
					num2 = num4;
				}
			}
		}
		if (num < 0)
		{
			return false;
		}
		m_CurrentItemIndex = num;
		return true;
	}

	private void Update()
	{
		if (m_ActivateContainerNextFrame)
		{
			if (m_DoubleCheckUIElementsPositions && m_NeedsRefreshBeforeActivation)
			{
				m_RefreshNextFrame = true;
				m_NeedsRefreshBeforeActivation = false;
			}
			else
			{
				ActivateContainer_Internal();
				m_NeedsRefreshBeforeActivation = m_DoubleCheckUIElementsPositions;
			}
		}
		if (m_RefreshNextFrame)
		{
			m_RefreshNextFrame = false;
			RefreshContainer();
		}
	}

	public void RefreshNextUpdate()
	{
		m_RefreshNextFrame = true;
	}

	public void JumpToFirst()
	{
		m_CurrentItemIndex = 0;
		FindFirstActiveItemIndex(m_CurrentItemIndex, rollOverAllowed: false);
	}

	public void JumpToLast()
	{
		m_CurrentItemIndex = m_AllElements.Count - 1;
		FindPreviousActiveItemIndex(m_CurrentItemIndex, rollOverAllowed: false);
	}

	public void SetActiveElementIndex(int index, bool rollOverAllowed)
	{
		if (index < 0 || index >= m_AllElements.Count)
		{
			Debug.LogError("[Accessibility] UI Group: Trying to set an out of bounds index for current item. Setting index to first valid and active item to prevent crash.");
			index = 0;
		}
		if (!m_AllElements[index].m_Object.IsElementActive())
		{
			Debug.LogWarning("[Accessibility] UI Group: Trying to set a current item index to an inactive element. Setting index to next valid and active item to prevent issues.");
		}
		m_CurrentItemIndex = FindFirstActiveItemIndex(index, rollOverAllowed);
	}

	public int GetCurrentElementIndex()
	{
		return m_CurrentItemIndex;
	}

	public List<Accessible_UIElement> GetElements()
	{
		return m_AllElements;
	}

	public bool SelectItem(UAP_BaseElement element, bool forceRepeatItem = false)
	{
		for (int i = 0; i < m_AllElements.Count; i++)
		{
			if (m_AllElements[i].m_Object == element)
			{
				m_CurrentItemIndex = i;
				return UAP_AccessibilityManager.MakeActiveContainer(this, forceRepeatItem);
			}
		}
		return false;
	}

	public string GetContainerName(bool useGameObjectNameIfNone = false)
	{
		if (m_ContainerName.Length > 0)
		{
			if (IsNameLocalizationKey())
			{
				return UAP_AccessibilityManager.Localize(m_ContainerName);
			}
			return m_ContainerName;
		}
		if (useGameObjectNameIfNone)
		{
			return gameObject.name;
		}
		return "";
	}

	public bool IsNameLocalizationKey()
	{
		return m_IsLocalizationKey;
	}
}
