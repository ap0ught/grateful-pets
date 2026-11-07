using System;
using System.Collections.Generic;
using System.Threading;
using ConsoleLib.Console;
using Qud.API;
using UnityEngine;
using XRL;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.UI.Framework;
using XRL.UI.ObjectFinderContexts;

namespace Qud.UI;

[HasModSensitiveStaticCache]
[HasGameBasedStaticCache]
[UIView("NearbyItems", false, false, false, null, null, false, 0, false, NavCategory = "Adventure", UICanvas = "NearbyItems", UICanvasHost = 1)]
public class NearbyItemsWindow : MovableSceneFrameWindowBase<NearbyItemsWindow>
{
	public bool Dirty;

	public bool KeyboardMode;

	public float hoverTimer;

	private bool FinderInitialized;

	public FrameworkScroller scrollController;

	private List<ObjectFinderLine.Data> _objects = new List<ObjectFinderLine.Data>();

	public int currentObjectCount;

	public static ObjectFinder Finder => ObjectFinder.instance;

	public IEnumerable<ObjectFinderLine.Data> objects
	{
		get
		{
			if (currentObjectCount > _objects.Count)
			{
				MetricsManager.LogError("currentObjectCount / _objects length dsync");
				yield break;
			}
			for (int x = 0; x < currentObjectCount; x++)
			{
				yield return _objects[x];
			}
		}
	}

	[ModSensitiveCacheInit]
	public static void SystemInit()
	{
		XRLCore.RegisterOnBeginPlayerTurnCallback((XRLCore _) =>
		{
			SingletonWindowBase<NearbyItemsWindow>.instance.Dirty = true;
		});
		XRLCore.RegisterOnEndPlayerTurnCallback((XRLCore _) =>
		{
			SingletonWindowBase<NearbyItemsWindow>.instance.Dirty = true;
		}, Single: true);
	}

	[GameBasedCacheInit]
	public static void GameInit()
	{
		lock (SingletonWindowBase<NearbyItemsWindow>.instance._objects)
		{
			SingletonWindowBase<NearbyItemsWindow>.instance.currentObjectCount = 0;
			SingletonWindowBase<NearbyItemsWindow>.instance.Dirty = true;
		}
		SingletonWindowBase<NearbyItemsWindow>.instance.FinderInitialized = false;
	}

	public override bool AllowPassthroughInput()
	{
		return true;
	}

	public void StartupFinder()
	{
		if (!FinderInitialized && Finder != null)
		{
			Finder.LoadDefaults();
			Finder.ReadOptions();
			FinderInitialized = true;
		}
	}

	public void TogglePreferredState()
	{
		StartupFinder();
		Toggle();
		SaveOptions();
	}

	public void SaveOptions()
	{
		Options.SetOption("OptionOverlayNearbyObjects", Visible);
	}

	public void ShowIfEnabled()
	{
		if (Options.OverlayNearbyObjects)
		{
			Show();
			StartupFinder();
			return;
		}
		Hide();
		if (FinderInitialized)
		{
			ObjectFinder.Reset();
			FinderInitialized = false;
		}
	}

	public override void Update()
	{
		if (!KeyboardMode && !ControlManager.LastInputFromMouse && scrollController.scrollContext.IsActive())
		{
			NavigationController.instance.activeContext = null;
		}
		try
		{
			if (Options.OverlayNearbyObjects && Monitor.TryEnter(_objects))
			{
				try
				{
					if (_objects != null)
					{
						if (currentObjectCount > _objects.Count)
						{
							MetricsManager.LogError("currentObjectCount / _objects length dsync");
							return;
						}
						for (int i = 0; i < currentObjectCount; i++)
						{
							UpdateObject(_objects[i]);
						}
					}
					if (Dirty)
					{
						scrollController.BeforeShow(null, objects);
						scrollController.onHighlight.RemoveListener(OnHighlight);
						scrollController.onHighlight.AddListener(OnHighlight);
						scrollController.onSelected.RemoveListener(OnSelect);
						scrollController.onSelected.AddListener(OnSelect);
						Dirty = false;
					}
				}
				finally
				{
					Monitor.Exit(_objects);
				}
			}
		}
		catch (Exception exception)
		{
			Debug.LogException(exception);
		}
		base.Update();
	}

	public void UpdateObject(ObjectFinderLine.Data data)
	{
		if (data.context is NearbyItems)
		{
			string text = Directions.GetUITextArrowForDirection(The.PlayerCell?.GetDirectionFromCell(data.go?.CurrentCell));
			if (Options.OverlayNearbyObjectsLocal)
			{
				text = "";
			}
			if (text != data.PrefixText)
			{
				data.PrefixText = text;
				Dirty = true;
			}
		}
	}

	public void OnHighlight(FrameworkDataElement e)
	{
		if (e is ObjectFinderLine.Data data && !KeyboardMode)
		{
			ShowTooltip(data);
		}
	}

	public void OnSelect(FrameworkDataElement e)
	{
		ObjectFinderLine.Data data = e as ObjectFinderLine.Data;
		if (data != null)
		{
			GameManager.Instance.gameQueue.queueSingletonTask("nearby items twiddle", () =>
			{
				EquipmentAPI.TwiddleObject(data.go);
			});
		}
	}

	public void ShowTooltip(ObjectFinderLine.Data data)
	{
	}

	public void UpdateGameContext()
	{
		lock (_objects)
		{
			if (Finder == null || !Finder.UpdateFilter())
			{
				return;
			}
			currentObjectCount = 0;
			foreach (var item in Finder.readItems())
			{
				ObjectFinderLine.Data data;
				if (currentObjectCount < _objects.Count)
				{
					data = _objects[currentObjectCount];
				}
				else
				{
					data = new ObjectFinderLine.Data();
					_objects.Add(data);
				}
				currentObjectCount++;
				ObjectFinderLine.Data data2 = data;
				if (data2.Icon == null)
				{
					data2.Icon = new Renderable();
				}
				data.Icon.Copy(item.go.RenderForUI());
				data.go = item.go;
				data.context = item.context;
				data.Description = item.go.DisplayName;
				if (item.go.IsTakeable())
				{
					data.RightText = Markup.Color("K", item.go.Weight + "lbs.");
				}
				else
				{
					data.RightText = null;
				}
				if (data.context is AutogotItems)
				{
					data.PrefixText = "{{K|<autogot>}}";
				}
				else if (data.context is NearbyItems)
				{
					data.PrefixText = Directions.GetUITextArrowForDirection(The.PlayerCell.GetDirectionFromCell(item.go.CurrentCell));
				}
			}
			Dirty = true;
		}
	}
}
