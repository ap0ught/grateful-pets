using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public abstract class ISoundPart : IPart
{
	[NonSerialized]
	public string[] SoundOptions;

	public string Sounds;

	public bool Played;

	public float Volume = 1f;

	public virtual void Trigger()
	{
		if (Sounds != null)
		{
			Played = true;
			if (SoundOptions == null)
			{
				SoundOptions = Sounds.Split(',');
			}
			PlayWorldSound(SoundOptions[Stat.Random5(0, SoundOptions.Length - 1)], Volume);
		}
	}
}
