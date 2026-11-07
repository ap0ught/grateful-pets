using System;
using UnityEngine;

namespace XRL.UI.Framework;

public class KeyMenuOption : MonoBehaviour, IFrameworkControl
{
	public UITextSkin textSkin;

	public UITextSkin prefixSkin;

	public void setData(FrameworkDataElement d)
	{
		if (d is MenuOption)
		{
			setDataMenuOption(d as MenuOption);
			return;
		}
		if (d is PrefixMenuOption)
		{
			setDataPrefixMenuOption(d as PrefixMenuOption);
			return;
		}
		throw new ArgumentException("KeyMenuOption expects MenuOption or PrefixMenuOption data");
	}

	public void Render(string prefix, string text)
	{
		textSkin.SetText(text);
		if (string.IsNullOrEmpty(prefix))
		{
			prefixSkin.gameObject.SetActive(value: false);
			return;
		}
		prefixSkin.gameObject.SetActive(value: true);
		prefixSkin.SetText(prefix);
	}

	public void setDataPrefixMenuOption(PrefixMenuOption data)
	{
		Render(data.Prefix, data.Description);
	}

	public void setDataMenuOption(MenuOption data)
	{
		string prefix = null;
		string keyDescription = data.getKeyDescription();
		if (!string.IsNullOrEmpty(keyDescription))
		{
			prefix = "[{{W|" + keyDescription + "}}]";
		}
		Render(prefix, data.Description);
		GetNavigationContext().disabled = data.disabled;
	}

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>()?.context;
	}
}
