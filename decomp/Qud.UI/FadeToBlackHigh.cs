using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL;

namespace Qud.UI;

public class FadeToBlackHigh : SingletonWindowBase<FadeToBlackHigh>
{
	public enum FadeToBlackStage
	{
		FadingOut,
		FadedOut,
		FadingIn,
		FadedIn
	}

	public bool WantsToBeSeen;

	public bool TileMode;

	public Image image;

	public static FadeToBlackStage stage;

	private bool Active;

	private long Start;

	private float Duration = 3f;

	private float From = 1f;

	private float To;

	private Color FromColor;

	private Color ToColor;

	private CanvasGroup _Group;

	private CanvasGroup Group => _Group ?? (_Group = GetComponent<CanvasGroup>());

	public static void FadeOut(float Duration, bool FadingUIManager = false)
	{
		FadeOut(Duration, The.Color.DarkBlack);
	}

	public static void FadeNow(float Duration, float? From = null, float? To = null, Color? FromColor = null, Color? ToColor = null)
	{
		CanvasGroup canvasGroup = SingletonWindowBase<FadeToBlackHigh>.instance.Group;
		float num = From ?? canvasGroup.alpha;
		float num2 = To ?? canvasGroup.alpha;
		SingletonWindowBase<FadeToBlackHigh>.instance.FromColor = FromColor ?? SingletonWindowBase<FadeToBlackHigh>.instance.image.color;
		SingletonWindowBase<FadeToBlackHigh>.instance.ToColor = ToColor ?? SingletonWindowBase<FadeToBlackHigh>.instance.FromColor;
		SingletonWindowBase<FadeToBlackHigh>.instance.image.color = SingletonWindowBase<FadeToBlackHigh>.instance.FromColor;
		SingletonWindowBase<FadeToBlackHigh>.instance.WantsToBeSeen = num2 > 0f;
		SingletonWindowBase<FadeToBlackHigh>.instance.Start = WindowBase.gameTimeMS;
		SingletonWindowBase<FadeToBlackHigh>.instance.Duration = Duration;
		SingletonWindowBase<FadeToBlackHigh>.instance.Active = true;
		SingletonWindowBase<FadeToBlackHigh>.instance.From = num;
		SingletonWindowBase<FadeToBlackHigh>.instance.To = num2;
		canvasGroup.alpha = num;
		stage = ((!(num2 > num)) ? FadeToBlackStage.FadingIn : FadeToBlackStage.FadingOut);
		SingletonWindowBase<FadeToBlackHigh>.instance.Show();
	}

	public static void Fade(float Duration, float? From = null, float? To = null, Color? FromColor = null, Color? ToColor = null)
	{
		WindowBase.queueUIAction(() =>
		{
			CanvasGroup canvasGroup = SingletonWindowBase<FadeToBlackHigh>.instance.Group;
			float num = From ?? canvasGroup.alpha;
			float num2 = To ?? canvasGroup.alpha;
			SingletonWindowBase<FadeToBlackHigh>.instance.FromColor = FromColor ?? SingletonWindowBase<FadeToBlackHigh>.instance.image.color;
			SingletonWindowBase<FadeToBlackHigh>.instance.ToColor = ToColor ?? SingletonWindowBase<FadeToBlackHigh>.instance.FromColor;
			SingletonWindowBase<FadeToBlackHigh>.instance.image.color = SingletonWindowBase<FadeToBlackHigh>.instance.FromColor;
			SingletonWindowBase<FadeToBlackHigh>.instance.WantsToBeSeen = num2 > 0f;
			SingletonWindowBase<FadeToBlackHigh>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlackHigh>.instance.Duration = Duration;
			SingletonWindowBase<FadeToBlackHigh>.instance.Active = true;
			SingletonWindowBase<FadeToBlackHigh>.instance.From = num;
			SingletonWindowBase<FadeToBlackHigh>.instance.To = num2;
			canvasGroup.alpha = num;
			stage = ((!(num2 > num)) ? FadeToBlackStage.FadingIn : FadeToBlackStage.FadingOut);
			SingletonWindowBase<FadeToBlackHigh>.instance.Show();
		});
	}

	public static void FadeOut(float Duration, Color Color)
	{
		stage = FadeToBlackStage.FadingOut;
		WindowBase.queueUIAction(() =>
		{
			stage = FadeToBlackStage.FadingOut;
			SingletonWindowBase<FadeToBlackHigh>.instance.ToColor = (SingletonWindowBase<FadeToBlackHigh>.instance.FromColor = Color);
			SingletonWindowBase<FadeToBlackHigh>.instance.image.color = Color;
			SingletonWindowBase<FadeToBlackHigh>.instance.WantsToBeSeen = true;
			SingletonWindowBase<FadeToBlackHigh>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlackHigh>.instance.Duration = Duration;
			SingletonWindowBase<FadeToBlackHigh>.instance.Active = true;
			SingletonWindowBase<FadeToBlackHigh>.instance.From = 0f;
			SingletonWindowBase<FadeToBlackHigh>.instance.To = 1f;
			SingletonWindowBase<FadeToBlackHigh>.instance.GetComponent<CanvasGroup>().alpha = 0f;
			SingletonWindowBase<FadeToBlackHigh>.instance.Show();
		});
	}

	public static void SetTileMode()
	{
		if (!SingletonWindowBase<FadeToBlackHigh>.instance.TileMode)
		{
			SingletonWindowBase<FadeToBlackHigh>.instance.TileMode = true;
			WindowBase.queueUIAction(() =>
			{
				RectTransform component = SingletonWindowBase<FadeToBlackHigh>.instance.GetComponent<RectTransform>();
				component.SetParent(GameManager.Instance.TileRoot.transform);
				component.sizeDelta = new Vector2(1280f, 600f);
				component.localPosition = Vector3.zero;
			});
		}
	}

	public static void SetUIMode()
	{
		if (SingletonWindowBase<FadeToBlackHigh>.instance.TileMode)
		{
			SingletonWindowBase<FadeToBlackHigh>.instance.TileMode = false;
			WindowBase.queueUIAction(() =>
			{
				RectTransform component = SingletonWindowBase<FadeToBlackHigh>.instance.GetComponent<RectTransform>();
				component.SetParent(UIManager.instance.transform);
				component.offsetMax = Vector2.zero;
				component.offsetMin = Vector2.zero;
			});
		}
	}

	public static void FadeIn(float duration)
	{
		FadeIn(duration, ConsoleLib.Console.ColorUtility.ColorMap['k']);
	}

	public static void FadeIn(float duration, Color color)
	{
		stage = FadeToBlackStage.FadingIn;
		WindowBase.queueUIAction(() =>
		{
			stage = FadeToBlackStage.FadingIn;
			SingletonWindowBase<FadeToBlackHigh>.instance.ToColor = (SingletonWindowBase<FadeToBlackHigh>.instance.FromColor = color);
			SingletonWindowBase<FadeToBlackHigh>.instance.image.color = color;
			SingletonWindowBase<FadeToBlackHigh>.instance.WantsToBeSeen = false;
			SingletonWindowBase<FadeToBlackHigh>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlackHigh>.instance.Duration = duration;
			SingletonWindowBase<FadeToBlackHigh>.instance.Active = true;
			SingletonWindowBase<FadeToBlackHigh>.instance.From = 1f;
			SingletonWindowBase<FadeToBlackHigh>.instance.To = 0f;
			SingletonWindowBase<FadeToBlackHigh>.instance.GetComponent<CanvasGroup>().alpha = 1f;
			SingletonWindowBase<FadeToBlackHigh>.instance.Show();
		});
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public override void Init()
	{
		base.Init();
		stage = FadeToBlackStage.FadedIn;
		Group.alpha = 0f;
	}

	public override void _HideWithoutLeave()
	{
		if (!WantsToBeSeen && !Active)
		{
			base._HideWithoutLeave();
		}
	}

	public void Update()
	{
		if (!Active)
		{
			return;
		}
		CanvasGroup canvasGroup = Group;
		float num = (float)(WindowBase.gameTimeMS - Start) / 1000f / Duration;
		canvasGroup.alpha = Mathf.Lerp(From, To, num);
		image.color = Color.Lerp(FromColor, ToColor, num);
		if (num >= 1f)
		{
			stage = ((!(From > To)) ? FadeToBlackStage.FadedOut : FadeToBlackStage.FadedIn);
			Active = false;
			if (!WantsToBeSeen)
			{
				Hide();
			}
		}
	}
}
