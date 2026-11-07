using System;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Parts;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainSpecial_UnitCrystalTransform : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "@they became a crystal being permanently.";
	}

	public override string GetTemplatedDescription()
	{
		return "?????";
	}

	public override void Init(GameObject target)
	{
	}

	public override void Apply(GameObject Object, Effect parent)
	{
		ApplyTo(Object);
	}

	public override void Remove(GameObject Object, Effect parent)
	{
	}

	public static void ApplyTo(GameObject Object)
	{
		if (Object.GetPropertyOrTag("AteCrystalDelight") == "true")
		{
			Object.ShowFailure("Your genome has already undergone this transformation.");
			return;
		}
		if (Object.IsPlayer())
		{
			Popup.Show("...");
			Popup.Show("You feel an uncomfortable pressure across the length of your body.");
			Popup.Show("Your limbs suddenly feel perversely round and bumpy before they shrink back into your body.");
			Popup.Show("An abrading force cuts the surfaces of your torso until you feel polished and perfectly smooth.");
			Popup.Show("A quartet of prisms shatter out of you and grow. Each quinfurcates at the tip into five finger prisms.");
			Popup.Show("You gained the mutation {{C|Crystallinity}}!");
			JournalAPI.AddAccomplishment("You ate some Crystal Delight and became a crystalline being.", "Crystalform! Crystalform! On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", in the year " + Calendar.GetYear() + " AR, =name= underwent the divine transformation and became a crystal being.", "<spice.instancesOf.inYear.!random.capitalize> =year=, <spice.instancesOf.afterTumultuousYears.!random>, " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " counselors suggested " + The.Player.GetPronounProvider().Subjective + " <spice.instancesOf.abdicate.!random> as sultan. Instead, " + The.Player.GetPronounProvider().Subjective + " ate a meal and underwent a transformation into a crystalline being.", null, "general", MuralCategory.BodyExperienceNeutral, MuralWeight.VeryHigh, null, -1L);
			Achievement.ATE_CRYSTAL_DELIGHT.Unlock();
		}
		Body body = Object.Body;
		body.Rebuild("HumanoidOctohedron");
		Mutations mutations = Object.RequirePart<Mutations>();
		if (MutationFactory.TryGetMutationEntry("Crystallinity", out var Entry))
		{
			mutations.AddMutation(Entry);
		}
		body.RegenerateDefaultEquipment();
		Object.Render.RenderString = "µ";
		Object.Render.Tile = "Creatures/sw_crystal_body.png";
		Object.SetStringProperty("AteCrystalDelight", "true");
		Object.SetStringProperty("Species", "crystal");
	}
}
