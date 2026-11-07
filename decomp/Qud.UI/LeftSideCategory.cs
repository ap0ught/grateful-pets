using TMPro;
using UnityEngine;
using XRL.UI;
using XRL.UI.Framework;

namespace Qud.UI;

public class LeftSideCategory : MonoBehaviour, IFrameworkControl
{
	public TextMeshProUGUI tmp;

	public UITextSkin text;

	private Media.SizeClass lastClass = Media.SizeClass.Unset;

	public NavigationContext GetNavigationContext()
	{
		return GetComponent<FrameworkContext>().context;
	}

	public void Update()
	{
		if (Media.sizeClass != lastClass)
		{
			lastClass = Media.sizeClass;
			Respond();
		}
	}

	public void Respond()
	{
		lastClass = Media.sizeClass;
		if (Media.sizeClass < Media.SizeClass.Medium)
		{
			if (tmp.fontSize != 10f)
			{
				text.style = UITextSkin.Size.unset;
				tmp.fontSize = 10f;
			}
		}
		else if (tmp.fontSize != 16f)
		{
			text.style = UITextSkin.Size.unset;
			tmp.fontSize = 16f;
		}
	}

	public void Awake()
	{
		Respond();
	}

	public void setData(FrameworkDataElement data)
	{
		if (data is KeybindCategoryRow keybindCategoryRow)
		{
			text.SetText("{{C|" + keybindCategoryRow.CategoryDescription + "}}");
		}
		if (data is MenuOption menuOption)
		{
			text.SetText("{{C|" + menuOption.getMenuText() + "}}");
		}
		if (data is HelpDataRow helpDataRow)
		{
			text.SetText("{{C|" + helpDataRow.CategoryId + "}}");
		}
		if (data is OptionsCategoryRow optionsCategoryRow)
		{
			text.SetText("{{C|" + optionsCategoryRow.CategoryId + "}}");
		}
	}
}
