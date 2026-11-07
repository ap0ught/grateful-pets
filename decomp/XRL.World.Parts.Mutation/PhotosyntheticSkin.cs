using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.UI;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class PhotosyntheticSkin : BaseMutation
{
	public static readonly string COMMAND_NAME = "CommandBask";

	public static readonly int ICON_COLOR_PRIORITY = 80;

	public Guid BaskActivatedAbilityID = Guid.Empty;

	public string OldBleedLiquid;

	public string OldBleedColor;

	public string OldBleedPrefix;

	public int SoakCounter;

	[NonSerialized]
	public ActivatedAbilityEntry _Ability;

	private bool MutationColor = Options.MutationColor;

	public ActivatedAbilityEntry Ability
	{
		get
		{
			if (_Ability == null || _Ability.ID != BaskActivatedAbilityID)
			{
				_Ability = MyActivatedAbility(BaskActivatedAbilityID);
			}
			return _Ability;
		}
	}

	public bool HasSunlight
	{
		get
		{
			if (ParentObject.IsUnderSky())
			{
				return IsDay();
			}
			return false;
		}
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		stats.Set("DurationDay", GetBonusDurationString(Level), !stats.mode.Contains("ability"));
		stats.Set("BonusRegen", GetBonusRegeneration(Level) + "%", !stats.mode.Contains("ability"));
		stats.Set("BonusQuickness", GetBonusQuickness(Level), !stats.mode.Contains("ability"));
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != SingletonEvent<BeginTakeActionEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != EnteredCellEvent.ID && ID != SingletonEvent<BeforeAbilityManagerOpenEvent>.ID)
		{
			return ID == AIGetPassiveAbilityListEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AIGetPassiveAbilityListEvent E)
	{
		if (Ability.Enabled && !ParentObject.HasEffectDescendedFrom<ProceduralCookingEffect>())
		{
			E.Add(COMMAND_NAME);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(BaskActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckCamouflage();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		if (HasSunlight)
		{
			Ability.Enabled = true;
			if (!ParentObject.IsTemporary)
			{
				SoakCounter++;
				if (SoakCounter > 300)
				{
					SoakCounter = 0;
					int num = (Level - 1) / 4 + 1;
					Inventory inventory = ParentObject.Inventory;
					if (inventory != null)
					{
						if (inventory.Count("Starch") < num)
						{
							inventory.AddObject("Starch", Silent: true);
						}
						if (inventory.Count("Lignin") < num)
						{
							inventory.AddObject("Lignin", Silent: true);
						}
					}
				}
			}
		}
		else
		{
			Ability.Enabled = false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (ParentObject.AreHostilesNearby())
			{
				return ParentObject.Fail("You can't bask with hostiles nearby.");
			}
			if (!ParentObject.CanMoveExtremities("Bask", ShowMessage: true))
			{
				return false;
			}
			if (!HasSunlight)
			{
				return ParentObject.Fail("You need sunlight to bask in.");
			}
			ProceduralCookingEffect proceduralCookingEffect = ProceduralCookingEffect.CreateSpecific(new List<string> { "CookingDomainPhotosyntheticSkin_RegenerationUnit", "CookingDomainPhotosyntheticSkin_UnitQuickness", "CookingDomainPhotosyntheticSkin_SatedUnit" });
			ParentObject.FireEvent("ClearFoodEffects");
			ParentObject.CleanEffects();
			proceduralCookingEffect.Init(ParentObject);
			proceduralCookingEffect.Duration = 1200 * GetBonusDuration(Level);
			ParentObject.ApplyEffect(proceduralCookingEffect);
			ParentObject.GetPart<Stomach>()?.ClearHunger();
			ParentObject.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
			"=subject.T= =verb:bask= in the sunlight and =verb:absorb= the nourishing rays.".StartReplace().AddObject(ParentObject).EmitMessage(' ', FromDialog: true);
			if (ParentObject.IsPlayer())
			{
				Popup.Show("You start to metabolize the meal, gaining the following effect for the rest of the day:\n\n{{W|" + proceduralCookingEffect.GetDetails() + "}}");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void CheckCamouflage()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			if (cell.HasObjectWithPartOtherThan(typeof(PlantProperties), ParentObject))
			{
				StatShifter.DefaultDisplayName = "camouflage";
				StatShifter.SetStatShift("DV", GetBonusCamouflage(Level));
			}
			else
			{
				StatShifter.RemoveStatShifts();
			}
		}
	}

	public override string GetDescription()
	{
		return "You replenish yourself by absorbing sunlight through your hearty green skin.";
	}

	public override string GetLevelText(int Level)
	{
		string text = "";
		text = text + "You can bask in the sunlight instead of eating a meal to gain a special metabolizing effect for {{rules|" + GetBonusDurationString(Level) + "}}: +{{rules|" + GetBonusRegeneration(Level) + "%}} to natural healing rate and +{{rules|" + GetBonusQuickness(Level) + "}} Quickness\n";
		text = text + "While in the sunlight, you accrue starch and lignin that you can use as ingredients in meals you cook (max {{rules|" + GetStarchServings(Level) + "}} of each).\n";
		text = text + "+{{rules|" + GetBonusCamouflage(Level) + "}} DV while occupying the same space as foliage\n";
		return text + "+200 reputation with {{w|roots}}, {{w|trees}}, {{w|vines}}, and {{w|the Consortium of Phyta}}";
	}

	public static int GetBonusCamouflage(int Level)
	{
		return Math.Min((Level - 1) / 4 + 1, 6);
	}

	public static int GetBonusRegeneration(int Level)
	{
		return 20 + Level * 10;
	}

	public static int GetBonusQuickness(int Level)
	{
		return 13 + Level * 2;
	}

	public static string GetStarchServings(int Level)
	{
		return ((Level - 1) / 4 + 1).Things("serving");
	}

	public static int GetBonusDuration(int Level)
	{
		return (Level - 1) / 4 + 1;
	}

	public static string GetBonusDurationString(int Level)
	{
		int bonusDuration = GetBonusDuration(Level);
		if (bonusDuration != 1)
		{
			return bonusDuration + " days";
		}
		return bonusDuration + " day";
	}

	public override bool Render(RenderEvent E)
	{
		bool flag = true;
		if (ParentObject.IsPlayerControlled())
		{
			if ((XRLCore.FrameTimer.ElapsedMilliseconds & 0x7F) == 0L)
			{
				MutationColor = Options.MutationColor;
			}
			if (!MutationColor)
			{
				flag = false;
			}
		}
		if (flag)
		{
			E.ApplyColors("&g", ICON_COLOR_PRIORITY);
		}
		return base.Render(E);
	}

	public override bool ChangeLevel(int NewLevel)
	{
		CheckCamouflage();
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		BaskActivatedAbilityID = AddMyActivatedAbility("Bask", COMMAND_NAME, "Physical Mutations", null, "\u000f");
		OldBleedLiquid = ParentObject.GetStringProperty("BleedLiquid");
		OldBleedPrefix = ParentObject.GetStringProperty("BleedPrefix");
		OldBleedColor = ParentObject.GetStringProperty("BleedColor");
		ParentObject.SetStringProperty("BleedLiquid", "blood-500,sap-500");
		ParentObject.SetStringProperty("BleedPrefix", "{{r|bloody}} and {{Y|sugary}}");
		ParentObject.SetStringProperty("BleedColor", "&r");
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		RemoveMyActivatedAbility(ref BaskActivatedAbilityID);
		ParentObject.SetStringProperty("BleedLiquid", OldBleedLiquid, RemoveIfNull: true);
		ParentObject.SetStringProperty("BleedPrefix", OldBleedPrefix, RemoveIfNull: true);
		ParentObject.SetStringProperty("BleedColor", OldBleedColor, RemoveIfNull: true);
		StatShifter.RemoveStatShifts();
		return base.Unmutate(GO);
	}
}
