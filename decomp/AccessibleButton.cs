using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Button")]
public class AccessibleButton : UAP_BaseElement
{
	private AccessibleButton()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EButton;
	}

	protected override void OnInteract()
	{
		Button button = GetButton();
		if (button != null)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			button.OnPointerClick(eventData);
		}
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

	private Button GetButton()
	{
		Button button = null;
		if (m_ReferenceElement != null && m_ReferenceElement.activeInHierarchy)
		{
			button = m_ReferenceElement.GetComponent<Button>();
		}
		if (button == null && gameObject.activeInHierarchy)
		{
			button = gameObject.GetComponent<Button>();
		}
		return button;
	}

	public override bool IsInteractable()
	{
		Button button = GetButton();
		if (button != null)
		{
			if (!button.enabled || !button.IsInteractable())
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public override bool AutoFillTextLabel()
	{
		if (base.AutoFillTextLabel())
		{
			return true;
		}
		bool flag = false;
		Transform transform = gameObject.transform.Find("Text");
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
			Component textMeshProLabelInChildren = GetTextMeshProLabelInChildren();
			if (textMeshProLabelInChildren != null)
			{
				m_Text = GetTextFromTextMeshPro(textMeshProLabelInChildren);
				flag = true;
			}
		}
		if (!flag)
		{
			m_Text = gameObject.name;
		}
		return flag;
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
			if (!flag)
			{
				Component textMeshProLabelInChildren = GetTextMeshProLabelInChildren();
				if (textMeshProLabelInChildren != null)
				{
					m_NameLabel = textMeshProLabelInChildren.gameObject;
					flag = true;
				}
			}
		}
		else
		{
			m_NameLabel = null;
		}
	}

	protected override void OnHoverHighlight(bool enable)
	{
		Button button = GetButton();
		if (button != null)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			if (enable)
			{
				button.OnPointerEnter(eventData);
			}
			else
			{
				button.OnPointerExit(eventData);
			}
		}
	}
}
