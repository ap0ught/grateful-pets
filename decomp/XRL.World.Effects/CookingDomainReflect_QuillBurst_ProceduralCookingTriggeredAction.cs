using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainReflect_QuillBurst_ProceduralCookingTriggeredAction : ProceduralCookingTriggeredAction
{
	public int Tier;

	public override void Init(GameObject target)
	{
		Tier = Stat.Random(8, 9);
		base.Init(target);
	}

	public override string GetDescription()
	{
		return "@they expel quills per the Quills mutation at level " + Tier + ".";
	}

	public override string GetTemplatedDescription()
	{
		return "@they expel quills per the Quills mutation at level 8-9.";
	}

	public override string GetNotification()
	{
		return "@they expel a blast of quills.";
	}

	public override void Apply(GameObject go)
	{
		go.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_quills_expel");
		foreach (Cell localAdjacentCell in go.Physics.CurrentCell.GetLocalAdjacentCells())
		{
			Quills quills = new Quills();
			quills.Level = Tier;
			quills.ParentObject = go;
			quills.QuillFling(localAdjacentCell, Stat.Roll("1d4+1"), UseQuills: false);
		}
	}
}
