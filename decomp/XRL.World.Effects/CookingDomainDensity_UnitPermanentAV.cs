using System;
using Qud.API;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainDensity_UnitPermanentAV : ProceduralCookingEffectUnit
{
	public int Bonus = 1;

	public bool Collapse;

	public override string GetDescription()
	{
		if (Collapse)
		{
			return "{{R|CRITICAL GRAVITATIONAL COLLAPSE}}";
		}
		return Bonus.Signed() + " AV permanently";
	}

	public override string GetTemplatedDescription()
	{
		return "+1 AV permanently. Small chance of gravitational collapse.";
	}

	public override void Init(GameObject target)
	{
		Collapse = 10.in100();
		Bonus = 1;
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		if (Collapse)
		{
			Object.Explode(15000, null, "10d10+250", 1f, Neutron: true);
			return;
		}
		Object.GetStat("AV").BaseValue += Bonus;
		if (Object.IsPlayer())
		{
			JournalAPI.AddAccomplishment("You became denser.", "On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", =name= uncorked the cosmic wine and imbibed the forbidden starjuice. O, the mass within!", "Somewhere in " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= had a dream that <entity.subjectPronoun> imbibed a dram of starjuice. <spice.commonPhrases.fromThenOn.!random.capitalize>, <entity.subjectPronoun> always kept some stars in a bottle on <entity.possessivePronoun> person.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.Medium, null, -1L);
			Achievement.COOKED_FLUX.Unlock();
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
