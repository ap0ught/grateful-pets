using System;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class AmbientSoundGenerator : IPart
{
	[NonSerialized]
	public string[] SoundOptions;

	public string Sounds;

	public int ChancePerThousand = 150;

	public float Volume = 0.25f;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction" && !string.IsNullOrEmpty(Sounds) && !ParentObject.IsPlayerLed())
		{
			if (SoundOptions == null)
			{
				SoundOptions = Sounds.Split(',');
			}
			if (string.IsNullOrEmpty(XRLCore.Core.PlayerWalking) && Stat.Random(1, 1000) <= ChancePerThousand)
			{
				PlayWorldSound(SoundOptions[Stat.Random5(0, SoundOptions.Length - 1)], Volume);
			}
		}
		return base.FireEvent(E);
	}
}
