using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Label")]
public class AccessibleLabel : UAP_BaseElement
{
	private AccessibleLabel()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ELabel;
	}

	public override bool IsElementActive()
	{
		if (!base.IsElementActive())
		{
			return false;
		}
		Text label = GetLabel();
		if (label != null)
		{
			if (!label.gameObject.activeInHierarchy || !label.enabled)
			{
				return false;
			}
			return true;
		}
		Component textMeshLabel = GetTextMeshLabel();
		if (textMeshLabel != null)
		{
			if (!textMeshLabel.gameObject.activeInHierarchy)
			{
				return false;
			}
			return true;
		}
		return true;
	}

	public override bool AutoFillTextLabel()
	{
		return true;
	}

	protected Component GetTextMeshLabel()
	{
		GameObject gameObject = base.gameObject;
		if (m_ReferenceElement != null)
		{
			gameObject = m_ReferenceElement;
		}
		if (m_NameLabel != null)
		{
			gameObject = m_NameLabel;
		}
		if (gameObject == null)
		{
			gameObject = base.gameObject;
		}
		return gameObject.GetComponent("TMP_Text");
	}

	protected override string GetMainText()
	{
		if (!m_TryToReadLabel)
		{
			if (IsNameLocalizationKey())
			{
				return UAP_AccessibilityManager.Localize(m_Text);
			}
			return m_Text;
		}
		Text label = GetLabel();
		if (label != null)
		{
			if (IsNameLocalizationKey())
			{
				return CombinePrefix(UAP_AccessibilityManager.Localize(label.text));
			}
			return CombinePrefix(label.text);
		}
		string textFromTextMeshPro = GetTextFromTextMeshPro(GetTextMeshLabel());
		if (!string.IsNullOrEmpty(textFromTextMeshPro))
		{
			if (IsNameLocalizationKey())
			{
				return CombinePrefix(UAP_AccessibilityManager.Localize(textFromTextMeshPro));
			}
			return CombinePrefix(textFromTextMeshPro);
		}
		if (IsNameLocalizationKey())
		{
			return UAP_AccessibilityManager.Localize(m_Text);
		}
		return m_Text;
	}

	private Text GetLabel()
	{
		Text text = null;
		if (m_ReferenceElement != null)
		{
			text = m_ReferenceElement.GetComponent<Text>();
		}
		if (m_NameLabel != null)
		{
			text = m_NameLabel.GetComponent<Text>();
		}
		if (text == null)
		{
			text = gameObject.GetComponent<Text>();
		}
		return text;
	}

	protected override void AutoInitialize()
	{
		if (m_TryToReadLabel)
		{
			Text label = GetLabel();
			if (label != null)
			{
				m_NameLabel = label.gameObject;
			}
			if (m_NameLabel == null)
			{
				Component textMeshLabel = GetTextMeshLabel();
				if (textMeshLabel != null)
				{
					m_NameLabel = textMeshLabel.gameObject;
				}
			}
		}
		else
		{
			m_NameLabel = null;
		}
	}
}
