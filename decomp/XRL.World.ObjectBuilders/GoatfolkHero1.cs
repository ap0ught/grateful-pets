using System.Collections.Generic;
using XRL.Names;
using XRL.Rules;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World.ObjectBuilders;

public class GoatfolkHero1 : IObjectBuilder
{
	public string ForceTitle;

	public string ForceName;

	public override void Initialize()
	{
		ForceTitle = null;
		ForceName = null;
	}

	public override void Apply(GameObject GO, string Context)
	{
		Dictionary<string, string> dictionary = new Dictionary<string, string>();
		if (!ForceTitle.IsNullOrEmpty())
		{
			dictionary["*Position*"] = ForceTitle;
		}
		string text = NameMaker.MakeEpithet(GO, null, null, null, null, null, null, null, null, null, "Hero", dictionary, SpecialFaildown: true);
		string text2 = NameMaker.MakeTitle(GO, null, null, null, null, null, null, null, null, null, "Hero", dictionary, SpecialFaildown: true);
		string forceName = ForceName;
		bool force = !ForceName.IsNullOrEmpty();
		Dictionary<string, string> namingContext = dictionary;
		if (GO.GiveProperName(forceName, force, "Hero", SpecialFaildown: true, null, null, namingContext) == null)
		{
			_ = GO.ShortDisplayName;
		}
		GO.RequirePart<DisplayNameColor>().SetColorByPriority("M", 30);
		if (!text.IsNullOrEmpty())
		{
			GO.RequirePart<Epithets>().AddEpithet(text, -40);
		}
		if (!text2.IsNullOrEmpty())
		{
			GO.RequirePart<Titles>().AddTitle(text2, -40);
		}
		Mutations mutations = GO.RequirePart<Mutations>();
		switch (text)
		{
		case "Stargazer":
			GO.BoostStat("Intelligence", 2);
			if (!mutations.HasMutation("LightManipulation"))
			{
				mutations.AddMutation(new LightManipulation(), 5);
			}
			break;
		case "Heartbiter":
			if (!mutations.HasMutation("AdrenalControl2"))
			{
				mutations.AddMutation(new AdrenalControl2());
			}
			break;
		case "Twicetalker":
			if (!mutations.HasMutation("TwoHeaded"))
			{
				mutations.AddMutation(new TwoHeaded());
				GO.ReceiveObject(GameObject.Create("Goatfolk_Horns", 0, 0, null, null, null, Context));
			}
			break;
		case "Souldrinker":
			if (!mutations.HasMutation("Syphon Vim"))
			{
				mutations.AddMutation(new LifeDrain());
			}
			break;
		case "Whitefinger":
			if (!mutations.HasMutation("ElectricalGeneration"))
			{
				mutations.AddMutation(new ElectricalGeneration(), 4);
			}
			break;
		case "Clovenhorn":
			GO.BoostStat("Strength", 1);
			if (!mutations.HasMutation("Horns"))
			{
				mutations.AddMutation(new Horns(), Stat.Random(5, 6));
			}
			if (!GO.HasSkill("Tactics_Charge"))
			{
				GO.AddSkill("Tactics_Charge");
			}
			break;
		}
		if (!text2.IsNullOrEmpty())
		{
			if (text2.Contains("Clan Hotur"))
			{
				GO.BoostStat("Strength", 1);
			}
			if (text2.Contains("Clan Ibex"))
			{
				mutations.AddMutation(new Horns(), 2);
			}
			if (text2.Contains("Clan Sol"))
			{
				if (!mutations.HasMutation("PhotosyntheticSkin"))
				{
					mutations.AddMutation(new PhotosyntheticSkin(), 4);
				}
				GoatfolkClan1 part = GO.GetPart<GoatfolkClan1>();
				if (part != null)
				{
					part.Photosynthetic = true;
				}
			}
			if (text2.Contains("Clan Whitetongue"))
			{
				GO.BoostStat("Intelligence", 1);
				GO.BoostStat("Ego", 1);
				GO.BoostStat("Willpower", 1);
			}
			if (text2.Contains("Clan Yr"))
			{
				GO.BoostStat("MoveSpeed", -0.75);
			}
			if (text2.Contains("Clan Mnim"))
			{
				GO.BoostStat("Toughness", 1);
			}
		}
		GO.ReceiveObjectFromPopulation("Armor 4", null, NoStack: false, 50, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Junk 4", null, NoStack: false, 0, 0, null, null, null, Context);
		GO.ReceiveObjectFromPopulation("Junk 4R", null, NoStack: false, 0, 0, null, null, null, Context);
		GO.MultiplyStat("Hitpoints", 2);
	}
}
