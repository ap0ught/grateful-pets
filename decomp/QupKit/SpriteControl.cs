using UnityEngine;
using UnityEngine.UI;

namespace QupKit;

public class SpriteControl : BaseControl
{
	public SpriteControl(string Name, string Sprite, float Scale)
	{
		rootObject = new GameObject();
		rootObject.name = Name;
		rootObject.AddComponent<RectTransform>();
		rootObject.AddComponent<CanvasRenderer>();
		rootObject.AddComponent<Image>();
		MainColor = new Color(1f, 1f, 1f, 1f);
		Debug.Log(Sprite);
		base.Sprite = Sprite;
		Width = rootObject.GetComponent<Image>().preferredWidth * Scale;
		Height = rootObject.GetComponent<Image>().preferredHeight * Scale;
	}
}
