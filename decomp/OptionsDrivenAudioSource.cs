using UnityEngine;
using XRL.Core;

public class OptionsDrivenAudioSource : MonoBehaviour
{
	public AudioSource source;

	private float volume;

	private void Awake()
	{
		if (source == null)
		{
			source = GetComponent<AudioSource>();
		}
		if (source != null)
		{
			volume = source.volume;
		}
		if (source.enabled && !Globals.EnableSound)
		{
			source.enabled = false;
		}
		else if (!source.enabled && Globals.EnableSound)
		{
			source.enabled = true;
		}
	}

	private void Update()
	{
		if (source.enabled && !Globals.EnableSound)
		{
			source.enabled = false;
		}
		if (Globals.EnableSound)
		{
			if (!source.enabled && Globals.EnableSound)
			{
				source.enabled = true;
			}
			if (source != null)
			{
				source.volume = SoundManager.MasterVolume * SoundManager.SoundVolume * volume;
			}
		}
	}
}
