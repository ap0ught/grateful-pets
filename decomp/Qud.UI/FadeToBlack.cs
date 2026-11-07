using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.UI;
using XRL;

namespace Qud.UI;

public class FadeToBlack : SingletonWindowBase<FadeToBlack>
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
		CanvasGroup canvasGroup = SingletonWindowBase<FadeToBlack>.instance.Group;
		float num = From ?? canvasGroup.alpha;
		float num2 = To ?? canvasGroup.alpha;
		SingletonWindowBase<FadeToBlack>.instance.FromColor = FromColor ?? SingletonWindowBase<FadeToBlack>.instance.image.color;
		SingletonWindowBase<FadeToBlack>.instance.ToColor = ToColor ?? SingletonWindowBase<FadeToBlack>.instance.FromColor;
		SingletonWindowBase<FadeToBlack>.instance.image.color = SingletonWindowBase<FadeToBlack>.instance.FromColor;
		SingletonWindowBase<FadeToBlack>.instance.WantsToBeSeen = num2 > 0f;
		SingletonWindowBase<FadeToBlack>.instance.Start = WindowBase.gameTimeMS;
		SingletonWindowBase<FadeToBlack>.instance.Duration = Duration;
		SingletonWindowBase<FadeToBlack>.instance.Active = true;
		SingletonWindowBase<FadeToBlack>.instance.From = num;
		SingletonWindowBase<FadeToBlack>.instance.To = num2;
		canvasGroup.alpha = num;
		stage = ((!(num2 > num)) ? FadeToBlackStage.FadingIn : FadeToBlackStage.FadingOut);
		SingletonWindowBase<FadeToBlack>.instance.Show();
	}

	public static void Fade(float Duration, float? From = null, float? To = null, Color? FromColor = null, Color? ToColor = null)
	{
		WindowBase.queueUIAction(() =>
		{
			CanvasGroup canvasGroup = SingletonWindowBase<FadeToBlack>.instance.Group;
			float num = From ?? canvasGroup.alpha;
			float num2 = To ?? canvasGroup.alpha;
			SingletonWindowBase<FadeToBlack>.instance.FromColor = FromColor ?? SingletonWindowBase<FadeToBlack>.instance.image.color;
			SingletonWindowBase<FadeToBlack>.instance.ToColor = ToColor ?? SingletonWindowBase<FadeToBlack>.instance.FromColor;
			SingletonWindowBase<FadeToBlack>.instance.image.color = SingletonWindowBase<FadeToBlack>.instance.FromColor;
			SingletonWindowBase<FadeToBlack>.instance.WantsToBeSeen = num2 > 0f;
			SingletonWindowBase<FadeToBlack>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlack>.instance.Duration = Duration;
			SingletonWindowBase<FadeToBlack>.instance.Active = true;
			SingletonWindowBase<FadeToBlack>.instance.From = num;
			SingletonWindowBase<FadeToBlack>.instance.To = num2;
			canvasGroup.alpha = num;
			stage = ((!(num2 > num)) ? FadeToBlackStage.FadingIn : FadeToBlackStage.FadingOut);
			SingletonWindowBase<FadeToBlack>.instance.Show();
		});
	}

	public static void FadeOut(float Duration, Color Color)
	{
		stage = FadeToBlackStage.FadingOut;
		WindowBase.queueUIAction(() =>
		{
			stage = FadeToBlackStage.FadingOut;
			SingletonWindowBase<FadeToBlack>.instance.ToColor = (SingletonWindowBase<FadeToBlack>.instance.FromColor = Color);
			SingletonWindowBase<FadeToBlack>.instance.image.color = Color;
			SingletonWindowBase<FadeToBlack>.instance.WantsToBeSeen = true;
			SingletonWindowBase<FadeToBlack>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlack>.instance.Duration = Duration;
			SingletonWindowBase<FadeToBlack>.instance.Active = true;
			SingletonWindowBase<FadeToBlack>.instance.From = 0f;
			SingletonWindowBase<FadeToBlack>.instance.To = 1f;
			SingletonWindowBase<FadeToBlack>.instance.GetComponent<CanvasGroup>().alpha = 0f;
			SingletonWindowBase<FadeToBlack>.instance.Show();
		});
	}

	public static void SetTileMode()
	{
		if (!SingletonWindowBase<FadeToBlack>.instance.TileMode)
		{
			SingletonWindowBase<FadeToBlack>.instance.TileMode = true;
			WindowBase.queueUIAction(() =>
			{
				RectTransform component = SingletonWindowBase<FadeToBlack>.instance.GetComponent<RectTransform>();
				component.SetParent(GameManager.Instance.TileRoot.transform);
				component.sizeDelta = new Vector2(1280f, 600f);
				component.localPosition = Vector3.zero;
			});
		}
	}

	public static void SetUIMode()
	{
		if (SingletonWindowBase<FadeToBlack>.instance.TileMode)
		{
			SingletonWindowBase<FadeToBlack>.instance.TileMode = false;
			WindowBase.queueUIAction(() =>
			{
				RectTransform component = SingletonWindowBase<FadeToBlack>.instance.GetComponent<RectTransform>();
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
			SingletonWindowBase<FadeToBlack>.instance.ToColor = (SingletonWindowBase<FadeToBlack>.instance.FromColor = color);
			SingletonWindowBase<FadeToBlack>.instance.image.color = color;
			SingletonWindowBase<FadeToBlack>.instance.WantsToBeSeen = false;
			SingletonWindowBase<FadeToBlack>.instance.Start = WindowBase.gameTimeMS;
			SingletonWindowBase<FadeToBlack>.instance.Duration = duration;
			SingletonWindowBase<FadeToBlack>.instance.Active = true;
			SingletonWindowBase<FadeToBlack>.instance.From = 1f;
			SingletonWindowBase<FadeToBlack>.instance.To = 0f;
			SingletonWindowBase<FadeToBlack>.instance.GetComponent<CanvasGroup>().alpha = 1f;
			SingletonWindowBase<FadeToBlack>.instance.Show();
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
