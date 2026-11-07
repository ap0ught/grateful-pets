using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class AudioEntry
{
	public static Dictionary<AudioEntry, UniTask<Object>> Tasks = new Dictionary<AudioEntry, UniTask<Object>>();

	public AudioClip Clip;

	public string Path;

	public int Variant;

	public virtual void Unload()
	{
		Resources.UnloadAsset(Clip);
		Clip = null;
	}

	public virtual async UniTask<AudioClip> GetClip()
	{
		if ((object)Clip != null)
		{
			return Clip;
		}
		if (Tasks.TryGetValue(this, out var value))
		{
			return (AudioClip)(await value);
		}
		UniTaskCompletionSource<Object> source = new UniTaskCompletionSource<Object>();
		Tasks.Add(this, source.Task);
		Clip = (AudioClip)(await Resources.LoadAsync<AudioClip>(Path));
		source.TrySetResult(Clip);
		Tasks.Remove(this);
		return Clip;
	}
}
