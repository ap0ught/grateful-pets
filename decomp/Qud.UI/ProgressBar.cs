using UnityEngine;
using UnityEngine.UI;
using XRL.UI;

namespace Qud.UI;

public class ProgressBar : MonoBehaviour
{
	public UITextSkin Number;

	public Slider Bar;

	public void Show()
	{
		gameObject.SetActive(value: true);
	}

	public void Hide()
	{
		gameObject.SetActive(value: false);
	}

	public void Set(int Value, int MaxValue)
	{
		Value = Mathf.Min(Value, MaxValue);
		Number.SetText(Value + " / " + MaxValue);
		Bar.maxValue = MaxValue;
		Bar.value = Value;
		Show();
	}

	public void Set(AchievementInfo Achievement)
	{
		if (Achievement?.Progress == null)
		{
			Hide();
		}
		else
		{
			Set(Achievement.Progress.Value, Achievement.AchievedAt);
		}
	}
}
