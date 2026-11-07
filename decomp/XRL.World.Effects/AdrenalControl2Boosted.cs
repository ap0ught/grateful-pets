using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

public class AdrenalControl2Boosted : Effect
{
	public int AppliedSpeedBonus;

	public int AppliedMutationBonus;

	public AdrenalControl2Boosted()
	{
		DisplayName = "{{R|adrenaline flowing}}";
	}

	public AdrenalControl2Boosted(AdrenalControl2 mutation)
		: this()
	{
		AppliedSpeedBonus = mutation.GetQuicknessBonus(mutation.Level);
		AppliedMutationBonus = mutation.GetMutationBonus(mutation.Level);
		Duration = mutation.GetQuicknessDuration(mutation.Level);
	}

	public override void Remove(GameObject Object)
	{
		PlayWorldSound("sfx_ability_mutation_physical_generic_deactivate");
	}

	public override int GetEffectType()
	{
		return 4;
	}

	public override string GetDetails()
	{
		if (AppliedMutationBonus == 1)
		{
			return $"+{AppliedSpeedBonus} Quickness\n+{AppliedMutationBonus} rank to physical mutations";
		}
		return $"+{AppliedSpeedBonus} Quickness\n+{AppliedMutationBonus} ranks to physical mutations";
	}
}
