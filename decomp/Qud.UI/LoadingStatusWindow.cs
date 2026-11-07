using System;
using System.Threading.Tasks;
using ConsoleLib.Console;
using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

public class LoadingStatusWindow : SingletonWindowBase<LoadingStatusWindow>
{
	public bool StayHidden;

	public UITextSkin StatusText;

	public bool WantsToBeSeen;

	private float CurrentAlpha;

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void SetLoadingStatus(string text, bool waitForUiUpdate = false)
	{
		Task task = SetLoadingTextAsync(text);
		if (waitForUiUpdate)
		{
			while (!task.Wait(TimeSpan.FromMilliseconds(1000.0)) && !Keyboard.Closed)
			{
			}
		}
	}

	public async Task SetLoadingTextAsync(string text)
	{
		if (text == null || StayHidden)
		{
			WantsToBeSeen = false;
			return;
		}
		WantsToBeSeen = true;
		await The.UiContext;
		StatusText.SetText(text);
		SingletonWindowBase<LoadingStatusWindow>.instance.Show();
		if (CurrentAlpha <= 0f)
		{
			StatusText.rectTransform.sizeDelta = new Vector2(StatusText.preferredWidth, 40f);
		}
		else
		{
			StatusText.rectTransform.sizeDelta = new Vector2(Math.Max(StatusText.rectTransform.sizeDelta.x, StatusText.preferredWidth), 40f);
		}
		CurrentAlpha = 1.5f;
		GetComponent<CanvasGroup>().alpha = 1f;
	}

	public void Update()
	{
		if (StayHidden)
		{
			CurrentAlpha = 0f;
		}
		else if (!WantsToBeSeen && CurrentAlpha > 0f)
		{
			CurrentAlpha = Math.Max(0f, CurrentAlpha - Time.deltaTime * 2f);
		}
		GetComponent<CanvasGroup>().alpha = Math.Min(1f, CurrentAlpha);
		if (CurrentAlpha == 0f)
		{
			Hide();
			gameObject.SetActive(value: false);
		}
	}
}
