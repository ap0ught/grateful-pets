using System;
using XRL.Names;
using XRL.World.Parts;
using XRL.World.Parts.Mutation;

namespace XRL.World;

public class HeroMaker
{
	public static readonly int ICON_COLOR_PRIORITY = 100;

	public static GameObject CreateHero(string BaseBlueprint)
	{
		return MakeHero(GameObject.Create(BaseBlueprint));
	}

	public static GameObject MakeHero(GameObject BaseCreature, string[] AdditionalBaseTemplates = null, string[] AdditionalSpecializationTemplates = null, int TierOverride = -1, string SpecialType = "Hero")
	{
		try
		{
			if (BaseCreature.GetIntProperty("Hero") > 0)
			{
				return BaseCreature;
			}
			if (!BaseCreature.HasPart<Brain>())
			{
				AnimateObject.Animate(BaseCreature);
			}
			string propertyOrTag = BaseCreature.GetPropertyOrTag("Role");
			BaseCreature.SetIntProperty("Hero", 1);
			BaseCreature.SetStringProperty("Role", "Hero");
			string text = ResolveTemplateTag(BaseCreature, "HeroNameColor", "M", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text2 = ResolveTemplateTag(BaseCreature, "HeroTileColor", "&M", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text3 = ResolveTemplateTag(BaseCreature, "HeroColorString", "&M", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text4 = ResolveTemplateTag(BaseCreature, "HeroDetailColor", "same", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (text2 == "same")
			{
				text2 = null;
			}
			if (text3 == "same")
			{
				text3 = null;
			}
			if (text4 == "same")
			{
				text4 = null;
			}
			if (!text2.IsNullOrEmpty() || !text3.IsNullOrEmpty() || !text4.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<HeroIconColor>().Configure(text3, text2.IsNullOrEmpty() ? text3 : text2, text4, null, null, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY, ICON_COLOR_PRIORITY);
			}
			if (BaseCreature.HasStat("Strength"))
			{
				BaseCreature.GetStat("Strength").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroStrBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Intelligence"))
			{
				BaseCreature.GetStat("Intelligence").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroIntBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Toughness"))
			{
				BaseCreature.GetStat("Toughness").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroTouBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Willpower"))
			{
				BaseCreature.GetStat("Willpower").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroWilBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Ego"))
			{
				BaseCreature.GetStat("Ego").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroEgoBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Agility"))
			{
				BaseCreature.GetStat("Agility").BoostStat(Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroAgiBoost", "1", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Hitpoints"))
			{
				BaseCreature.GetStat("Hitpoints").BaseValue = (int)((double)BaseCreature.GetStat("Hitpoints").BaseValue * Convert.ToDouble(ResolveTemplateTag(BaseCreature, "HeroHPBoost", "2", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("Level"))
			{
				BaseCreature.GetStat("Level").BaseValue = Math.Max((int)((double)BaseCreature.GetStat("Level").Value * Convert.ToDouble(ResolveTemplateTag(BaseCreature, "HeroLevelMultiplier", "1.5", AdditionalBaseTemplates, AdditionalSpecializationTemplates))), Convert.ToInt32(ResolveTemplateTag(BaseCreature, "HeroMinLevel", "0", AdditionalBaseTemplates, AdditionalSpecializationTemplates)));
			}
			if (BaseCreature.HasStat("XP") && BaseCreature.HasStat("Level"))
			{
				BaseCreature.GetStat("XP").BaseValue = Leveler.GetXPForLevel(BaseCreature.GetStat("Level").Value);
			}
			if (BaseCreature.HasStat("XPValue") && BaseCreature.HasStat("Level"))
			{
				float num = BaseCreature.GetStat("Level").Value;
				num /= 2f;
				if (propertyOrTag == "Minion")
				{
					BaseCreature.GetStat("XPValue").BaseValue = (int)(num * 100f);
				}
				else
				{
					BaseCreature.GetStat("XPValue").BaseValue = (int)(num * 200f);
				}
			}
			string text5 = ResolveTemplateTag(BaseCreature, "HeroSkills", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!text5.IsNullOrEmpty())
			{
				foreach (string item in text5.CachedCommaExpansion())
				{
					BaseCreature.AddSkill(item);
				}
			}
			string text6 = ResolveTemplateTag(BaseCreature, "HeroSelfPreservationThreshold", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!text6.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<AISelfPreservation>().Threshold = text6.RollCached();
			}
			string value = ResolveTemplateTag(BaseCreature, "SimpleConversation", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			string text7 = ResolveTemplateTag(BaseCreature, "HeroConversation", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!text7.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<ConversationScript>().ConversationID = text7;
			}
			else if (!value.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<ConversationScript>();
				BaseCreature.SetStringProperty("SimpleConversation", value);
			}
			int num2 = 0;
			int num3 = 0;
			string text8 = ResolveTemplateTag(BaseCreature, "HeroGenotype", "none", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (text8 != "True Kin" && !BaseCreature.HasTag("Robot"))
			{
				if (text8 != "Chimera")
				{
					num2 = ResolveTemplateTag(BaseCreature, "HeroMentalMutations", "0-2", AdditionalBaseTemplates, AdditionalSpecializationTemplates).RollCached();
				}
				if (text8 != "Esper")
				{
					num3 = ResolveTemplateTag(BaseCreature, "HeroPhysicalMutations", "0-2", AdditionalBaseTemplates, AdditionalSpecializationTemplates).RollCached();
				}
				if (text8 == "Chimera")
				{
					num3++;
				}
				else if (text8 == "Esper")
				{
					num2++;
				}
			}
			if (num2 > 0 || num3 > 0)
			{
				Mutations mutations = BaseCreature.RequirePart<Mutations>();
				for (int i = 0; i < num2; i++)
				{
					BaseMutation randomMutation;
					do
					{
						randomMutation = MutationFactory.GetRandomMutation("Mental");
					}
					while (randomMutation != null && mutations.HasMutation(randomMutation));
					if (randomMutation != null)
					{
						mutations.AddMutation(randomMutation, "1d4".RollCached());
					}
				}
				for (int j = 0; j < num3; j++)
				{
					BaseMutation randomMutation2;
					do
					{
						randomMutation2 = MutationFactory.GetRandomMutation("Physical");
					}
					while (randomMutation2 != null && mutations.HasMutation(randomMutation2));
					if (randomMutation2 != null)
					{
						mutations.AddMutation(randomMutation2, "1d4".RollCached());
					}
				}
			}
			string text9 = ResolveTemplateTag(BaseCreature, "HeroMutationPopulation", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!text9.IsNullOrEmpty())
			{
				BaseCreature.MutateFromPopulationTable(text9, (TierOverride == -1) ? BaseCreature.GetTier() : TierOverride);
			}
			string text10 = NameMaker.MakeHonorific(BaseCreature, null, null, null, null, null, null, null, null, null, SpecialType, null, SpecialFaildown: true);
			bool value2 = !text10.IsNullOrEmpty() || BaseCreature.HasHonorific;
			string text11 = NameMaker.MakeEpithet(BaseCreature, null, null, null, null, null, null, null, null, null, SpecialType, null, SpecialFaildown: true, value2);
			bool value3 = !text11.IsNullOrEmpty() || BaseCreature.HasEpithet;
			string text12 = NameMaker.MakeTitle(BaseCreature, null, null, null, null, null, null, null, null, null, SpecialType, null, SpecialFaildown: true, value2, value3);
			string text13 = NameMaker.MakeExtraTitle(BaseCreature, null, null, null, null, null, null, null, null, null, SpecialType, null, SpecialFaildown: true, value2, value3);
			if (!text10.IsNullOrEmpty())
			{
				if (!text11.IsNullOrEmpty() && text11.HasDelimitedSubstring(' ', text10, StringComparison.CurrentCultureIgnoreCase))
				{
					text11 = null;
				}
				if (!text12.IsNullOrEmpty() && text12.HasDelimitedSubstring(' ', text10, StringComparison.CurrentCultureIgnoreCase))
				{
					text12 = null;
				}
				if (!text13.IsNullOrEmpty() && text13.HasDelimitedSubstring(' ', text10, StringComparison.CurrentCultureIgnoreCase))
				{
					text13 = null;
				}
			}
			if (!text11.IsNullOrEmpty())
			{
				if (!text12.IsNullOrEmpty() && text12.HasDelimitedSubstring(' ', text11, StringComparison.CurrentCultureIgnoreCase))
				{
					text12 = null;
				}
				if (!text13.IsNullOrEmpty() && text13.HasDelimitedSubstring(' ', text11, StringComparison.CurrentCultureIgnoreCase))
				{
					text13 = null;
				}
			}
			BaseCreature.GiveProperName(null, Force: false, SpecialType, SpecialFaildown: true, value2, value3);
			if (!text10.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<Honorifics>().AddHonorific(text10, 40);
			}
			if (!text11.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<Epithets>().AddEpithet(text11, -40);
			}
			if (!text12.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<Titles>().AddTitle(text12, -40);
			}
			if (!text13.IsNullOrEmpty())
			{
				BaseCreature.RequirePart<Titles>().AddTitle(text13);
			}
			if (!text.IsNullOrEmpty() && text != "y" && text != "&y")
			{
				BaseCreature.RequirePart<DisplayNameColor>().SetColorByPriority(text, 30);
			}
			if (BaseCreature.HasPart<Inventory>())
			{
				int num4;
				if (TierOverride != -1)
				{
					num4 = TierOverride;
					BaseCreature.SetIntProperty("inventoryTier", num4);
				}
				else
				{
					num4 = BaseCreature.GetTier();
				}
				string mark = null;
				string markColor = null;
				if (BaseCreature.HasTag("HeroAddMakersMark"))
				{
					mark = MakersMark.Generate();
					markColor = ResolveTemplateTag(BaseCreature, "HeroMakersMarkColor", "R", AdditionalBaseTemplates, AdditionalSpecializationTemplates);
					if (markColor == "random")
					{
						markColor = (60.in100() ? Crayons.GetRandomColor() : Crayons.GetRandomDarkColor());
					}
					else if (markColor == "randombright")
					{
						markColor = Crayons.GetRandomColor();
					}
					HasMakersMark hasMakersMark = BaseCreature.RequirePart<HasMakersMark>();
					hasMakersMark.Mark = mark;
					hasMakersMark.Color = markColor;
				}
				string tag = BaseCreature.GetTag("HeroInventory");
				if (!tag.IsNullOrEmpty())
				{
					if (!mark.IsNullOrEmpty())
					{
						BaseCreature.EquipFromPopulationTable(tag, num4, (GameObject obj) =>
						{
							if (obj.HasPart<Description>() && !obj.HasTag("AlwaysStack"))
							{
								obj.RequirePart<MakersMark>().AddCrafter(BaseCreature, mark, markColor);
							}
						});
					}
					else
					{
						BaseCreature.EquipFromPopulationTable(tag, num4);
					}
				}
			}
			if (BaseCreature.HasTag("HeroHasGuards"))
			{
				BaseCreature.RemovePart<HasGuards>();
				HasGuards hasGuards = new HasGuards();
				hasGuards.NumberOfGuards = BaseCreature.GetTag("HeroHasGuards", "2-4");
				BaseCreature.AddPart(hasGuards);
			}
			if (BaseCreature.HasTag("HeroHasSlaves"))
			{
				string text14 = null;
				if (BaseCreature.TryGetPart<HasSlaves>(out var Part))
				{
					text14 = Part.SlaveTier;
					BaseCreature.RemovePart(Part);
				}
				Part = new HasSlaves();
				Part.NumberOfSlaves = BaseCreature.GetTag("HeroHasSlaves", "3-4");
				if (!text14.IsNullOrEmpty())
				{
					Part.SlaveTier = text14;
				}
				BaseCreature.AddPart(Part);
			}
			if (BaseCreature.HasTag("HeroHasThralls"))
			{
				BaseCreature.RemovePart<HasThralls>();
				HasThralls hasThralls = new HasThralls();
				hasThralls.NumberOfThralls = BaseCreature.GetTag("HeroHasThralls", "4-7");
				BaseCreature.AddPart(hasThralls);
			}
			if (!ResolveTemplateTag(BaseCreature, "HeroNoWaterRitual", "false", AdditionalBaseTemplates, AdditionalSpecializationTemplates).EqualsNoCase("true"))
			{
				BaseCreature.RemovePart<GivesRep>();
				BaseCreature.AddPart(new GivesRep());
				BaseCreature.FireEvent("FactionsAdded");
			}
			string text15 = ResolveTemplateTag(BaseCreature, "HeroFactionHeirloomChance", null, AdditionalBaseTemplates, AdditionalSpecializationTemplates);
			if (!text15.IsNullOrEmpty() && int.TryParse(text15, out var result) && result.in100())
			{
				string primaryFaction = BaseCreature.GetPrimaryFaction();
				if (!primaryFaction.IsNullOrEmpty())
				{
					Faction ifExists = Factions.GetIfExists(primaryFaction);
					if (ifExists != null)
					{
						BaseCreature.ReceiveObject(ifExists.GenerateHeirloom());
					}
				}
			}
			if (BaseCreature.Render != null)
			{
				BaseCreature.Render.RenderLayer++;
			}
			BaseCreature.FireEvent("MadeHero");
			return BaseCreature;
		}
		catch (Exception ex)
		{
			MetricsManager.LogError("Error making heroic: " + BaseCreature.Blueprint + " -> " + ex);
			return null;
		}
	}

	public static GameObject MakeHero(GameObject BaseCreature, string AdditionalSpecializationTemplate, int TierOverride = -1, string SpecialType = "Hero")
	{
		string[] additionalSpecializationTemplates = null;
		if (!AdditionalSpecializationTemplate.IsNullOrEmpty())
		{
			additionalSpecializationTemplates = new string[1] { AdditionalSpecializationTemplate };
		}
		return MakeHero(BaseCreature, null, additionalSpecializationTemplates, TierOverride, SpecialType);
	}

	public static GameObject MakeHero(GameObject BaseCreature, string AdditionalBaseTemplate, string AdditionalSpecializationTemplate, int TierOverride = -1, string SpecialType = "Hero")
	{
		string[] additionalBaseTemplates = null;
		string[] additionalSpecializationTemplates = null;
		if (!AdditionalBaseTemplate.IsNullOrEmpty())
		{
			additionalBaseTemplates = new string[1] { AdditionalBaseTemplate };
		}
		if (!AdditionalSpecializationTemplate.IsNullOrEmpty())
		{
			additionalSpecializationTemplates = new string[1] { AdditionalSpecializationTemplate };
		}
		return MakeHero(BaseCreature, additionalBaseTemplates, additionalSpecializationTemplates, TierOverride, SpecialType);
	}

	public static string ResolveTemplateTag(GameObject BaseCreature, string Tag, string Default = null, string[] AdditionalBaseTemplates = null, string[] AdditionalSpecializationTemplates = null)
	{
		string text = Default;
		if (AdditionalBaseTemplates != null)
		{
			string[] array = AdditionalBaseTemplates;
			foreach (string name in array)
			{
				GameObjectBlueprint blueprintIfExists = GameObjectFactory.Factory.GetBlueprintIfExists(name);
				if (blueprintIfExists != null)
				{
					text = blueprintIfExists.GetTag(Tag, text);
				}
			}
		}
		text = BaseCreature.GetTag(Tag, text);
		if (AdditionalSpecializationTemplates != null)
		{
			string[] array = AdditionalSpecializationTemplates;
			foreach (string name2 in array)
			{
				GameObjectBlueprint blueprintIfExists2 = GameObjectFactory.Factory.GetBlueprintIfExists(name2);
				if (blueprintIfExists2 != null)
				{
					text = blueprintIfExists2.GetTag(Tag, text);
				}
			}
		}
		return BaseCreature.GetStringProperty(Tag, text);
	}
}
