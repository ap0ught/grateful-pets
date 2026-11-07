using System;
using XRL.Language;

namespace XRL.World.Parts;

[Serializable]
public class CrypticParticleText : IPart
{
	public int Chance = 8;

	public int Timer;

	public string InitialTimer
	{
		set
		{
			Timer = value?.RollCached() ?? Timer;
		}
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		if (Timer > 0)
		{
			Timer = Math.Max(Timer - Amount, 0);
			if (Timer == 0)
			{
				Preach();
			}
		}
		else if (Chance.in100())
		{
			Preach();
		}
	}

	public void Preach()
	{
		string text = TextFilters.GenerateCrypticWord();
		ParentObject.ParticleText("{{c|'" + text + "'}}", IgnoreVisibility: true);
	}
}
