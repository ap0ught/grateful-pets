using UnityEngine;

[AddComponentMenu("Accessibility/UI/Accessible Label 3D")]
public class AccessibleLabel_3D : UAP_BaseElement_3D
{
	private AccessibleLabel_3D()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ELabel;
	}

	protected override string GetMainText()
	{
		if (IsNameLocalizationKey())
		{
			return CombinePrefix(UAP_AccessibilityManager.Localize(m_Text));
		}
		return m_Text;
	}
}
