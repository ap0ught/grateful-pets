using System;
using LaundryBear.PlatformServices;
using UnityEngine;

namespace Platform.IO;

public static class State
{
	private static IStorage? storage;

	public static void SetStorage(IStorage storage)
	{
		if (State.storage != null)
		{
			Debug.LogWarning("Trying to set storage to " + storage.GetType().Name + " but it has already been set");
			return;
		}
		Debug.Log("Setting Storage to " + storage.GetType().Name);
		State.storage = storage;
	}

	public static IStorage GetStorage()
	{
		if (storage == null)
		{
			throw new Exception("Couldn't get IStorage service... are you calling into Platform.IO before it is initialized?");
		}
		return storage;
	}
}
