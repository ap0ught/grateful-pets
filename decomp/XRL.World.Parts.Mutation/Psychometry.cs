using System;
using System.Collections.Generic;
using System.Text;
using XRL.Language;
using XRL.UI;
using XRL.World.Effects;
using XRL.World.Parts.Skill;
using XRL.World.Tinkering;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class Psychometry : BaseMutation
{
	public static readonly string COMMAND_NAME = "Psychometry";

	public static readonly string MENU_COMMAND_NAME = "CommandPsychometryMenu";

	public bool RealityDistortionBased = true;

	public List<string> LearnedBlueprints;

	public Psychometry()
	{
		Type = "Mental";
	}

	public override void CollectStats(Templates.StatCollector stats, int Level)
	{
		bool flag = stats.mode.Contains("ability");
		stats.Set("IdentifyComplexity", GetIdentifiableComplexity(Level), !flag);
		stats.Set("ConstructComplexity", GetLearnableComplexity(Level), !flag);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != PooledEvent<GetItemElementsEvent>.ID && ID != PooledEvent<GetRitualSifrahSetupEvent>.ID && ID != GetTinkeringBonusEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == OwnerGetInventoryActionsEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetTinkeringBonusEvent E)
	{
		if (E.Type == "Examine" || E.Type == "ReverseEngineerTurns" || E.Type == "Hacking")
		{
			if (UsePsychometry(E.Actor, E.Item))
			{
				E.Bonus += 1 + Level / 10;
				E.SecondaryBonus += Level;
				E.PsychometryApplied = true;
			}
			else
			{
				if (E.Actor.IsInvalid() || E.Actor.HasEffect<Stun>())
				{
					return false;
				}
				if (Popup.ShowYesNo("Do you want to continue despite being unable to use Psychometry?") != DialogResult.Yes)
				{
					return false;
				}
			}
		}
		else if (E.Type == "Repair" || E.Type == "Disarming" || E.Type == "ItemModding")
		{
			if (UsePsychometry(E.Actor, E.Item))
			{
				E.SecondaryBonus += Level;
				E.PsychometryApplied = true;
			}
			else
			{
				if (!E.Actor.IsValid() || E.Actor.HasEffect<Stun>())
				{
					return false;
				}
				if (Popup.ShowYesNo("Do you want to continue despite being unable to use Psychometry?") != DialogResult.Yes)
				{
					return false;
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetRitualSifrahSetupEvent E)
	{
		if (E.Type == "Item Naming")
		{
			if (UsePsychometry(E.Actor, E.Subject))
			{
				E.PsychometryApplied = true;
			}
			else if (!E.Actor.IsValid() || E.Actor.HasEffect<Stun>())
			{
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(OwnerGetInventoryActionsEvent E)
	{
		if (E.Object.GetEpistemicStatus() != 2)
		{
			if (!E.Actor.IsConfused && !Options.SifrahExamine && E.Object.TryGetPart<Examiner>(out var Part) && Part.Complexity > 0)
			{
				E.AddAction("Psychometry", "read history with Psychometry", COMMAND_NAME, null, 'i', FireOnActor: true);
			}
		}
		else if (ParentObject.HasPart<XRL.World.Parts.Skill.Tinkering>())
		{
			string text = E.Object.GetPart<TinkerItem>()?.ActiveBlueprint ?? E.Object.Blueprint;
			if (!LearnedBlueprints.Contains(text))
			{
				bool flag = false;
				foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
				{
					if (tinkerRecipe.Blueprint == text)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					flag = false;
					foreach (TinkerData knownRecipe in TinkerData.KnownRecipes)
					{
						if (knownRecipe.Blueprint == text)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						E.AddAction("Psychometry", "read early history with Psychometry", COMMAND_NAME, null, 'i', FireOnActor: true);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (!Activate(E.Item))
			{
				return true;
			}
			Examiner part = E.Item.GetPart<Examiner>();
			TinkerItem part2 = E.Item.GetPart<TinkerItem>();
			ParentObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_psychometry_activate");
			if (!E.Item.Understood())
			{
				if (Options.SifrahExamine)
				{
					InventoryActionEvent.Check(E.Item, E.Actor, E.Item, "Examine");
					return true;
				}
				if (part.Complexity > GetIdentifiableComplexity())
				{
					Popup.ShowFail(E.Item.IndicativeProximal + (E.Item.IsPlural ? " artifacts" : " artifact") + E.Item.Is + " too complex for you to decipher " + E.Item.its + " function.");
					return true;
				}
				part.MakeUnderstood();
				Popup.ShowFail("You flush with understanding of the " + (E.Item.IsPlural ? "artifacts'" : "artifact's") + " past and determine " + E.Item.them + " to be " + E.Item.an(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ".");
				UseEnergy(1000, "Mental Mutation Psychometry");
			}
			else
			{
				if (Options.SifrahReverseEngineer && part2 != null && part2.CanDisassemble)
				{
					if (E.Actor.HasSkill("Tinkering_ReverseEngineer"))
					{
						Popup.ShowFail("You must disassemble " + E.Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " in order to unlock " + E.Item.its + " secrets.");
					}
					else
					{
						Popup.ShowFail("You must learn the way of the Reverse Engineer and disassemble " + E.Item.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + " in order to unlock " + E.Item.its + " secrets.");
					}
					return true;
				}
				if (part != null && part.Complexity > GetLearnableComplexity())
				{
					Popup.ShowFail(E.Item.IndicativeProximal + (E.Item.IsPlural ? " artifacts" : " artifact") + E.Item.Is + " too complex for you to decipher " + E.Item.its + " method of construction.");
					return true;
				}
				string text = part2?.ActiveBlueprint ?? E.Item.Blueprint;
				LearnedBlueprints.Add(text);
				foreach (TinkerData tinkerRecipe in TinkerData.TinkerRecipes)
				{
					if (tinkerRecipe.Blueprint == text)
					{
						GameObject gameObject = GameObject.CreateSample(tinkerRecipe.Blueprint);
						gameObject.MakeUnderstood();
						try
						{
							E.Actor.PlayWorldOrUISound("sfx_characterMod_tinkerSchematic_learn");
							tinkerRecipe.DisplayName = gameObject.DisplayNameOnlyDirect;
							Popup.Show("You abide the memory of " + ((E.Item.Count > 1) ? E.Item.a : E.Item.the) + Grammar.MakePossessive(E.Item.ShortDisplayNameSingle) + " creation. You learn to build " + (gameObject.IsPlural ? gameObject.ShortDisplayNameSingle : Grammar.Pluralize(gameObject.ShortDisplayNameSingle)) + ".");
							TinkerData.KnownRecipes.Add(tinkerRecipe);
						}
						finally
						{
							gameObject.Obliterate();
						}
						return true;
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == MENU_COMMAND_NAME)
		{
			return E.Actor.Fail("To use Psychometry, interact with an artifact and then choose 'read history with Psychometry.");
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("scholarship", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override string GetDescription()
	{
		return "You read the history of artifacts by touching them, learning what they do and how they were made.";
	}

	public static int GetIdentifiableComplexity(int Level)
	{
		return 4 + Level / 2;
	}

	public int GetIdentifiableComplexity()
	{
		return GetIdentifiableComplexity(Level);
	}

	public static int GetLearnableComplexity(int Level)
	{
		return 2 + (Level - 1) / 2;
	}

	public int GetLearnableComplexity()
	{
		return GetLearnableComplexity(Level);
	}

	public override string GetLevelText(int Level)
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		if (Options.SifrahExamine)
		{
			stringBuilder.Compound("Adds a bonus turn and is useful in many tinkering and some ritual Sifrah games.");
		}
		else
		{
			stringBuilder.Compound("Unerringly identify artifacts up to complexity tier {{rules|", "\n").Append(GetIdentifiableComplexity(Level)).Append("}}.");
			stringBuilder.Compound("Learn how to construct identified artifacts up to complexity tier {{rules|", "\n").Append(GetLearnableComplexity(Level)).Append("}} (must have the appropriate Tinker skill).");
		}
		stringBuilder.Compound("You may open security doors and use some secure devices by touching them.", "\n");
		return stringBuilder.ToString();
	}

	public override bool ChangeLevel(int NewLevel)
	{
		return base.ChangeLevel(NewLevel);
	}

	public override bool Mutate(GameObject GO, int Level)
	{
		ActivatedAbilityID = GO.AddActivatedAbility("Psychometry", MENU_COMMAND_NAME, "Mental Mutations", null, "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, RealityDistortionBased);
		if (LearnedBlueprints == null)
		{
			LearnedBlueprints = new List<string>();
		}
		return base.Mutate(GO, Level);
	}

	public override bool Unmutate(GameObject GO)
	{
		GO.RemoveActivatedAbility(ref ActivatedAbilityID);
		return base.Unmutate(GO);
	}

	public bool Advisable(GameObject Subject)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return false;
		}
		if (!RealityDistortionBased)
		{
			return true;
		}
		if (!IComponent<GameObject>.CheckRealityDistortionAdvisability(ParentObject, Subject.GetCurrentCell() ?? ParentObject.GetCurrentCell(), ParentObject, null, this))
		{
			return false;
		}
		return true;
	}

	public bool Activate(GameObject Subject)
	{
		if (!GameObject.Validate(ref Subject))
		{
			return false;
		}
		if (ParentObject.IsConfused)
		{
			return ParentObject.Fail("You strain to part the veil of time in order to use psychometry on " + Subject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", but you are too confused.");
		}
		if (!IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID))
		{
			if (IsMyActivatedAbilityCoolingDown(ActivatedAbilityID))
			{
				ParentObject.Fail("You strain to part the veil of time in order to use psychometry on " + Subject.t(int.MaxValue, null, null, AsIfKnown: false, Single: true) + ", but your psyche is too exhausted.");
			}
			return false;
		}
		if (!RealityDistortionBased)
		{
			return true;
		}
		Cell cell = Subject.GetCurrentCell() ?? ParentObject.GetCurrentCell();
		Event obj = Event.New("InitiateRealityDistortionTransit");
		obj.SetParameter("Object", ParentObject);
		obj.SetParameter("Mutation", this);
		obj.SetParameter("Cell", cell);
		obj.SetParameter("Purpose", "use psychometry on " + Subject.t());
		if (!ParentObject.FireEvent(obj))
		{
			return false;
		}
		if (!cell.FireEvent(obj))
		{
			return false;
		}
		return true;
	}
}
