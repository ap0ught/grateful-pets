using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_EMP_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier = 8;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(8, 9);
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they release an electromagnetic pulse at level " + Tier + ".";
	}

	public override string GetTemplatedDescription()
	{
		return "@they release an electromagnetic pulse at level 8-9.";
	}

	public override string GetNotification()
	{
		return "@they release a powerful electromagnetic pulse.";
	}

	public override void Apply(GameObject go)
	{
		ElectromagneticPulse.EMP(go.CurrentCell, ElectromagneticPulse.GetRadius(Tier), Stat.Random(4 + Tier * 2, 13 + Tier * 2), IncludeBaseCell: false, go.GetPhase());
	}
}
