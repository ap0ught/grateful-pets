using System;
using Qud.API;
using XRL.Language;
using XRL.UI;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class CookingDomainSpecial_UnitSlogTransform : ProceduralCookingEffectUnit
{
	public override string GetDescription()
	{
		return "@they became a slug permanently.";
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
		if (Object.GetPropertyOrTag("AteCloacaSurprise") == "true")
		{
			Object.ShowFailure("Your genome has already undergone this transformation.");
			return;
		}
		if (Object.IsPlayer())
		{
			Popup.Show("...");
			Popup.Show("You feel an uncomfortable pressure across the length of your body.");
			Popup.Show("Feelers rip through your scalp and shudder with curiosity.");
			Popup.Show("Your arms shrink into your torso.");
			Popup.Show("A bilge hose painted with mucus undulates out of your lower body. It spews the amniotic broth of its birth from its sputtering mouth.");
			Popup.Show("You gained the mutation {{w|Bilge Sphincter}}!");
			JournalAPI.AddAccomplishment("You ate the Cloaca Surprise.", "Slugform! Slugform! On the " + Calendar.GetDay() + " of " + Calendar.GetMonth() + ", in the year " + Calendar.GetYear() + " AR, =name= underwent the divine transformation and assumed the Slugform.", "Deep in the bowels of Golgotha, =name= came upon a giant slug performing a secret ritual. Because of " + Grammar.MakePossessive(The.Player.BaseDisplayNameStripped) + " <spice.elements." + The.Player.GetMythicDomain() + ".quality.!random>, the slug taught " + The.Player.BaseDisplayNameStripped + " the secret to being a slug.", null, "general", MuralCategory.BodyExperienceNeutral, MuralWeight.VeryHigh, null, -1L);
			Achievement.ATE_SURPRISE.Unlock();
		}
		Object.Body.Rebuild("SlugWithHands");
		Object.RequirePart<Mutations>().AddMutation(new SlogGlands());
		Object.Render.RenderString = "Q";
		Object.Render.Tile = "Creatures/sw_slog.bmp";
		Object.SetStringProperty("AteCloacaSurprise", "true");
		Object.SetStringProperty("Species", "slug");
	}
}
