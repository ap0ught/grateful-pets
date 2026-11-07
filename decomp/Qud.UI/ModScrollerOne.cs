using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XRL;
using XRL.UI;

namespace Qud.UI;

[RequireComponent(typeof(ScrollRect))]
public class ModScrollerOne : QudBaseMenuController<ModInfo, ModMenuLine>
{
	private ScrollRect _scroller;

	public bool forceEditorUpdate;

	public Vector2 normalPos;

	public List<ModInfo> mods
	{
		get
		{
			return menuData;
		}
		set
		{
			menuData = value;
		}
	}

	public void Awake()
	{
		activateHandlers.AddListener(OnActivate);
	}

	public override void Update()
	{
		if (_scroller == null || _scroller.gameObject != gameObject)
		{
			_scroller = GetComponent<ScrollRect>();
		}
		if (_scroller.content.sizeDelta.x != _scroller.viewport.rect.width)
		{
			_scroller.content.GetComponent<LayoutElement>().preferredHeight = (float)Math.Floor(_scroller.viewport.rect.height);
			_scroller.content.sizeDelta = new Vector2(_scroller.viewport.rect.width, (float)Math.Ceiling(_scroller.content.sizeDelta.y));
		}
		normalPos = _scroller.normalizedPosition;
		base.Update();
	}

	public void OnActivate(ModInfo modInfo)
	{
		if (modInfo.IsScripting && !Options.AllowCSMods)
		{
			Popup.WaitNewPopupMessage(modInfo.DisplayTitle + " contains scripts and has been permanently disabled in the options.\n{{K|(Options->Modding->Allow scripting mods)}}", PopupMessage.SingleButton);
		}
		else if (!modInfo.Settings.Enabled)
		{
			modInfo.Settings.Enabled = true;
		}
		else if (modInfo.Settings.Failed)
		{
			modInfo.ConfirmFailure();
		}
		else if (modInfo.IsMissingDependency)
		{
			modInfo.ConfirmDependencies();
		}
		else if (modInfo.HasUpdate && modInfo.RemoteVersion > (modInfo.Settings?.UpdateVersion ?? XRL.Version.Zero))
		{
			modInfo.ConfirmUpdate();
		}
		else
		{
			modInfo.Settings.Enabled = false;
		}
	}

	public override void UpdateElements(bool evenIfNotCurrent = false)
	{
		base.UpdateElements(evenIfNotCurrent);
		LayoutRebuilder.ForceRebuildLayoutImmediate(_scroller.content);
		_scroller.verticalNormalizedPosition = 1f;
	}
}
