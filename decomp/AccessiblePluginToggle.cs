using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Plugin Toggle")]
public class AccessiblePluginToggle : AccessibleToggle
{
	public bool m_HandleActivation = true;

	private void OnEnable()
	{
		UAP_AccessibilityManager.RegisterAccessibilityModeChangeCallback(AccessibilitiyPlugin_StateChanged);
		UpdateToggleState();
	}

	private void OnDisable()
	{
		UAP_AccessibilityManager.UnregisterAccessibilityModeChangeCallback(AccessibilitiyPlugin_StateChanged);
	}

	private void Start()
	{
		Toggle toggle = GetToggle();
		if (toggle != null)
		{
			toggle.onValueChanged.AddListener(OnToggleStateChanged);
		}
	}

	public void OnToggleStateChanged(bool newState)
	{
		if (m_HandleActivation && newState != UAP_AccessibilityManager.IsEnabled())
		{
			UAP_AccessibilityManager.EnableAccessibility(newState);
		}
	}

	public void AccessibilitiyPlugin_StateChanged(bool newEnabledState)
	{
		UpdateToggleState();
	}

	private void UpdateToggleState()
	{
		if (UAP_AccessibilityManager.IsEnabled() != IsChecked())
		{
			SetToggleState(UAP_AccessibilityManager.IsEnabled());
		}
	}
}
