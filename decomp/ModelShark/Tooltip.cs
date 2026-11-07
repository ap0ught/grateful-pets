using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ModelShark;

public class Tooltip
{
	public interface SetupHelper
	{
		void BeforeShow(TooltipTrigger trigger, Tooltip tooltip);
	}

	public RectTransform RectTransform { get; set; }

	public TooltipStyle TooltipStyle { get; set; }

	public GameObject GameObject { get; set; }

	public List<TextField> TextFields { get; set; }

	public List<TMPField> TMPFields { get; set; }

	public List<ImageField> ImageFields { get; set; }

	public List<SectionField> SectionFields { get; set; }

	public Image BackgroundImage { get; set; }

	public CanvasRenderer[] CanvasRenderers { get; set; }

	public Graphic[] Graphics { get; set; }

	public bool StaysOpen { get; set; }

	public bool NeverRotate { get; set; }

	public bool IsBlocking { get; set; }

	public static string Delimiter { get; set; }

	public List<SetupHelper> SetupHelpers { get; set; }

	public void Initialize()
	{
		if (string.IsNullOrEmpty(Delimiter))
		{
			Delimiter = TooltipManager.Instance.textFieldDelimiter;
		}
		RectTransform = GameObject.GetComponent<RectTransform>();
		TooltipStyle = GameObject.GetComponent<TooltipStyle>();
		BackgroundImage = GameObject.GetComponent<Image>();
		CanvasRenderers = GameObject.GetComponentsInChildren<CanvasRenderer>(includeInactive: true);
		Graphics = GameObject.GetComponentsInChildren<Graphic>(includeInactive: true);
		Text[] componentsInChildren = GameObject.GetComponentsInChildren<Text>(includeInactive: true);
		TextFields = new List<TextField>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			if (componentsInChildren[i].text.Contains(Delimiter))
			{
				TextFields.Add(new TextField
				{
					Text = componentsInChildren[i],
					Original = componentsInChildren[i].text
				});
			}
		}
		TextMeshProUGUI[] componentsInChildren2 = GameObject.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
		TMPFields = new List<TMPField>();
		for (int j = 0; j < componentsInChildren2.Length; j++)
		{
			if (componentsInChildren2[j].text.Contains(Delimiter))
			{
				TMPFields.Add(new TMPField
				{
					Text = componentsInChildren2[j],
					Original = componentsInChildren2[j].text
				});
			}
		}
		List<DynamicImage> list = GameObject.GetComponentsInChildren<DynamicImage>(includeInactive: true).ToList();
		ImageFields = new List<ImageField>();
		for (int k = 0; k < list.Count; k++)
		{
			Image component = list[k].GetComponent<Image>();
			ImageFields.Add(new ImageField
			{
				Image = component,
				Name = list[k].placeholderName.Trim(Delimiter.ToCharArray()),
				Original = component.sprite
			});
		}
		List<DynamicSection> list2 = GameObject.GetComponentsInChildren<DynamicSection>(includeInactive: true).ToList();
		SectionFields = new List<SectionField>();
		for (int l = 0; l < list2.Count; l++)
		{
			GameObject gameObject = list2[l].gameObject;
			SectionFields.Add(new SectionField
			{
				GameObject = gameObject,
				Name = list2[l].placeholderName.Trim(Delimiter.ToCharArray()),
				Original = gameObject.activeSelf
			});
		}
		SetupHelpers = GameObject.GetComponentsInChildren<SetupHelper>(includeInactive: true).ToList();
	}

	public void WarmUp()
	{
		GameObject.SetActive(value: true);
		for (int i = 0; i < CanvasRenderers.Length; i++)
		{
			CanvasRenderers[i].SetAlpha(0f);
		}
	}

	public void Deactivate()
	{
		if (!(TooltipManager.Instance == null))
		{
			for (int i = 0; i < TextFields.Count; i++)
			{
				TextFields[i].Text.text = TextFields[i].Original;
			}
			for (int j = 0; j < TMPFields.Count; j++)
			{
				TMPFields[j].Text.text = TMPFields[j].Original;
			}
			for (int k = 0; k < ImageFields.Count; k++)
			{
				ImageFields[k].Image.sprite = ImageFields[k].Original;
			}
			if (TooltipManager.Instance.BlockingTooltip == this)
			{
				TooltipManager.Instance.BlockingTooltip = null;
			}
			GameObject.SetActive(value: false);
			GameObject.transform.SetParent(TooltipManager.Instance.TooltipContainer.transform, worldPositionStays: false);
		}
	}

	public void Display(float fadeDuration)
	{
		if (fadeDuration > 0f)
		{
			for (int i = 0; i < Graphics.Length; i++)
			{
				Graphics[i].CrossFadeAlpha(1f, fadeDuration, ignoreTimeScale: true);
			}
		}
		else
		{
			for (int j = 0; j < CanvasRenderers.Length; j++)
			{
				CanvasRenderers[j].SetAlpha(1f);
			}
		}
	}
}
