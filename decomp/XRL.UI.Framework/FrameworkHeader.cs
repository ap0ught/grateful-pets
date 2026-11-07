using UnityEngine;
using UnityEngine.UI;

namespace XRL.UI.Framework;

public class FrameworkHeader : MonoBehaviour, IFrameworkControl
{
	public UITextSkin textSkin;

	public Color headerColor = Color.white;

	public void setData(FrameworkDataElement d)
	{
		Image[] componentsInChildren = GetComponentsInChildren<Image>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].color = headerColor;
		}
		textSkin.color = headerColor;
		textSkin.SetText(d.Description);
	}

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>()?.context;
	}
}
