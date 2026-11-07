using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainElectric_Discharge_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier = 5;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(5, 6);
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they release an electrical discharge per Electrical Generation at level " + Tier + ".";
	}

	public override string GetTemplatedDescription()
	{
		return "@they release an electrical discharge per Electrical Generation at level 5-6.";
	}

	public override string GetNotification()
	{
		return "@they release a powerful electrical discharge.";
	}

	public override void Apply(GameObject go)
	{
		ElectricalGeneration electricalGeneration = new ElectricalGeneration
		{
			ParentObject = go,
			Level = Tier
		};
		electricalGeneration.Charge = electricalGeneration.GetMaxCharge();
		electricalGeneration.PerformDischarge(AllowCancel: false);
	}
}
