using System;
using LaundryBear.PlatformServices;
using Platform.IO;
using UnityEngine;
using XRL;

namespace Platform;

public struct DelayShutdown : IDisposable
{
	private bool constructedProperly;

	private bool alreadyDisposed;

	private bool forceMainThread;

	public static DelayShutdown AutoScope()
	{
		DelayShutdown result = default;
		result.constructedProperly = true;
		result.Begin();
		return result;
	}

	public static DelayShutdown AutoScopeForceMainThread()
	{
		DelayShutdown result = default;
		result.constructedProperly = true;
		result.forceMainThread = true;
		result.Begin();
		return result;
	}

	public void Dispose()
	{
		if (WasConstructedProperly() && !alreadyDisposed)
		{
			alreadyDisposed = true;
			End();
		}
	}

	private void Begin()
	{
		if (WasConstructedProperly())
		{
			if (forceMainThread)
			{
				BeginRawForceMainThread();
			}
			else
			{
				BeginRaw();
			}
		}
	}

	private void End()
	{
		if (forceMainThread)
		{
			EndRawForceMainThread();
		}
		else
		{
			EndRaw();
		}
	}

	public static void BeginRawForceMainThread()
	{
		The.UiContext.Send((object _) =>
		{
			BeginRaw();
		}, null);
	}

	public static void EndRawForceMainThread()
	{
		The.UiContext.Send((object _) =>
		{
			EndRaw();
		}, null);
	}

	public static void BeginRaw()
	{
		if (!(State.GetStorage() is ICanDelayShutdown canDelayShutdown))
		{
			Debug.LogError("Could not find ICanDelayShutdown service. not implemented for this platform?");
			return;
		}
		canDelayShutdown.IsUserHandlingShutdownDelay = true;
		canDelayShutdown.BeginDelayShutdown();
	}

	public static void EndRaw()
	{
		if (!(State.GetStorage() is ICanDelayShutdown canDelayShutdown))
		{
			Debug.LogError("Could not find ICanDelayShutdown service. not implemented for this platform?");
			return;
		}
		canDelayShutdown.EndShutdownDelay();
		canDelayShutdown.IsUserHandlingShutdownDelay = false;
	}

	private bool WasConstructedProperly()
	{
		if (!constructedProperly)
		{
			Debug.LogError("DelayShutdown was not properly constructed. This struct should be constructed using AutoScope or AutoScopeForceMainThread or will not do anything.)");
		}
		return constructedProperly;
	}
}
