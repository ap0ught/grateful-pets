using System;
using System.Collections.Generic;
using System.Linq;
using HistoryKit;
using Qud.API;
using XRL.Language;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class CherubimSpawner : IPart
{
	public static readonly string BaseDescription = "Gallium veins press against the underside of =pronouns.possessive= crystalline *skin* and gleam warmly. =pronouns.Possessive= body is perfect, and the whole of it is wet with amniotic slick; could =pronouns.subjective= have just now peeled =pronouns.reflexive= off an oil canvas? =verb:Were:afterpronoun= =pronouns.subjective= cast into the material realm by a dreaming, dripping brain? Whatever the embryo, =pronouns.subjective= =verb:are:afterpronoun= now the archetypal *creatureType*; it's all there in impeccable simulacrum: *features*. Perfection is realized.";

	public static readonly string BaseMechanicalDescription = "Dials tick and vacuum tubes mantle under synthetic *skin* and inside plastic joints. *features* are wrought from a vast and furcate machinery into the ideal form of the *creatureType*. By the artistry of =pronouns.possessive= construction, =pronouns.subjective= closely =verb:resemble:afterpronoun= =pronouns.possessive= referent, but an exposed cog here and an exhaust valve there betray the truth of =pronouns.possessive= nature. =pronouns.Possessive= movements are short and mimetic; =pronouns.subjective= =verb:inhabit:afterpronoun= the valley between the mountains of life and imagination.";

	public static readonly List<string> Factions = new List<string>
	{
		"Prey", "Baboons", "Apes", "Crabs", "Bears", "Winged Mammals", "Birds", "Fish", "Insects", "Swine",
		"Reptiles", "Cannibals", "Arachnids", "Cats", "Dogs", "Mollusks", "Tortoises", "Robots", "Baetyls", "Antelopes",
		"Worms", "Oozes", "Equines", "Newly Sentient Beings", "Hermits", "Frogs", "Strangers", "Flowers", "Roots", "Fungi",
		"Vines", "Urchins", "Succulents", "Trees"
	};

	public static readonly List<string> Elements = new List<string>
	{
		"glass", "jewels", "stars", "time", "salt", "ice", "scholarship", "might", "chance", "circuitry",
		"travel"
	};

	public string Group = "A";

	public int Period = 1;

	public bool bDynamic = true;

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		base.Register(Object, Registrar);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeObjectCreatedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeObjectCreatedEvent E)
	{
		string text = "";
		if (Period >= 4)
		{
			text = "Mechanical ";
		}
		HistoricEntitySnapshot sultan = HistoryAPI.GetSultanForPeriod(Period);
		string AStateName = "cherubim" + Period + "Afaction";
		string AFaction = The.Game.RequireGameState(AStateName, () =>
		{
			string text5 = null;
			if (sultan == null)
			{
				MetricsManager.LogError("no sultan found for period " + Period + " generating " + AStateName);
			}
			else
			{
				List<string> list = sultan.GetList("likedFactions");
				if (list == null || list.Count == 0)
				{
					MetricsManager.LogError("no liked factions found for period " + Period + " sultan generating " + AStateName);
				}
				else
				{
					IEnumerable<string> enumerable = list.Where((string f) => Factions.Contains(f));
					if (enumerable == null)
					{
						MetricsManager.LogError("no eligible factions found for period " + Period + " sultan generating " + AStateName);
					}
					else
					{
						text5 = enumerable.GetRandomElement();
					}
				}
			}
			if (text5 == null)
			{
				text5 = Factions.GetRandomElement();
			}
			return text5;
		});
		string BStateName = "cherubim:" + Period + "Bfaction";
		string text2 = The.Game.RequireGameState(BStateName, () =>
		{
			string text5 = null;
			if (sultan == null)
			{
				MetricsManager.LogError("no sultan found for period " + Period + " generating " + BStateName);
			}
			else
			{
				List<string> list = sultan.GetList("likedFactions");
				if (list == null || list.Count == 0)
				{
					MetricsManager.LogError("no liked factions found for period " + Period + " sultan generating " + BStateName);
				}
				else
				{
					IEnumerable<string> enumerable = list.Where((string f) => Factions.Contains(f) && f != AFaction);
					if (enumerable == null)
					{
						MetricsManager.LogError("no eligible factions found for period " + Period + " sultan generating " + BStateName);
					}
					else
					{
						text5 = enumerable.GetRandomElement();
					}
				}
			}
			if (text5 == null)
			{
				text5 = AFaction;
			}
			return text5;
		});
		string AElement = The.Game.RequireGameState("cherubim" + Period + "Aelement", () =>
		{
			HistoricEntitySnapshot historicEntitySnapshot = sultan;
			object obj;
			if (historicEntitySnapshot == null)
			{
				obj = null;
			}
			else
			{
				List<string> list = historicEntitySnapshot.GetList("elements");
				obj = ((list != null) ? list.GetRandomElement() : null);
			}
			if (obj == null)
			{
				obj = Elements.GetRandomElement();
			}
			return (string)obj;
		});
		string text3 = The.Game.RequireGameState("cherubim:" + Period + "Belement", () =>
		{
			HistoricEntitySnapshot historicEntitySnapshot = sultan;
			object obj;
			if (historicEntitySnapshot == null)
			{
				obj = null;
			}
			else
			{
				List<string> list = historicEntitySnapshot.GetList("elements");
				if (list == null)
				{
					obj = null;
				}
				else
				{
					IEnumerable<string> enumerable = list.Where((string e) => e != AElement);
					obj = ((enumerable != null) ? enumerable.GetRandomElement() : null);
				}
			}
			if (obj == null)
			{
				obj = AElement;
			}
			return (string)obj;
		});
		string text4 = ((Group == "A") ? AFaction : text2);
		string element = ((Group == "A") ? AElement : text3);
		GameObject gameObject = GameObject.Create(text + text4 + " Cherub");
		gameObject.SetStringProperty("SpawnedFrom", ParentObject.Blueprint);
		gameObject.Render.RenderString = "\u008f";
		if (text == "Mechanical ")
		{
			gameObject.Render.RenderString = "\u008e";
		}
		gameObject.Render.DetailColor = "W";
		if (text == "Mechanical ")
		{
			gameObject.Render.DisplayName = gameObject.Render.DisplayName.Replace("mechanical ", "");
		}
		if (text == "Mechanical ")
		{
			string features = Grammar.InitCap(Grammar.MakeAndList(gameObject.GetxTag("TextFragments", "PoeticFeatures").Split(',').ToList()));
			ReplaceDescription(gameObject, BaseMechanicalDescription, features);
		}
		else
		{
			string features2 = "the " + string.Join(", the ", gameObject.GetxTag("TextFragments", "PoeticFeatures").Split(','));
			ReplaceDescription(gameObject, BaseDescription, features2);
		}
		if (text == "Mechanical ")
		{
			gameObject.RequirePart<HeatSelfOnFreeze>().HeatAmount = "50";
			gameObject.RequirePart<ReflectProjectiles>().Chance = 50;
		}
		else
		{
			gameObject.RequirePart<HeatSelfOnFreeze>();
			gameObject.RequirePart<ReflectProjectiles>();
		}
		if (text == "Mechanical ")
		{
			gameObject.RequirePart<Robot>().EMPable = false;
			gameObject.RequirePart<MentalShield>();
			gameObject.RequirePart<Metal>();
			if (!gameObject.HasPart<NightVision>())
			{
				gameObject.GetPart<Mutations>().AddMutation(new DarkVision(), 12);
			}
			Corpse part = gameObject.GetPart<Corpse>();
			if (part != null)
			{
				part.CorpseChance = 0;
				part.BurntCorpseChance = 0;
				part.VaporizedCorpseChance = 0;
			}
			gameObject.IsOrganic = false;
			gameObject.SetStringProperty("SeveredLimbBlueprint", "RobotLimb");
			gameObject.SetStringProperty("SeveredHeadBlueprint", "RobotHead7");
			gameObject.SetStringProperty("SeveredFaceBlueprint", "RobotFace");
			gameObject.SetStringProperty("SeveredArmBlueprint", "RobotArm");
			gameObject.SetStringProperty("SeveredHandBlueprint", "RobotHand");
			gameObject.SetStringProperty("SeveredLegBlueprint", "RobotLeg");
			gameObject.SetStringProperty("SeveredFootBlueprint", "RobotFoot");
			gameObject.SetStringProperty("SeveredFeetBlueprint", "RobotFeet");
			gameObject.SetStringProperty("SeveredTailBlueprint", "RobotTail");
			gameObject.SetStringProperty("SeveredRootsBlueprint", "RobotRoots");
			gameObject.SetStringProperty("SeveredFinBlueprint", "RobotFin");
			gameObject.SetIntProperty("Bleeds", 1);
			gameObject.SetStringProperty("BleedLiquid", "oil-1000");
			gameObject.SetStringProperty("BleedColor", "&K");
			gameObject.SetStringProperty("BleedPrefix", "&Koily");
			gameObject.Body?.CategorizeAll(7);
			gameObject.SetStringProperty("WaterRitualLiquid", "oil");
		}
		if (!bDynamic)
		{
			gameObject.Brain.Wanders = false;
			gameObject.Brain.WandersRandomly = false;
			gameObject.Brain.Allegiance.Clear();
			gameObject.Brain.Allegiance.Add("Cherubim", 100);
			gameObject.SetIntProperty("HelpModifier", 500);
			gameObject.SetIntProperty("CherubimLock", 1);
			gameObject.SetIntProperty("StaysOnZLevel", 1);
			gameObject.RequirePart<AIMarkOfDeathGuardian>();
		}
		BestowElement(gameObject, element);
		if (text == "Mechanical ")
		{
			gameObject.Render.DisplayName = "mechanical " + gameObject.Render.DisplayName;
		}
		E.ReplacementObject = gameObject;
		return base.HandleEvent(E);
	}

	public static void ReplaceDescription(GameObject Object, string Description, string Features)
	{
		string newValue = (Object.HasTag("AlternateCreatureType") ? Object.GetTag("AlternateCreatureType") : Object.Render.DisplayName.Substring(0, Object.Render.DisplayName.IndexOf(' ')));
		Object.GetPart<Description>()._Short = Description.Replace("*skin*", Object.GetxTag("TextFragments", "Skin")).Replace("*creatureType*", newValue).Replace("*features*", Features);
	}

	public static void BestowElement(GameObject Object, string Element, bool PrependName = true)
	{
		switch (Element)
		{
		case "glass":
			Object.AddPart<ReflectDamage>().ReflectPercentage = 25;
			Object.AddPart<ModGlazed>().Chance = 10;
			if (PrependName)
			{
				Object.Render.DisplayName = "glass " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&K";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of glass cherubim.\n• Attacks have a 10% chance to dismember.\n• Reflects 25% damage back at attackers.";
			break;
		case "jewels":
			Object.Statistics["Ego"].BaseValue += 10;
			Object.AddPart<ModTransmuteOnHit>().ChancePerThousand = 50;
			if (PrependName)
			{
				Object.Render.DisplayName = "jeweled " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&M";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of jeweled cherubim.\n• +10 Ego.\n• Attacks have a small chance to transmute opponents into gemstones.";
			break;
		case "stars":
			Object.RequirePart<Mutations>().AddMutation(new LightManipulation(), 10);
			if (PrependName)
			{
				Object.Render.DisplayName = "star " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&Y";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of star cherubim.\n• Light Manipulation 10";
			break;
		case "time":
			Object.RequirePart<Mutations>().AddMutation(new TemporalFugue(), 10);
			if (PrependName)
			{
				Object.Render.DisplayName = "time " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&b";
			Object.SetIntProperty("AIAbilityIgnoreCon", 1);
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of time cherubim.\n• Temporal Fugue 10";
			break;
		case "salt":
			Object.Statistics["Willpower"].BaseValue += 10;
			Object.Statistics["Hitpoints"].BaseValue *= 2;
			if (PrependName)
			{
				Object.Render.DisplayName = "salt " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&y";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of salt cherubim.\n• +10 Willpower\n• +100% HP";
			break;
		case "ice":
			Object.Statistics["ColdResistance"].BaseValue += 100;
			Object.RequirePart<Mutations>().AddMutation(new IceBreather(), 10);
			if (PrependName)
			{
				Object.Render.DisplayName = "ice " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&C";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of ice cherubim.\n• +100 Cold Resist\n• Ice Breath 10";
			break;
		case "scholarship":
		{
			Object.Statistics["Intelligence"].BaseValue += 10;
			ModBeetlehost modBeetlehost = Object.AddPart<ModBeetlehost>();
			modBeetlehost.Chance = 100;
			modBeetlehost.WorksOn(AdjacentCellContents: false, Carrier: false, CellContents: false, Enclosed: false, Equipper: false, Holder: false, Implantee: false, Inventory: false, Self: true);
			if (PrependName)
			{
				Object.Render.DisplayName = "learned " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&B";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of learned cherubim.\n• +10 Intelligence\n• Attacks discharge clockwork beetles.";
			break;
		}
		case "might":
			Object.Statistics["Strength"].BaseValue += 20;
			if (PrependName)
			{
				Object.Render.DisplayName = "mighty " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&r";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of mighty cherubim.\n• +20 Strength";
			break;
		case "chance":
		{
			ModBlinkEscape modBlinkEscape = Object.RequirePart<ModBlinkEscape>();
			modBlinkEscape.WorksOnEquipper = false;
			modBlinkEscape.WorksOnSelf = true;
			Object.RegisterPartEvent(modBlinkEscape, "BeforeApplyDamage");
			modBlinkEscape.Tier = 20;
			Object.AddPart<ModFatecaller>().Chance = 20;
			if (PrependName)
			{
				Object.Render.DisplayName = "chaotic " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&m";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of chaotic cherubim.\n• Whenever this creature is about to take damage, there's a 25% chance they blink away instead.\n• Whenever this creature attacks, 50% of the time the Fates have their way.";
			break;
		}
		case "circuitry":
			Object.Statistics["ElectricResistance"].BaseValue += 100;
			Object.RequirePart<Mutations>().AddMutation(new ElectricalGeneration(), 10);
			if (PrependName)
			{
				Object.Render.DisplayName = "electric " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&W";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of electric cherubim.\n• +100 Electrical Resist\n• Electrical Generation 10";
			break;
		case "travel":
			Object.Statistics["Speed"].BaseValue += 5;
			Object.RequirePart<Mutations>().AddMutation(new Teleportation(), 10);
			if (PrependName)
			{
				Object.Render.DisplayName = "quickened " + Object.Render.DisplayName;
			}
			Object.Render.ColorString = "&g";
			Object.AddPart<RulesDescription>().Text = "\nThis creature belongs to the caste of quickened cherubim.\n• +5 Quickness\n• Teleportation 10";
			break;
		}
	}
}
