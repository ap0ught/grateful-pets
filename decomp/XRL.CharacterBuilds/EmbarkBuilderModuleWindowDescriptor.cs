using System;
using ConsoleLib.Console;
using Qud.UI;
using UnityEngine;

namespace XRL.CharacterBuilds;

public class EmbarkBuilderModuleWindowDescriptor
{
	public string name;

	public string viewID;

	public string tile;

	public string prefab;

	public string title;

	public Type windowType;

	public IRenderable navIcon;

	public AbstractEmbarkBuilderModule module;

	public AbstractBuilderModuleWindowBase window;

	public bool enabled
	{
		get
		{
			AbstractBuilderModuleWindowBase abstractBuilderModuleWindowBase = window;
			if ((object)abstractBuilderModuleWindowBase == null || abstractBuilderModuleWindowBase.isWindowEnabled)
			{
				return module.enabled;
			}
			return false;
		}
	}

	public EmbarkBuilderModuleWindowDescriptor(AbstractEmbarkBuilderModule module)
	{
		this.module = module;
	}

	public virtual void show()
	{
		window.EnsureContentsActive();
		window.BeforeShow(this);
		module.builder.activeWindow = this;
		UIManager.showWindow(viewID);
	}

	public AbstractBuilderModuleWindowBase getWindow()
	{
		if (viewID == null)
		{
			viewID = Guid.NewGuid().ToString();
		}
		if (window == null)
		{
			if (prefab == null)
			{
				window = UIManager.getWindow(viewID) as AbstractBuilderModuleWindowBase;
			}
			else
			{
				GameObject gameObject = Resources.Load("Prefabs/" + prefab) as GameObject;
				if (gameObject != null)
				{
					gameObject.SetActive(value: false);
					window = module.builder.createWindow(viewID, windowType);
					window.name = name;
					GameObject gameObject2 = UnityEngine.Object.Instantiate(gameObject);
					gameObject2 = window.InstantiatePrefab(gameObject2);
					gameObject2.transform.SetParent(window.transform, worldPositionStays: false);
				}
			}
		}
		if (window == null)
		{
			Debug.LogError(viewID + " not found to init module window");
		}
		return window;
	}

	public virtual void windowInit(AbstractBuilderModuleWindowBase window)
	{
		this.window = window;
		window._module = module;
		this.window.Init();
	}
}
