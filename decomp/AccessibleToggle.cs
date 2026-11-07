using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Toggle")]
public class AccessibleToggle : UAP_BaseElement
{
	public bool m_UseCustomOnOff;

	public string m_CustomOn = "Checked";

	public string m_CustomOff = "Not Checked";

	public bool m_CustomHintsAreLocalizationKeys;

	public AudioClip m_CustomOnAudio;

	public AudioClip m_CustomOffAudio;

	public AccessibleToggle()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EToggle;
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

	public override bool IsInteractable()
	{
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			if (!toggle.enabled || !toggle.IsInteractable())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	protected override void OnInteract()
	{
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			toggle.OnPointerClick(eventData);
		}
	}

	protected Toggle GetToggle()
	{
		Toggle toggle = null;
		if (m_ReferenceElement != null)
		{
			toggle = m_ReferenceElement.GetComponent<Toggle>();
		}
		if (toggle == null)
		{
			toggle = GetComponent<Toggle>();
		}
		return toggle;
	}

	public override string GetCurrentValueAsText()
	{
		if (IsChecked())
		{
			if (m_UseCustomOnOff)
			{
				if (m_CustomHintsAreLocalizationKeys)
				{
					return UAP_AccessibilityManager.Localize(m_CustomOn);
				}
				return m_CustomOn;
			}
			return UAP_AccessibilityManager.Localize_Internal("Checkbox_Checked");
		}
		if (m_UseCustomOnOff)
		{
			if (m_CustomHintsAreLocalizationKeys)
			{
				return UAP_AccessibilityManager.Localize(m_CustomOff);
			}
			return m_CustomOff;
		}
		return UAP_AccessibilityManager.Localize_Internal("Checkbox_NotChecked");
	}

	public bool IsChecked()
	{
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			return toggle.isOn;
		}
		return false;
	}

	public void SetToggleState(bool toggleState)
	{
		if (toggleState != IsChecked())
		{
			Toggle toggle = GetToggle();
			if (toggle != null)
			{
				toggle.isOn = toggleState;
			}
		}
	}

	public override AudioClip GetCurrentValueAsAudio()
	{
		Toggle toggle = GetToggle();
		if (toggle == null)
		{
			return null;
		}
		if (m_UseCustomOnOff)
		{
			if (toggle.isOn)
			{
				return m_CustomOnAudio;
			}
			return m_CustomOffAudio;
		}
		return null;
	}

	public override bool AutoFillTextLabel()
	{
		if (base.AutoFillTextLabel())
		{
			return true;
		}
		bool flag = false;
		Transform transform = gameObject.transform.Find("Label");
		if (transform != null)
		{
			Text component = transform.gameObject.GetComponent<Text>();
			if (component != null)
			{
				m_Text = component.text;
				flag = true;
			}
		}
		if (!flag)
		{
			Text componentInChildren = gameObject.GetComponentInChildren<Text>();
			if (componentInChildren != null)
			{
				m_Text = componentInChildren.text;
				flag = true;
			}
		}
		if (!flag)
		{
			m_Text = gameObject.name;
		}
		return flag;
	}

	protected override void OnHoverHighlight(bool enable)
	{
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			if (enable)
			{
				toggle.OnPointerEnter(eventData);
			}
			else
			{
				toggle.OnPointerExit(eventData);
			}
		}
	}

	protected override void AutoInitialize()
	{
		if (m_TryToReadLabel)
		{
			bool flag = false;
			Transform transform = gameObject.transform.Find("Text");
			if (transform != null)
			{
				Text component = transform.gameObject.GetComponent<Text>();
				if (component != null)
				{
					m_NameLabel = component.gameObject;
					flag = true;
				}
			}
			if (!flag)
			{
				Text componentInChildren = gameObject.GetComponentInChildren<Text>();
				if (componentInChildren != null)
				{
					m_NameLabel = componentInChildren.gameObject;
					flag = true;
				}
			}
		}
		else
		{
			m_NameLabel = null;
		}
	}
}
