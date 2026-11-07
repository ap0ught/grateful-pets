using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

public abstract class UAP_BaseElement : MonoBehaviour
{
	[Serializable]
	public class UAPBoolCallback : UnityEvent<bool>
	{
	}

	public enum EHighlightSource
	{
		Internal,
		UserInput,
		TouchExplore
	}

	[Serializable]
	public class UAPHighlightCallback : UnityEvent<bool, EHighlightSource>
	{
	}

	public UnityEvent m_OnInteractionStart = new UnityEvent();

	public UnityEvent m_OnInteractionEnd = new UnityEvent();

	public UnityEvent m_OnInteractionAbort = new UnityEvent();

	public bool m_ForceStartHere;

	public int m_ManualPositionOrder = -1;

	public GameObject m_ManualPositionParent;

	public bool m_UseTargetForOutline;

	public int m_PositionOrder;

	public int m_SecondaryOrder;

	public Vector2 m_Pos = new Vector2(0f, 0f);

	[Header("Element Name")]
	public AudioClip m_TextAsAudio;

	public string m_Prefix = "";

	public bool m_PrefixIsLocalizationKey;

	public bool m_PrefixIsPostFix;

	public bool m_FilterText = true;

	public string m_Text = "";

	public GameObject m_NameLabel;

	public List<GameObject> m_AdditionalNameLabels;

	public GameObject[] m_TestList;

	[FormerlySerializedAs("m_IsNGUILocalizationKey")]
	public bool m_IsLocalizationKey;

	public bool m_TryToReadLabel = true;

	public GameObject m_ReferenceElement;

	public bool m_AllowVoiceOver = true;

	public bool m_ReadType = true;

	[HideInInspector]
	public bool m_WasJustAdded = true;

	[HideInInspector]
	public AccessibleUIGroupRoot.EUIElement m_Type;

	private AccessibleUIGroupRoot AUIContainer;

	public bool m_CustomHint;

	public AudioClip m_HintAsAudio;

	public string m_Hint = "";

	public bool m_HintIsLocalizationKey;

	[HideInInspector]
	public bool m_IsInsideScrollView;

	private bool m_HasStarted;

	[HideInInspector]
	public bool m_IsInitialized;

	public UAPHighlightCallback m_CallbackOnHighlight = new UAPHighlightCallback();

	private void Reset()
	{
		AutoFillTextLabel();
		m_IsInitialized = false;
		Initialize();
	}

	public void Initialize()
	{
		if (!m_IsInitialized)
		{
			AutoInitialize();
			if (!m_IsLocalizationKey)
			{
				m_Text = GetMainText();
			}
			m_TryToReadLabel = m_NameLabel != null;
			m_IsInitialized = true;
		}
	}

	protected virtual void AutoInitialize()
	{
		_ = m_IsInitialized;
	}

	private void OnEnable()
	{
		if (m_HasStarted)
		{
			RegisterWithContainer();
			CancelInvoke("RefreshContainerNextFrame");
			Invoke("RefreshContainerNextFrame", 0.5f);
		}
	}

	private void Start()
	{
		m_HasStarted = true;
		RegisterWithContainer();
		CancelInvoke("RefreshContainerNextFrame");
		Invoke("RefreshContainerNextFrame", 0.5f);
	}

	private void GetContainer()
	{
		Transform parent = transform;
		while (parent != null && AUIContainer == null)
		{
			AUIContainer = parent.gameObject.GetComponent<AccessibleUIGroupRoot>();
			parent = parent.parent;
		}
	}

	internal AccessibleUIGroupRoot GetUIGroupContainer()
	{
		if (AUIContainer == null)
		{
			GetContainer();
		}
		return AUIContainer;
	}

	private void RegisterWithContainer()
	{
		GetContainer();
		if (AUIContainer == null)
		{
			LogErrorNoValidParent();
			return;
		}
		AUIContainer.CheckForRegister(this);
		UAP_SelectionGroup[] componentsInParent = GetComponentsInParent<UAP_SelectionGroup>();
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			componentsInParent[i].AddElement(this);
		}
	}

	public void SetAsStartItem()
	{
		m_ForceStartHere = true;
		GetContainer();
		if (AUIContainer == null)
		{
			LogErrorNoValidParent();
		}
		else
		{
			AUIContainer.SetAsStartItem(this);
		}
	}

	private void RefreshContainerNextFrame()
	{
		GetContainer();
		if (AUIContainer == null)
		{
			LogErrorNoValidParent();
		}
		else
		{
			AUIContainer.RefreshNextUpdate();
		}
	}

	private void LogErrorNoValidParent()
	{
		string text = gameObject.name;
		Transform parent = gameObject.transform.parent;
		while (parent != null)
		{
			text = parent.name + "/" + text;
			parent = parent.parent;
		}
		Debug.LogError("[Accessibility] Could not find an Accessibility UI Container in any parent object of " + gameObject.name + "! This UI element will be unaccessible. Full Path: " + text);
	}

	public virtual bool Is3DElement()
	{
		return false;
	}

	public virtual bool AutoFillTextLabel()
	{
		bool flag = false;
		if (m_NameLabel != null)
		{
			Text component = m_NameLabel.GetComponent<Text>();
			if (component != null)
			{
				m_Text = component.text;
				flag = true;
			}
			if (!flag)
			{
				Component component2 = m_NameLabel.GetComponent("TMP_Text");
				if (component2 != null)
				{
					m_Text = GetTextFromTextMeshPro(component2);
					flag = true;
				}
			}
		}
		if (!flag)
		{
			m_Text = gameObject.name;
		}
		return flag;
	}

	private void OnDestroy()
	{
		if (AUIContainer != null)
		{
			AUIContainer.UnRegister(this);
		}
		UAP_SelectionGroup[] componentsInParent = GetComponentsInParent<UAP_SelectionGroup>();
		for (int i = 0; i < componentsInParent.Length; i++)
		{
			componentsInParent[i].RemoveElement(this);
		}
	}

	public virtual bool IsInteractable()
	{
		return false;
	}

	public void Interact()
	{
		m_OnInteractionStart.Invoke();
		OnInteract();
	}

	protected virtual void OnInteract()
	{
	}

	public void InteractEnd()
	{
		m_OnInteractionEnd.Invoke();
		OnInteractEnd();
	}

	protected virtual void OnInteractEnd()
	{
	}

	public void InteractAbort()
	{
		m_OnInteractionAbort.Invoke();
		OnInteractAbort();
	}

	protected virtual void OnInteractAbort()
	{
		OnInteractEnd();
	}

	protected string CombinePrefix(string text)
	{
		string text2 = (m_PrefixIsLocalizationKey ? UAP_AccessibilityManager.Localize(m_Prefix) : m_Prefix);
		if (text2.Length == 0)
		{
			return text;
		}
		if (text2.IndexOf("{0}") != -1)
		{
			string text3 = text2;
			text3 = text3.Replace("{0}", text);
			if (m_AdditionalNameLabels != null && m_AdditionalNameLabels.Count > 0)
			{
				for (int i = 0; i < m_AdditionalNameLabels.Count; i++)
				{
					if (text3.IndexOf("{" + (i + 1).ToString("0") + "}") != -1)
					{
						text3 = text3.Replace("{" + (i + 1).ToString("0") + "}", GetLabelText(m_AdditionalNameLabels[i]));
					}
				}
			}
			return text3;
		}
		if (m_PrefixIsPostFix)
		{
			return text + " " + text2;
		}
		return text2 + " " + text;
	}

	public static string FilterText(string text)
	{
		RemoveSubsting(ref text, "[-]");
		RemoveSubsting(ref text, "<b>");
		RemoveSubsting(ref text, "</b>");
		RemoveSubsting(ref text, "<B>");
		RemoveSubsting(ref text, "</B>");
		RemoveSubsting(ref text, "<u>");
		RemoveSubsting(ref text, "</u>");
		RemoveSubsting(ref text, "<U>");
		RemoveSubsting(ref text, "</U>");
		RemoveSubsting(ref text, "<i>");
		RemoveSubsting(ref text, "</i>");
		RemoveSubsting(ref text, "<I>");
		RemoveSubsting(ref text, "</I>");
		int startIndex = 0;
		startIndex = text.IndexOf('[', startIndex);
		while (startIndex > -1 && text.Length > startIndex + 7)
		{
			if (text[startIndex + 7] == ']')
			{
				text = text.Remove(startIndex, 8);
			}
			else
			{
				startIndex++;
			}
			startIndex = text.IndexOf('[', startIndex);
		}
		return text;
	}

	private static void RemoveSubsting(ref string text, string substring)
	{
		for (int num = text.LastIndexOf(substring); num >= 0; num = text.LastIndexOf(substring))
		{
			text = text.Replace(substring, "");
		}
	}

	public string GetTextToRead()
	{
		string text = GetMainText();
		if (m_FilterText)
		{
			text = FilterText(text);
		}
		return text;
	}

	protected virtual string GetMainText()
	{
		if (m_TryToReadLabel)
		{
			AutoFillTextLabel();
		}
		if (IsNameLocalizationKey())
		{
			return CombinePrefix(UAP_AccessibilityManager.Localize(m_Text));
		}
		if (m_TryToReadLabel)
		{
			return CombinePrefix(m_Text);
		}
		return m_Text;
	}

	public void SetCustomText(string itemText)
	{
		m_TryToReadLabel = false;
		m_Text = itemText;
		m_IsLocalizationKey = false;
	}

	public virtual string GetCurrentValueAsText()
	{
		return "";
	}

	public virtual AudioClip GetCurrentValueAsAudio()
	{
		return null;
	}

	public virtual bool IsElementActive()
	{
		if (!enabled)
		{
			return false;
		}
		if (!gameObject.activeInHierarchy)
		{
			return false;
		}
		return true;
	}

	public bool SelectItem(bool forceRepeatItem = false)
	{
		if (!IsElementActive())
		{
			if (UAP_AccessibilityManager.IsEnabled())
			{
				Debug.LogWarning("[Accessibility] Trying to select element '" + GetMainText() + "' (" + gameObject.name + ") but the element is not active/interactable/visible.");
			}
			return false;
		}
		_ = m_HasStarted;
		return SelectItem_Internal(forceRepeatItem);
	}

	private bool SelectItem_Internal(bool forceRepeatItem)
	{
		if (AUIContainer == null)
		{
			RegisterWithContainer();
			if (AUIContainer == null)
			{
				Debug.LogWarning("[Accessibility] SelectItem: " + gameObject.name + " is not placed within an Accessibility UI container. Can't be selected. Aborting.");
				return false;
			}
		}
		return AUIContainer.SelectItem(this, forceRepeatItem);
	}

	public virtual bool Increment()
	{
		return false;
	}

	public virtual bool Decrement()
	{
		return false;
	}

	public virtual void HoverHighlight(bool enable, EHighlightSource selectionSource)
	{
		EventTrigger eventTrigger = null;
		if (m_ReferenceElement != null && m_ReferenceElement.activeInHierarchy)
		{
			eventTrigger = m_ReferenceElement.GetComponent<EventTrigger>();
		}
		if (eventTrigger == null && gameObject.activeInHierarchy)
		{
			eventTrigger = gameObject.GetComponent<EventTrigger>();
		}
		if (eventTrigger != null)
		{
			if (enable)
			{
				eventTrigger.OnSelect(new BaseEventData(EventSystem.current)
				{
					selectedObject = eventTrigger.gameObject
				});
			}
			else
			{
				eventTrigger.OnDeselect(new BaseEventData(EventSystem.current)
				{
					selectedObject = eventTrigger.gameObject
				});
			}
		}
		OnHoverHighlight(enable);
		m_CallbackOnHighlight.Invoke(enable, selectionSource);
	}

	protected virtual void OnHoverHighlight(bool enable)
	{
	}

	public GameObject GetTargetGameObject()
	{
		if (m_ReferenceElement != null)
		{
			return m_ReferenceElement.gameObject;
		}
		return gameObject;
	}

	public bool IsNameLocalizationKey()
	{
		return m_IsLocalizationKey;
	}

	public string GetCustomHint()
	{
		if (m_HintIsLocalizationKey)
		{
			return UAP_AccessibilityManager.Localize(m_Hint);
		}
		return m_Hint;
	}

	public void SetCustomHintText(string hintText, bool isLocalizationKey = false)
	{
		m_CustomHint = true;
		m_Hint = hintText;
		m_HintIsLocalizationKey = isLocalizationKey;
	}

	public void ResetHintText()
	{
		m_CustomHint = false;
	}

	protected string GetTextFromTextMeshPro(Component textMeshProLabel)
	{
		if (textMeshProLabel == null)
		{
			return null;
		}
		string result = null;
		PropertyInfo property = textMeshProLabel.GetType().GetProperty("text");
		if (property != null)
		{
			result = property.GetValue(textMeshProLabel, null) as string;
		}
		return result;
	}

	protected virtual string GetLabelText(GameObject go)
	{
		if (go == null)
		{
			return "";
		}
		Text component = go.GetComponent<Text>();
		if (component != null)
		{
			return component.text;
		}
		string textFromTextMeshPro = GetTextFromTextMeshPro(go.GetComponent("TMP_Text"));
		if (!string.IsNullOrEmpty(textFromTextMeshPro))
		{
			return textFromTextMeshPro;
		}
		return "";
	}

	protected Component GetTextMeshProLabelInChildren()
	{
		foreach (Transform item in transform)
		{
			Component component = item.gameObject.GetComponent("TMP_Text");
			if (component != null)
			{
				return component;
			}
		}
		return null;
	}
}
