using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("Accessibility/UI/Accessible Slider")]
public class AccessibleSlider : UAP_BaseElement
{
	public bool m_ReadPercentages = true;

	public float m_Increments = 5f;

	public bool m_IncrementInPercent = true;

	public bool m_WholeNumbersOnly = true;

	private AccessibleSlider()
	{
		m_Type = AccessibleUIGroupRoot.EUIElement.ESlider;
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
		Slider slider = GetSlider();
		if (slider != null)
		{
			if (!slider.enabled || !slider.IsInteractable())
			{
				return false;
			}
			return true;
		}
		return true;
	}

	private Slider GetSlider()
	{
		Slider slider = null;
		if (m_ReferenceElement != null)
		{
			slider = m_ReferenceElement.GetComponent<Slider>();
		}
		if (slider == null)
		{
			slider = GetComponent<Slider>();
		}
		return slider;
	}

	public override string GetCurrentValueAsText()
	{
		bool flag = false;
		float num = -1f;
		Slider slider = GetSlider();
		if (slider != null)
		{
			flag = true;
			num = slider.value;
			if (m_ReadPercentages)
			{
				num = (num - slider.minValue) / (slider.maxValue - slider.minValue);
				num *= 100f;
			}
		}
		if (flag)
		{
			string text = num.ToString("0.##");
			if (m_WholeNumbersOnly || (slider != null && slider.wholeNumbers))
			{
				text = num.ToString("0");
			}
			if (m_ReadPercentages)
			{
				text += "%";
			}
			return text;
		}
		return "";
	}

	public override bool Increment()
	{
		Slider slider = GetSlider();
		if (slider != null && slider.value == slider.maxValue)
		{
			return false;
		}
		ModifySliderValue(m_Increments);
		return true;
	}

	public override bool Decrement()
	{
		Slider slider = GetSlider();
		if (slider != null && slider.value == slider.minValue)
		{
			return false;
		}
		ModifySliderValue(0f - m_Increments);
		return true;
	}

	private void ModifySliderValue(float change)
	{
		Slider slider = GetSlider();
		if (slider != null)
		{
			float num = change;
			if (m_IncrementInPercent)
			{
				num = (slider.maxValue - slider.minValue) * (num / 100f);
			}
			slider.value += num;
		}
	}

	protected override void OnHoverHighlight(bool enable)
	{
		Slider slider = GetSlider();
		if (slider != null)
		{
			PointerEventData eventData = new PointerEventData(EventSystem.current);
			if (enable)
			{
				slider.OnPointerEnter(eventData);
			}
			else
			{
				slider.OnPointerExit(eventData);
			}
		}
	}

	public override bool AutoFillTextLabel()
	{
		if (!base.AutoFillTextLabel())
		{
			m_Text = "";
		}
		return false;
	}
}
