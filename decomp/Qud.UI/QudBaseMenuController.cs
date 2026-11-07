using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleLib.Console;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Qud.UI;

public abstract class QudBaseMenuController : MonoBehaviour
{
	public enum MenuEventPipeline
	{
		KeyboardOrJoystick,
		Pointer
	}

	public MenuEventPipeline lastEventSource = MenuEventPipeline.Pointer;

	public virtual void SetSelected(ControlledSelectable item, MenuEventPipeline pipeline = MenuEventPipeline.Pointer)
	{
	}

	public virtual void Activate(ControlledSelectable item, MenuEventPipeline pipeline = MenuEventPipeline.Pointer)
	{
	}

	public virtual void SetSelectedInput(IControlledInputField item, MenuEventPipeline pipeline = MenuEventPipeline.Pointer)
	{
	}

	public virtual void ActivateInput(IControlledInputField item, MenuEventPipeline pipeline = MenuEventPipeline.Pointer)
	{
	}
}
public abstract class QudBaseMenuController<DataT, UnityT> : QudBaseMenuController where UnityT : ControlledSelectable
{
	public AudioSource selectSound;

	private int _selectedOption;

	[NonSerialized]
	public Func<bool> isCurrentWindow = () => true;

	private List<UnityT> menuItems = new List<UnityT>();

	private List<GameObject> spacers = new List<GameObject>();

	public List<DataT> menuData;

	public List<ControlledTMPInputField> inputFields;

	[Obsolete]
	public SelectableTextMenuItem cancelButton;

	public GameObject menuItemPrefab;

	public GameObject spacerPrefab;

	public GameObject menuItemContainer;

	public QudMenuBottomContext menuBottomContext;

	public List<QudMenuItem> bottomContextOptions = new List<QudMenuItem>();

	public string cancelText = "cancel";

	public bool forceEditorRebuild;

	private int lastHash;

	public UnityEvent cancelHandlers = new UnityEvent();

	public UnityEvent<DataT> activateHandlers = new UnityEvent<DataT>();

	public UnityEvent<QudMenuItem> activateContextHandlers = new UnityEvent<QudMenuItem>();

	public UnityEvent<DataT> selectHandlers = new UnityEvent<DataT>();

	public int selectedOption
	{
		get
		{
			return _selectedOption;
		}
		set
		{
			if (value == -1)
			{
				_selectedOption = -1;
			}
			else
			{
				if (value == _selectedOption)
				{
					return;
				}
				_selectedOption = Math.Max(0, Math.Min(optionsCount - 1, value));
				int num = ((inputFields != null) ? inputFields.Count : 0);
				if (_selectedOption < num)
				{
					PlayClick();
					inputFields[_selectedOption].Init();
					inputFields[_selectedOption].selected = true;
				}
				else if (_selectedOption < menuItems.Count + num)
				{
					if (menuData != null && _selectedOption - num >= 0 && menuData.Count > _selectedOption - num && menuItems.Count > _selectedOption - num)
					{
						menuItems[_selectedOption - num].selected = true;
						PlayClick();
						selectHandlers.Invoke(menuData[_selectedOption - num]);
					}
				}
				else
				{
					int num2 = _selectedOption - menuItems.Count - num;
					if (num2 < menuBottomContext.buttons.Count)
					{
						PlayClick();
						menuBottomContext.buttons[num2].selected = true;
					}
				}
			}
		}
	}

	public int optionsCount => ((inputFields != null) ? inputFields.Count : 0) + ((menuData != null) ? menuData.Count : 0) + bottomContextOptions.Count;

	[Obsolete]
	public bool hasCancel => cancelButton != null;

	public IEnumerable<IControlledSelectable> GetOptions()
	{
		if (inputFields != null)
		{
			foreach (ControlledTMPInputField inputField in inputFields)
			{
				yield return inputField;
			}
		}
		foreach (UnityT menuItem in menuItems)
		{
			yield return menuItem;
		}
		foreach (SelectableTextMenuItem button in menuBottomContext.buttons)
		{
			yield return button;
		}
	}

	public override void SetSelected(ControlledSelectable item, MenuEventPipeline pipeline = MenuEventPipeline.Pointer)
	{
		lastEventSource = pipeline;
		if (!isCurrentWindow())
		{
			return;
		}
		int num = inputFields?.Count ?? 0;
		if (menuBottomContext.buttons.Contains(item as SelectableTextMenuItem))
		{
			PlayClick();
			_selectedOption = num + menuItems.Count + menuBottomContext.buttons.IndexOf((SelectableTextMenuItem)item);
			return;
		}
		_selectedOption = num + menuItems.IndexOf((UnityT)item);
		if (_selectedOption >= num && _selectedOption < num + menuData.Count)
		{
			PlayClick();
			selectHandlers.Invoke(menuData[_selectedOption]);
		}
	}

	public virtual void Reselect(int selectedOption)
	{
		_selectedOption = -1;
		this.selectedOption = selectedOption;
	}

	public QudMenuItem? GetSelectedContextButton()
	{
		if (selectedOption >= menuItems.Count + inputFields.Count)
		{
			return menuBottomContext.items[selectedOption - menuItems.Count - inputFields.Count];
		}
		return null;
	}

	protected void PlayClick()
	{
	}

	public virtual void UpdateButtonLayout()
	{
		if (menuItems.Count != menuData.Count || menuData.GetHashCode() != lastHash)
		{
			lastHash = menuData.GetHashCode();
			UpdateElements();
		}
		if (optionsCount > 0 && !GetOptions().Any((IControlledSelectable i) => i.IsSelected()))
		{
			selectedOption = Math.Min(selectedOption, optionsCount - 1);
		}
		if (menuBottomContext != null)
		{
			menuBottomContext.items = bottomContextOptions;
			menuBottomContext.controller = this;
		}
	}

	public virtual void Update()
	{
		if (menuItems == null)
		{
			menuItems = new List<UnityT>();
		}
		if (menuData == null)
		{
			menuData = new List<DataT>();
		}
		if ((!forceEditorRebuild && !Application.isPlaying) || !isCurrentWindow())
		{
			return;
		}
		if (forceEditorRebuild)
		{
			menuItems.Clear();
		}
		forceEditorRebuild = false;
		UpdateButtonLayout();
		if (!Application.isPlaying || !isCurrentWindow())
		{
			return;
		}
		if (ControlManager.isCommandDown("Navigate Up"))
		{
			lastEventSource = MenuEventPipeline.KeyboardOrJoystick;
			int num = Math.Min(selectedOption - 1, menuData.Count - 1);
			if (num < 0)
			{
				selectedOption = Math.Max(0, menuData.Count - 1);
			}
			else
			{
				selectedOption = num;
			}
		}
		if (ControlManager.isCommandDown("Navigate Down"))
		{
			lastEventSource = MenuEventPipeline.KeyboardOrJoystick;
			int num2 = selectedOption + 1;
			if (num2 >= optionsCount)
			{
				selectedOption = 0;
			}
			else
			{
				selectedOption = num2;
			}
		}
		if (ControlManager.isCommandDown("Page Up"))
		{
			ScrollRect componentInChildrenFast = gameObject.GetComponentInChildrenFast<ScrollRect>();
			if (componentInChildrenFast != null)
			{
				componentInChildrenFast.verticalNormalizedPosition += componentInChildrenFast.viewport.rect.height / componentInChildrenFast.content.rect.height;
				menuItems.Find((UnityT i) => i.IsInView())?.Select();
			}
		}
		if (ControlManager.isCommandDown("Page Down"))
		{
			ScrollRect componentInChildrenFast2 = gameObject.GetComponentInChildrenFast<ScrollRect>();
			if (componentInChildrenFast2 != null)
			{
				componentInChildrenFast2.verticalNormalizedPosition -= componentInChildrenFast2.viewport.rect.height / componentInChildrenFast2.content.rect.height;
				menuItems.FindLast((UnityT i) => i.IsInView())?.Select();
			}
		}
		if (ControlManager.isCommandDown("Accept"))
		{
			lastEventSource = MenuEventPipeline.KeyboardOrJoystick;
			QudMenuItem? selectedContextButton = GetSelectedContextButton();
			if (selectedContextButton.HasValue)
			{
				QudMenuItem value = selectedContextButton.Value;
				ActivateContextButton(value, lastEventSource);
			}
			else if (menuData.Count > selectedOption && selectedOption >= 0)
			{
				Activate(menuData[selectedOption], MenuEventPipeline.KeyboardOrJoystick);
			}
			return;
		}
		if (bottomContextOptions.Count > 0 && ControlManager.isCommandDown("Navigate Left"))
		{
			lastEventSource = MenuEventPipeline.KeyboardOrJoystick;
			int num3 = menuItems.Count + inputFields.Count;
			if (selectedOption >= num3)
			{
				selectedOption = Math.Max(num3, selectedOption - 1);
			}
			else
			{
				selectedOption = num3;
			}
		}
		if (bottomContextOptions.Count > 0 && ControlManager.isCommandDown("Navigate Right"))
		{
			lastEventSource = MenuEventPipeline.KeyboardOrJoystick;
			int num4 = menuItems.Count + inputFields.Count;
			if (selectedOption >= num4)
			{
				selectedOption = Math.Min(num4 + bottomContextOptions.Count - 1, selectedOption + 1);
			}
			else
			{
				selectedOption = num4 + bottomContextOptions.Count - 1;
			}
		}
		CheckKeyInteractions(bottomContextOptions, ActivateContextButton);
	}

	public virtual void CheckKeyInteractions(IEnumerable<QudMenuItem> items, Action<QudMenuItem, MenuEventPipeline> activateHandler)
	{
		foreach (QudMenuItem item in items)
		{
			foreach (string item2 in (item.hotkey ?? "").CachedCommaExpansion())
			{
				if (ControlManager.isCommandDown(item2))
				{
					activateHandler(item, MenuEventPipeline.KeyboardOrJoystick);
					continue;
				}
				if (item2.StartsWith("char:"))
				{
					char c = ((item2.Length > 5) ? item2[5] : '\0');
					if (c != '\0' && c != ' ' && ControlManager.isCharDown(c))
					{
						activateHandler(item, MenuEventPipeline.KeyboardOrJoystick);
					}
					continue;
				}
				try
				{
					if (ControlManager.isKeyDown(Keyboard.ParseUnityEngineKeyCode(item2)))
					{
						ActivateContextButton(item, MenuEventPipeline.KeyboardOrJoystick);
					}
				}
				catch (ArgumentException)
				{
				}
			}
		}
	}

	public virtual void UpdateElements(bool evenIfNotCurrent = false)
	{
		if (!isCurrentWindow() && !evenIfNotCurrent)
		{
			return;
		}
		List<GameObject> list = new List<GameObject>();
		foreach (Transform item in menuItemContainer.transform)
		{
			if (!menuItems.Contains(item.gameObject.GetComponent<UnityT>()) && !spacers.Contains(item.gameObject))
			{
				list.Add(item.gameObject);
			}
		}
		list.ForEach((GameObject o) =>
		{
			o.DestroyImmediate();
		});
		if (menuItems.Count > menuData.Count)
		{
			menuItems.GetRange(menuData.Count, menuItems.Count - menuData.Count).ForEach((UnityT o) =>
			{
				o.gameObject.DestroyImmediate();
			});
			menuItems.RemoveRange(menuData.Count, menuItems.Count - menuData.Count);
		}
		if (spacers.Count > Math.Max(0, menuData.Count - 1))
		{
			spacers.GetRange(Math.Max(0, menuData.Count - 1), spacers.Count - Math.Max(0, menuData.Count - 1)).ForEach((GameObject o) =>
			{
				o.gameObject.DestroyImmediate();
			});
			spacers.RemoveRange(Math.Max(0, menuData.Count - 1), spacers.Count - Math.Max(0, menuData.Count - 1));
		}
		while (menuItems.Count < menuData.Count)
		{
			if (menuItems.Count > 0 && spacerPrefab != null)
			{
				GameObject gameObject = spacerPrefab.Instantiate();
				gameObject.transform.SetParent(menuItemContainer.transform, worldPositionStays: false);
				spacers.Add(gameObject);
			}
			GameObject gameObject2 = menuItemPrefab.Instantiate();
			gameObject2.transform.SetParent(menuItemContainer.transform, worldPositionStays: false);
			UnityT component = gameObject2.GetComponent<UnityT>();
			component.controller = this;
			menuItems.Add(component);
		}
		for (int num = 0; num < menuData.Count; num++)
		{
			menuItems[num].data = menuData[num];
			menuItems[num].UpdateData();
		}
		ControlManager.currentContext.isInInput = IsInInput;
	}

	public bool IsInInput()
	{
		bool result = false;
		if (inputFields != null)
		{
			for (int i = 0; i < inputFields.Count; i++)
			{
				if (inputFields[i].isFocused && inputFields[i].selected)
				{
					return true;
				}
			}
		}
		return result;
	}

	public void Cancel()
	{
		if (isCurrentWindow())
		{
			cancelHandlers.Invoke();
		}
	}

	public override void Activate(ControlledSelectable item, MenuEventPipeline pipeline = MenuEventPipeline.Pointer)
	{
		lastEventSource = pipeline;
		if (isCurrentWindow())
		{
			if (menuBottomContext.buttons.Contains(item as SelectableTextMenuItem))
			{
				QudMenuItem data = (QudMenuItem)item.data;
				ActivateContextButton(data, pipeline);
			}
			else
			{
				Activate(menuData[menuItems.IndexOf((UnityT)item)], pipeline);
			}
		}
	}

	public virtual void Activate(DataT data, MenuEventPipeline pipeline)
	{
		lastEventSource = pipeline;
		activateHandlers.Invoke(data);
	}

	public virtual void ActivateContextButton(QudMenuItem data, MenuEventPipeline pipeline = MenuEventPipeline.Pointer)
	{
		lastEventSource = pipeline;
		activateContextHandlers.Invoke(data);
		if (data.command == "Cancel")
		{
			Cancel();
		}
	}
}
