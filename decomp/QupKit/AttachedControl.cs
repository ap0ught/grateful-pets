using UnityEngine;

namespace QupKit;

public class AttachedControl : BaseControl
{
	public AttachedControl(GameObject GO)
	{
		rootObject = GO;
		Name = GO.name;
		Width = GO.GetComponent<RectTransform>().sizeDelta.x;
		Height = GO.GetComponent<RectTransform>().sizeDelta.y;
	}
}
