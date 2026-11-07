using UnityEngine;

namespace Qud.UI;

public class SingletonWindowBase<WindowType> : WindowBase where WindowType : class, new()
{
	public static WindowType instance;

	public GameObject FindChild(string path)
	{
		return gameObject.transform.Find(path).gameObject;
	}

	public ComponentType GetChildComponent<ComponentType>(string path) where ComponentType : class
	{
		return gameObject.transform.Find(path).GetComponent<ComponentType>();
	}

	public override void Init()
	{
		instance = this as WindowType;
		base.Init();
	}
}
