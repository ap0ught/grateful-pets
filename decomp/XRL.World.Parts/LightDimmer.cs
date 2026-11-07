using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class LightDimmer : IPart
{
	public int Chance = 100;

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		Tick(Amount);
	}

	public void Tick(int Increment = 1)
	{
		if (Stat.Random(1, Chance) > Increment)
		{
			return;
		}
		LightSource lightSource = ParentObject.GetPart<LightSource>();
		if (lightSource == null)
		{
			lightSource = new LightSource
			{
				Radius = 4
			};
			ParentObject.AddPart(lightSource);
		}
		if (50.in100())
		{
			if (lightSource.Radius > 1)
			{
				lightSource.Radius--;
				DidX("dim");
			}
		}
		else
		{
			lightSource.Radius++;
			DidX("brighten");
		}
	}
}
