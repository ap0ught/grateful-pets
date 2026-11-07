using System;

namespace XRL.World.Parts;

[Serializable]
public class MechanimistLibrarian : IPart
{
	public override void Initialize()
	{
		try
		{
			ParentObject.GiveProperName("Sheba Hagadias", Force: true);
			ParentObject.RequirePart<Titles>().AddTitle(ParentObject.GetCreatureType());
			ParentObject.RequirePart<Titles>().AddTitle("librarian of the Stilt");
			ParentObject.RequirePart<DisplayNameColor>().SetColor("Y");
			ParentObject.Render.ColorString = "&W";
			ParentObject.Render.TileColor = "&C";
			ParentObject.Render.DetailColor = "W";
			if (ParentObject.BaseStat("Intelligence") < 27)
			{
				ParentObject.GetStat("Intelligence").BaseValue = 27;
			}
			ParentObject.RemovePart<AISitting>();
			ParentObject.ReceiveObject("Cloth Robe");
			ParentObject.ReceiveObject("Spectacles");
			ParentObject.SetStringProperty("Mayor", "Mechanimists");
			ParentObject.GetPart<Description>().Short = "In the narthex of the Stilt, cloistered beneath a marble arch and close to =pronouns.possessive= Argent Fathers, =pronouns.subjective= =verb:muse:afterpronoun= over a tattered codex. =pronouns.Subjective==verb:'re:afterpronoun= safe here, but it wasn't always that way. As a youngling, =pronouns.possessive= own kind understood =pronouns.objective= little. Only when =pronouns.subjective= =verb:were:afterpronoun= gifted a copy of the Canticles Chromaic did =pronouns.subjective= learn comfort, or mirth, or reason. =pronouns.Possessive= journey to the Stilt took several years, but now that =pronouns.subjective==verb:'re:afterpronoun= here, Sheba =verb:seek= to consolidate all the learning of the ages tucked away in Qud's innumerable chrome nooks. Here, =pronouns.subjective= =verb:prepare:afterpronoun= a residence where pilgrims can study the wisdom of others and bring themselves nearer to the divinity of the Kasaphescence.";
			if (ParentObject.GetGender().Name != "female")
			{
				ParentObject.SetPronounSet("she/her");
			}
			ParentObject.SetIntProperty("Librarian", 1);
			ParentObject.RemovePart<Chat>();
			ParentObject.RemovePart<ConversationScript>();
			ParentObject.RemovePart<Miner>();
			ParentObject.RemovePart<TurretTinker>();
			ParentObject.AddPart(new ConversationScript("MechanimistLibrarian", ClearLost: true));
			ParentObject.RequirePart<AISuppressIndependentBehavior>();
			ParentObject.RequirePart<Interesting>();
			TakeOnRoleEvent.Send(ParentObject, "Librarian");
			ParentObject.RemovePart(this);
		}
		catch (Exception x)
		{
			MetricsManager.LogException("Librarian", x);
		}
	}
}
