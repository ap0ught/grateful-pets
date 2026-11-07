using System;
using System.Collections;
using System.Collections.Concurrent;
using Kobold;
using UnityEngine;
using XRL;
using XRL.UI;

namespace Qud.UI;

public class Notification : MonoBehaviour
{
	public struct Data
	{
		public string Title;

		public string Text;

		public string Icon;

		public string Sound;

		public int? Value;

		public int? MaxValue;

		public Color? TitleColor;

		public Color? TextColor;

		public Color? FrameColor;
	}

	private static Notification Instance;

	private static readonly ConcurrentQueue<Data> Queue = new ConcurrentQueue<Data>();

	private static readonly Action StartDelegate = () =>
	{
		GameManager.Instance.StartCoroutine(Instance.Routine());
	};

	private static bool Running;

	public ImageTinyFrame Icon;

	public UITextSkin Title;

	public UITextSkin Text;

	public ProgressBar Progress;

	public float TimeIn = 0.5f;

	public float TimeOut = 0.5f;

	public float TimeHold = 4f;

	private GameObject Object;

	private RectTransform Transform;

	private Vector2 BaseSize;

	private Vector2 TextSize;

	private Vector2 Origin;

	private Vector2 Target;

	public static void Enqueue(string Title, string Text = null, string Icon = null, string Sound = null, int? Value = null, int? MaxValue = null, Color? TitleColor = null, Color? TextColor = null, Color? FrameColor = null)
	{
		Queue.Enqueue(new Data
		{
			Title = Title,
			Text = Text,
			Sound = Sound,
			Icon = Icon,
			Value = Value,
			MaxValue = MaxValue,
			TitleColor = TitleColor,
			TextColor = TextColor,
			FrameColor = FrameColor
		});
		if (!Running)
		{
			Running = true;
			if (GameManager.IsOnUIContext())
			{
				StartDelegate();
			}
			else
			{
				GameManager.Instance.uiQueue.queueTask(StartDelegate);
			}
		}
	}

	public void Resize()
	{
		Vector2 preferredValues = Title.GetPreferredValues(TextSize.x, TextSize.y);
		Vector2 preferredValues2 = Text.GetPreferredValues(TextSize.x, TextSize.y);
		Transform.sizeDelta = new Vector2(Mathf.Clamp(BaseSize.x + (Mathf.Max(preferredValues.x, preferredValues2.x) - TextSize.x), 240f, 400f), BaseSize.y);
	}

	private IEnumerator Routine()
	{
		Object.SetActive(value: true);
		Transform.anchoredPosition = Origin;
		Data result;
		while (Queue.TryDequeue(out result))
		{
			if (result.Sound != null)
			{
				SoundManager.PlayUISound(result.Sound);
			}
			Transform.anchoredPosition = Origin;
			Title.SetText(result.Title ?? "");
			Title.color = result.TitleColor ?? The.Color.Yellow;
			Text.SetText(result.Text ?? "");
			Text.color = result.TextColor ?? The.Color.Gray;
			if (result.Value.HasValue && result.MaxValue.HasValue)
			{
				Progress.Set(result.Value.Value, result.MaxValue.Value);
			}
			else
			{
				Progress.Hide();
			}
			if (!result.Icon.IsNullOrEmpty())
			{
				Icon.gameObject.SetActive(value: true);
				Icon.sprite = SpriteManager.GetUnitySprite(result.Icon);
				Icon.borderColor = result.FrameColor ?? The.Color.Yellow;
			}
			else
			{
				Icon.gameObject.SetActive(value: false);
			}
			Resize();
			float d = 0f;
			while (d < TimeIn)
			{
				yield return null;
				d += Time.deltaTime;
				float p = d / TimeIn;
				Transform.anchoredPosition = Vector2.Lerp(Origin, Target, Easing.SineEaseOut(p));
			}
			Transform.anchoredPosition = Target;
			yield return new WaitForSeconds(TimeHold);
			d = 0f;
			while (d < TimeOut)
			{
				yield return null;
				d += Time.deltaTime;
				float p = d / TimeOut;
				Transform.anchoredPosition = Vector2.Lerp(Target, Origin, Easing.SineEaseIn(p));
			}
			Transform.anchoredPosition = Origin;
		}
		Object.SetActive(value: false);
		Running = false;
	}

	private void Start()
	{
		Object = gameObject;
		Transform = (RectTransform)Object.transform;
		Instance = this;
		BaseSize = Transform.sizeDelta;
		TextSize = Title.GetPreferredValues("HEAIAAAAAAASCREAMINGAAAAAAAAAAAUGH", 400f, 40f);
		Vector2 anchoredPosition = Transform.anchoredPosition;
		Origin = new Vector2(anchoredPosition.x, Math.Min(anchoredPosition.y, 0f - BaseSize.y));
		Target = new Vector2(Origin.x, 0f);
		Object.SetActive(value: false);
	}
}
