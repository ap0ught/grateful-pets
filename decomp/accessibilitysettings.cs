using UnityEngine;
using UnityEngine.UI;

public class accessibilitysettings : MonoBehaviour
{
	public Toggle m_EnableAccessibility;

	public Slider m_SpeechRateSlider;

	public GameObject m_AccessibilityConfirmation;

	public void OnEnable()
	{
		m_EnableAccessibility.isOn = UAP_AccessibilityManager.IsEnabled();
		m_SpeechRateSlider.value = UAP_AccessibilityManager.GetSpeechRate();
	}

	public void OnAccessibilityEnabledToggleChanged(bool newValue)
	{
		if (UAP_AccessibilityManager.IsEnabled() != newValue)
		{
			if (newValue)
			{
				m_AccessibilityConfirmation.SetActive(value: true);
			}
			else
			{
				UAP_AccessibilityManager.EnableAccessibility(enable: false, readNotification: true);
			}
		}
	}

	public void OnSpeechRateSliderChanged()
	{
		UAP_AccessibilityManager.SetSpeechRate(Mathf.RoundToInt(m_SpeechRateSlider.value));
	}

	public void OnEnableCancel()
	{
		m_EnableAccessibility.isOn = false;
		m_AccessibilityConfirmation.SetActive(value: false);
	}

	public void OnEnableConfirm()
	{
		UAP_AccessibilityManager.EnableAccessibility(enable: true, readNotification: true);
		m_AccessibilityConfirmation.SetActive(value: false);
	}

	public void OnCloseSettings()
	{
		Object.DestroyImmediate(gameObject);
	}
}
