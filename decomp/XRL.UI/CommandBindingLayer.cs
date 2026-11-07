using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;

namespace XRL.UI;

public class CommandBindingLayer
{
	public bool enabled;

	public string name;

	public List<CommandBinding> actions = new List<CommandBinding>();

	private int sanityCheck;

	public CommandBindingLayer(string name)
	{
		this.name = name;
	}

	public void Enable()
	{
		if (sanityCheck != 0)
		{
			MetricsManager.LogWarning("layer " + name + " didn't pass sanity check on Enable");
		}
		foreach (CommandBinding action in actions)
		{
			if (!action.enabled)
			{
				action.Enable();
			}
		}
		enabled = true;
	}

	public void Disable()
	{
		foreach (CommandBinding action in actions)
		{
			if (action.enabled)
			{
				action.Disable();
			}
		}
		enabled = false;
	}

	public InputAction AddAction(string id, InputActionType type)
	{
		try
		{
			sanityCheck++;
			CommandBindingManager.currentActionMap.Disable();
			return CommandBindingManager.currentActionMap.AddAction(id, type);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("InputActionLayer::AddAction", x);
		}
		return null;
	}

	public void AddBinding(CommandBinding binding)
	{
		actions.Add(binding);
		binding.Disable();
		sanityCheck--;
		if (sanityCheck != 0)
		{
			MetricsManager.LogWarning("layer " + name + " didn't pass sanity check on AddBinding");
		}
	}
}
