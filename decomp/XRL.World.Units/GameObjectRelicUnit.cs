using System;

namespace XRL.World.Units;

[Serializable]
public class GameObjectRelicUnit : GameObjectUnit
{
	public string Tier;

	public override void Apply(GameObject Object)
	{
		Object.ReceiveObject(RelicGenerator.GenerateRelic(Tier.RollCached(), RandomName: true));
	}

	public override void Reset()
	{
		base.Reset();
		Tier = null;
	}

	public override string GetDescription(bool Inscription = false)
	{
		int num = Tier.RollMax();
		string text = "low";
		if (num > 5)
		{
			text = "high";
		}
		else if (num > 2)
		{
			text = "mid";
		}
		return "Spawns with a " + text + "-tier relic";
	}
}
