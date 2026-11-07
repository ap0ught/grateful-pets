using ModelShark;
using UnityEngine;
using UnityEngine.UI;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

[ExecuteInEditMode]
public class OptionsRow : MonoBehaviour, IFrameworkControl, IFrameworkControlSubcontexts
{
	public Image background;

	public NavigationContext proxyContext;

	public FrameworkContext frameworkContext;

	public OptionsDataRow data;

	public TooltipTrigger TooltipTrigger;

	public IFrameworkControl CurrentControl;

	public bool selectedMode;

	public VerticalLayoutGroup layoutGroup;

	public LayoutElement layoutElement;

	private bool? wasSelected;

	public void SetupContexts(ScrollChildContext scontext)
	{
		frameworkContext.context = scontext;
		if (CurrentControl is IFrameworkControlSubcontexts frameworkControlSubcontexts)
		{
			frameworkControlSubcontexts.SetupContexts(scontext);
		}
		else if (scontext != null)
		{
			scontext.proxyTo = proxyContext ?? (proxyContext = new NavigationContext());
			proxyContext.parentContext = scontext;
		}
	}

	public NavigationContext GetNavigationContext()
	{
		return frameworkContext.context;
	}

	public void setData(FrameworkDataElement newData)
	{
		CurrentControl?.setData(null);
		data = newData as OptionsDataRow;
		if (string.IsNullOrEmpty(data?.HelpText))
		{
			TooltipTrigger.enabled = false;
		}
		else
		{
			TooltipTrigger.enabled = true;
			TooltipTrigger.SetText("BodyText", RTF.FormatToRTF(data.HelpText));
		}
		TooltipTrigger.maxTextWidth = (int)(base.transform as RectTransform).rect.width;
		bool flag = false;
		string text = newData?.GetType().Name;
		foreach (RectTransform item in base.transform)
		{
			if (item.name == text)
			{
				item.gameObject.SetActive(value: true);
				CurrentControl = item.GetComponent<IFrameworkControl>();
				CurrentControl?.setData(newData);
				flag = true;
				switch (text)
				{
				case "OptionsCategoryRow":
					layoutGroup.enabled = false;
					layoutElement.minHeight = 20f;
					break;
				case "OptionsSliderRow":
					layoutGroup.enabled = false;
					layoutElement.minHeight = 50f;
					break;
				case "OptionsCheckboxRow":
					if (data.Title != null && data.Title.Length >= 120)
					{
						layoutGroup.enabled = true;
						layoutElement.minHeight = 40f;
					}
					else if (data.Title != null && data.Title.Length >= 67)
					{
						layoutGroup.enabled = false;
						layoutElement.minHeight = 40f;
					}
					else
					{
						layoutGroup.enabled = false;
						layoutElement.minHeight = 20f;
					}
					break;
				case "OptionsMenubuttonRow":
					layoutGroup.enabled = false;
					layoutElement.minHeight = 20f;
					break;
				default:
					layoutGroup.enabled = true;
					layoutElement.minHeight = -1f;
					break;
				}
			}
			else if (item.name != "UISelectionBackground" && item.name != "SelectionCaret")
			{
				item.gameObject.SetActive(value: false);
			}
		}
		if (newData != null && !flag)
		{
			Transform transform = base.transform.Find("Unknown");
			transform.gameObject.SetActive(value: true);
			transform.GetComponent<UITextSkin>().SetText("Unhandled " + newData?.GetType().Name + ":" + newData.Id);
			layoutGroup.enabled = false;
			layoutElement.minHeight = 20f;
		}
	}

	public void Update()
	{
		bool? flag = GetNavigationContext()?.IsActive();
		if (wasSelected == flag)
		{
			return;
		}
		wasSelected = flag;
		selectedMode = flag == true;
		if (selectedMode)
		{
			if (background.color.a != 0.2470588f)
			{
				background.color = new Color(background.color.r, background.color.g, background.color.b, 0.2470588f);
			}
		}
		else if (background.color.a != 0f)
		{
			background.color = new Color(background.color.r, background.color.g, background.color.b, 0f);
		}
	}
}
