using System.Collections;
using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

[UIView("Stage", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "Stage", UICanvasHost = 1)]
public class StageWindow : SingletonWindowBase<StageWindow>
{
	public GameObject dock;

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public override void Show()
	{
		base.Show();
		UIManager.getWindow("PlayerStatusBar").Show();
		UIManager.getWindow("AbilityBar").Show();
		UIManager.getWindow("MessageLog").Show();
		UIManager.getWindow<NearbyItemsWindow>("NearbyItems").ShowIfEnabled();
		UIManager.getWindow<MinimapWindow>("Minimap").ShowIfEnabled();
	}

	public override void Hide()
	{
		if (GameManager.Instance.TargetZoomFactor == -2f)
		{
			canvasGroup.alpha = 0f;
		}
		else
		{
			base.Hide();
		}
		UIManager.getWindow("PlayerStatusBar").Hide();
		UIManager.getWindow("AbilityBar").Hide();
		UIManager.getWindow("MessageLog").Hide();
		UIManager.getWindow("NearbyItems").Hide();
		UIManager.getWindow("Minimap").Hide();
	}

	public static async void FadeOut(float time)
	{
		await The.UiContext;
		SingletonWindowBase<StageWindow>.instance.StartCoroutine(SingletonWindowBase<StageWindow>.instance._FadeOut(time));
	}

	public static async void FadeIn(float time)
	{
		await The.UiContext;
		SingletonWindowBase<StageWindow>.instance.StartCoroutine(SingletonWindowBase<StageWindow>.instance._FadeIn(time));
	}

	private IEnumerator _FadeOut(float time)
	{
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
		float start = time;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(1f, 0f, 1f - time / start);
			yield return null;
		}
		canvasGroup.alpha = 0f;
	}

	public IEnumerator _FadeIn(float time)
	{
		CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
		float start = time;
		while (time > 0f)
		{
			time -= Time.deltaTime;
			canvasGroup.alpha = Mathf.Lerp(0f, 1f, 1f - time / start);
			yield return null;
		}
		canvasGroup.alpha = 1f;
	}
}
