using Qud.UI;
using UnityEngine;

namespace XRL.UI;

public class TitledIconButton : MonoBehaviour
{
	public UITextSkin TitleText;

	public string Title;

	public ImageTinyFrame ImageTinyFrame;

	public void SetTitle(string title)
	{
		TitleText?.SetText(Title = title);
	}

	public void Update()
	{
		if (TitleText != null && TitleText.text != Title)
		{
			TitleText.SetText(Title);
		}
	}
}
