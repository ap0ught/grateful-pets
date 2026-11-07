using UnityEngine;
using UnityEngine.UI;

namespace QupKit;

public class PanelControl : BaseControl
{
	public PanelControl(string Name, float Width, float Height)
	{
		rootObject = new GameObject();
		rootObject.AddComponent<RectTransform>();
		rootObject.AddComponent<CanvasRenderer>();
		rootObject.AddComponent<Image>();
		rootObject.AddComponent<Mask>();
		MainColor = new Color(0f, 0f, 0f, 1f);
		rootObject.name = Name;
		base.Width = Width;
		base.Height = Height;
		base.Name = Name;
	}
}
