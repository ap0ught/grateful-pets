using UnityEngine;

namespace QupKit;

public class PrefabControl : BaseControl
{
	public PrefabControl(string Name, string ID)
	{
		rootObject = PrefabManager.Create(ID);
		rootObject.name = Name;
		Width = rootObject.GetComponent<RectTransform>().sizeDelta.x;
		Height = rootObject.GetComponent<RectTransform>().sizeDelta.y;
	}
}
