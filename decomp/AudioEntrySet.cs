using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using XRL.Collections;

public class AudioEntrySet : Rack<AudioEntry>
{
	public class Config
	{
		public int Shuffle = -1;
	}

	public static Dictionary<AudioEntrySet, UniTask> Tasks = new Dictionary<AudioEntrySet, UniTask>();

	public int Index;

	public bool Shuffle = true;

	public bool Initialized;

	private static System.Random ClipShuffler = new System.Random();

	public virtual void Initialize(string Path)
	{
		TextAsset textAsset = Resources.Load<TextAsset>(Path);
		if ((bool)textAsset)
		{
			Config config = JsonUtility.FromJson<Config>(textAsset.text);
			if (config != null && config.Shuffle != -1)
			{
				Shuffle = config.Shuffle == 1;
			}
		}
	}

	public int IndexOfVariant(int Variant)
	{
		for (int i = 0; i < Length; i++)
		{
			if (Items[i].Variant == Variant)
			{
				return i;
			}
		}
		return -1;
	}

	public async UniTask Load()
	{
		if (Initialized)
		{
			return;
		}
		if (Tasks.TryGetValue(this, out var value))
		{
			await value;
			return;
		}
		UniTaskCompletionSource source = new UniTaskCompletionSource();
		Tasks.Add(this, source.Task);
		for (int i = 0; i < Length; i++)
		{
			(await Items[i].GetClip()).LoadAudioData();
		}
		Initialized = true;
		source.TrySetResult();
		Tasks.Remove(this);
	}

	public void Unload()
	{
		for (int i = 0; i < Length; i++)
		{
			Items[i].Unload();
		}
		Initialized = false;
	}

	public AudioEntry Next()
	{
		int length = Length;
		if (length == 0)
		{
			return null;
		}
		AudioEntry result = Items[Index];
		if (Index < length - 1)
		{
			Index++;
			return result;
		}
		if (Shuffle && length > 1)
		{
			ShuffleInPlace(ClipShuffler);
			Index = 0;
		}
		return result;
	}
}
