using System;

namespace XRL.World.Effects;

[Serializable]
public class Heal20_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public override string GetDescription()
	{
		return "heal 20 hp";
	}

	public override void Apply(GameObject go)
	{
		go.Statistics["Hitpoints"].Penalty -= 20;
	}
}
