using UnityEngine;

[AddComponentMenu("Accessibility/UI/Accessible Button 3D")]
public class AccessibleButton_3D : UAP_BaseElement_3D
{
	private AccessibleButton_3D()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.EButton;
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
		return true;
	}
}
