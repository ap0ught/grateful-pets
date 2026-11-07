using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Dropdown")]
public class AccessibleDropdown : UAP_BaseElement
{
	[Header("Other")]
	public List<AudioClip> m_ValuesAsAudio = new List<AudioClip>();

	private int prevSelectedIndex = -1;

	private AccessibleDropdown()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EDropDown;
	}

	private void Awake()
	{
	}

	private Dropdown GetDropdown()
	{
		Dropdown dropdown = null;
		if (m_ReferenceElement != null)
		{
			dropdown = m_ReferenceElement.GetComponent<Dropdown>();
		}
		if (dropdown == null)
		{
			dropdown = GetComponent<Dropdown>();
		}
		return dropdown;
	}

	private Component GetTMPDropDown()
	{
		Component component = null;
		if (m_ReferenceElement != null)
		{
			component = m_ReferenceElement.GetComponent("TMP_Dropdown");
		}
		if (component == null)
		{
			component = GetComponent("TMP_Dropdown");
		}
		return component;
	}

	public override bool IsInteractable()
	{
		return IsElementActive();
	}

	public override bool IsElementActive()
	{
		if (!base.IsElementActive())
		{
			return false;
		}
		if (m_ReferenceElement != null && !m_ReferenceElement.gameObject.activeInHierarchy)
		{
			return false;
		}
		if (!UAP_AccessibilityManager.GetSpeakDisabledInteractables() && !IsInteractable())
		{
			return false;
		}
		return true;
	}

	public override string GetCurrentValueAsText()
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			return dropdown.captionText.text;
		}
		Component tMPDropDown = GetTMPDropDown();
		if (tMPDropDown != null)
		{
			PropertyInfo property = tMPDropDown.GetType().GetProperty("captionText");
			if (property != null)
			{
				Component textMeshProLabel = property.GetValue(tMPDropDown, null) as Component;
				return GetTextFromTextMeshPro(textMeshProLabel);
			}
		}
		return "";
	}

	public override AudioClip GetCurrentValueAsAudio()
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null && m_ValuesAsAudio.Count > dropdown.value)
		{
			return m_ValuesAsAudio[dropdown.value];
		}
		return null;
	}

	protected override void OnInteract()
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			prevSelectedIndex = dropdown.value;
			dropdown.Show();
		}
		Component tMPDropDown = GetTMPDropDown();
		if (tMPDropDown != null)
		{
			IPointerClickHandler component = tMPDropDown.gameObject.GetComponent<IPointerClickHandler>();
			if (component != null)
			{
				PointerEventData eventData = new PointerEventData(EventSystem.current);
				component.OnPointerClick(eventData);
			}
		}
	}

	protected override void OnInteractEnd()
	{
		prevSelectedIndex = -1;
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			dropdown.Hide();
		}
		Component tMPDropDown = GetTMPDropDown();
		if (tMPDropDown != null)
		{
			ICancelHandler component = tMPDropDown.gameObject.GetComponent<ICancelHandler>();
			if (component != null)
			{
				BaseEventData eventData = new BaseEventData(EventSystem.current);
				component.OnCancel(eventData);
			}
		}
	}

	protected override void OnInteractAbort()
	{
		if (prevSelectedIndex != -1)
		{
			Dropdown dropdown = GetDropdown();
			if (dropdown != null)
			{
				dropdown.value = prevSelectedIndex;
			}
			Component tMPDropDown = GetTMPDropDown();
			if (tMPDropDown != null)
			{
				PropertyInfo property = tMPDropDown.GetType().GetProperty("value");
				if (property != null)
				{
					property.SetValue(tMPDropDown, prevSelectedIndex, null);
				}
			}
		}
		OnInteractEnd();
	}

	public override bool Increment()
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			if (dropdown.value == dropdown.options.Count - 1)
			{
				return false;
			}
			int value = dropdown.value + 1;
			dropdown.value = value;
			return true;
		}
		Component tMPDropDown = GetTMPDropDown();
		if (tMPDropDown != null)
		{
			PropertyInfo property = tMPDropDown.GetType().GetProperty("value");
			if (property != null)
			{
				int num = (int)property.GetValue(tMPDropDown, null);
				int itemCount = GetItemCount();
				if (num == itemCount - 1)
				{
					return false;
				}
				property.SetValue(tMPDropDown, num + 1, null);
				return true;
			}
		}
		return false;
	}

	public override bool Decrement()
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			if (dropdown.value == 0)
			{
				return false;
			}
			int value = dropdown.value - 1;
			dropdown.value = value;
			return true;
		}
		Component tMPDropDown = GetTMPDropDown();
		if (tMPDropDown != null)
		{
			PropertyInfo property = tMPDropDown.GetType().GetProperty("options");
			PropertyInfo property2 = tMPDropDown.GetType().GetProperty("value");
			if (property != null && property2 != null)
			{
				int num = (int)property2.GetValue(tMPDropDown, null);
				if (num == 0)
				{
					return false;
				}
				property2.SetValue(tMPDropDown, num - 1, null);
				return true;
			}
		}
		return false;
	}

	public int GetItemCount()
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			return dropdown.options.Count;
		}
		Component tMPDropDown = GetTMPDropDown();
		if (tMPDropDown != null)
		{
			PropertyInfo property = tMPDropDown.GetType().GetProperty("options");
			if (property != null)
			{
				object value = property.GetValue(tMPDropDown, null);
				PropertyInfo property2 = value.GetType().GetProperty("Count");
				if (property2 != null)
				{
					return (int)property2.GetValue(value, null);
				}
			}
		}
		return 0;
	}

	public int GetSelectedItemIndex()
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			return dropdown.value + 1;
		}
		Component tMPDropDown = GetTMPDropDown();
		if (tMPDropDown != null)
		{
			PropertyInfo property = tMPDropDown.GetType().GetProperty("value");
			if (property != null)
			{
				return (int)property.GetValue(tMPDropDown, null) + 1;
			}
		}
		return 0;
	}

	protected override void OnHoverHighlight(bool enable)
	{
		Dropdown dropdown = GetDropdown();
		if (dropdown != null)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			if (enable)
			{
				dropdown.OnPointerEnter(eventData);
			}
			else
			{
				dropdown.OnPointerExit(eventData);
			}
		}
	}
}
