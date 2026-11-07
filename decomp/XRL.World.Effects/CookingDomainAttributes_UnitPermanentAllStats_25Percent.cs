using System;
using Qud.API;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainAttributes_UnitPermanentAllStats_25Percent : ProceduralCookingEffectUnit
{
	public const string RANDOM_SEED = "CookingDomainAttributes_UnitPermanentAllStats_25Percent";

	public int Bonus = 1;

	public bool Succeed;

	public override string GetDescription()
	{
		if (!Succeed)
		{
			return "Nothing happened.";
		}
		return Bonus.Signed() + " to all six attributes permanently";
	}

	public override string GetTemplatedDescription()
	{
		return "25% chance to gain +1 Strength, Agility, Toughness, Intelligence, Willpower, and Ego permanently.";
	}

	public override void Init(GameObject target)
	{
		target.WithSeededRandom((Random rng) =>
		{
			Succeed = 25.in100(rng);
		}, "CookingDomainAttributes_UnitPermanentAllStats_25Percent");
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		Object.PermuteRandomMutationBuys();
		if (Succeed)
		{
			Object.GetStat("Strength").BaseValue += Bonus;
			Object.GetStat("Agility").BaseValue += Bonus;
			Object.GetStat("Toughness").BaseValue += Bonus;
			Object.GetStat("Intelligence").BaseValue += Bonus;
			Object.GetStat("Willpower").BaseValue += Bonus;
			Object.GetStat("Ego").BaseValue += Bonus;
			if (Object.IsPlayer())
			{
				JournalAPI.AddAccomplishment("You rejoiced in a drop of nectar.", "=name= the Eater supped on royal nectar and metamorphosed into Godhead.", "While traveling through " + The.Player.CurrentZone.GetTerrainDisplayName() + ", =name= stopped at a tavern near " + JournalAPI.GetLandmarkNearestPlayer().Text + ". There " + The.Player.GetPronounProvider().Subjective + " wisely declined to play a game of dice and instead supped on a nourishing serving of Eaters' nectar.", null, "general", MuralCategory.BodyExperienceGood, MuralWeight.Low, null, -1L);
			}
		}
		if (Object.IsPlayer())
		{
			Object.GetPart<Leveler>().SifrahInsights();
		}
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}
}
