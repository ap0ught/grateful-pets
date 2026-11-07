using System;
using System.Collections.Generic;
using UnityEngine;

namespace XRL.UI.Framework;

public class FrameworkUnityController : MonoBehaviour
{
	public bool DebugContextActivity;

	public static FrameworkUnityController instance;

	public NavigationController controller;

	private static HashSet<string> usedCommands = new HashSet<string>();

	public void Awake()
	{
		instance = this;
		controller = NavigationController.instance;
	}

	public void Update()
	{
		NavigationContext navigationContext = controller.activeContext;
		usedCommands.Clear();
		while (navigationContext != null)
		{
			NavigationContext navigationContext2 = navigationContext;
			if (navigationContext.commandHandlers != null)
			{
				foreach (KeyValuePair<string, Action> commandHandler in navigationContext.commandHandlers)
				{
					if (usedCommands.Contains(commandHandler.Key))
					{
						continue;
					}
					usedCommands.Add(commandHandler.Key);
					if (ControlManager.isCommandDown(commandHandler.Key))
					{
						Event obj = controller.FireInputCommandEvent(commandHandler.Key);
						if (obj.cancelled || obj.handled || navigationContext2 != controller.activeContext)
						{
							return;
						}
						navigationContext2 = navigationContext;
					}
				}
			}
			navigationContext = navigationContext.parentContext;
		}
		if (controller.activeContext != null)
		{
			if (ControlManager.isCommandDown("Navigate Left"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationXAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Navigate Right"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationXAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Navigate Up"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationYAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Navigate Down"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationYAxis, null, 1);
			}
			if (ControlManager.isCommandDown("U Negative"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationUAxis, null, -1);
			}
			if (ControlManager.isCommandDown("U Positive"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationUAxis, null, 1);
			}
			if (ControlManager.isCommandDown("V Negative"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationVAxis, null, -1);
			}
			if (ControlManager.isCommandDown("V Positive"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationVAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Page Up"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageYAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Page Down"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageYAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Page Left"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageXAxis, null, -1);
			}
			if (ControlManager.isCommandDown("Page Right"))
			{
				controller.FireInputAxisEvent(InputAxisTypes.NavigationPageXAxis, null, 1);
			}
			if (ControlManager.isCommandDown("Accept"))
			{
				controller.FireInputButtonEvent(InputButtonTypes.AcceptButton);
			}
			if (ControlManager.isCommandDown("Cancel"))
			{
				controller.FireInputButtonEvent(InputButtonTypes.CancelButton);
			}
		}
	}
}
