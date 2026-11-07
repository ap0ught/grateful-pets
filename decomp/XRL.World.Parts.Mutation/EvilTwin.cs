using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.UI;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class EvilTwin : BaseMutation
{
	public static readonly int CHANCE_PER_ZONE_NON_LEVEL_BASED = 5;

	public static readonly int CHANCE_PER_ZONE_LEVEL_BASED = 25;

	public static readonly int BASE_CHANCE_PER_LEVEL_LEVEL_BASED = 20;

	public static readonly int CHANCE_PER_LEVEL_LEVEL_BASED = 2;

	public static readonly string DEFECT_PREFIX = "Evil";

	public static readonly string DEFECT_MESSAGE = "{{c|You sense a sinister presence nearby.}}";

	public static readonly string DEFECT_DESCRIPTION = "It's evil you.";

	public int Due;

	public Dictionary<string, bool> Visited = new Dictionary<string, bool>();

	public EvilTwin()
	{
		Type = "Mental";
	}

	public override bool CanLevel()
	{
		return false;
	}

	public override string GetDescription()
	{
		return "Acting on some inscrutable impulse, a parallel version of yourself travels through space and time to destroy you.\n\nEach time you embark on a new location, there's a small chance your evil twin has tracked you there and attempts to kill you.";
	}

	public override string GetLevelText(int Level)
	{
		return "";
	}

	private static void ApplyHostility(GameObject Actor, Brain Brain, int Depth)
	{
		if (Actor != null && Depth < 100)
		{
			Brain.AddOpinion<OpinionInscrutable>(Actor);
			ApplyHostility(Actor.PartyLeader, Brain, Depth + 1);
			if (Actor.TryGetEffect<Dominated>(out var Effect))
			{
				ApplyHostility(Effect.Dominator, Brain, Depth + 1);
			}
		}
	}

	public static bool CreateEvilTwin(GameObject Original, string Prefix, Cell TargetCell = null, string Message = null, string ColorString = "&K", GameObject Actor = null, string MessageForActor = null, bool MakeExtras = true, string Description = null, string Factions = null)
	{
		if (Actor == null)
		{
			Actor = Original;
		}
		string text = "Twin";
		if (!Prefix.IsNullOrEmpty())
		{
			text = Prefix + (Prefix.EndsWith("-") ? "" : " ") + text;
		}
		if (!Original.CanBeReplicated(Actor, text, Temporary: true))
		{
			return false;
		}
		if (Factions == null)
		{
			Factions = (Original.IsPlayer() ? "Entropic-100,Mean-100,Playerhater-99" : "Entropic-100,Mean-100");
		}
		GameObject gameObject = Original.DeepCopy();
		if (gameObject.HasPart<EvilTwin>())
		{
			Mutations part = gameObject.GetPart<Mutations>();
			part.RemoveMutation(part.GetMutation("EvilTwin"));
			gameObject.RemovePart<EvilTwin>();
		}
		if (Original.IsPlayer())
		{
			gameObject.SetStringProperty("PlayerCopy", "true");
		}
		gameObject.SetStringProperty("EvilTwin", "true");
		gameObject.SetIntProperty("Entropic", 1);
		Brain brain = gameObject.Brain;
		brain.PartyLeader = null;
		brain.Hibernating = false;
		brain.Staying = false;
		brain.Passive = false;
		brain.Factions = Factions;
		brain.Allegiance.Hostile = true;
		brain.Allegiance.Calm = false;
		if (Actor != null)
		{
			brain.AddOpinion<OpinionMollify>(Actor);
			Actor.AddOpinion<OpinionMollify>(gameObject);
		}
		if (!Prefix.IsNullOrEmpty())
		{
			gameObject.DisplayName = Prefix + (Prefix.EndsWith("-") ? "" : " ") + gameObject.DisplayNameOnlyDirect;
		}
		if (!Description.IsNullOrEmpty() && gameObject.TryGetPart<Description>(out var Part) && Part._Short == "It's you.")
		{
			Part.Short = Description;
		}
		Event obj = Event.New("EvilTwinAttitudeSetup");
		obj.SetParameter("Original", Original);
		obj.SetParameter("Twin", gameObject);
		obj.SetParameter("Actor", Actor);
		if (Original.FireEvent(obj))
		{
			brain.PushGoal(new Kill(Original));
			ApplyHostility(Original, brain, 0);
		}
		Zone currentZone = Original.CurrentZone;
		gameObject.Render.ColorString = ColorString;
		Temporary.AddHierarchically(gameObject);
		gameObject.Brain?.PerformEquip();
		if (TargetCell == null)
		{
			int num = 1000;
			while (num > 0)
			{
				num--;
				int x = Stat.Random(0, currentZone.Width - 1);
				int y = Stat.Random(0, currentZone.Height - 1);
				Cell cell = currentZone.GetCell(x, y);
				if (cell.IsEmpty())
				{
					TargetCell = cell;
					break;
				}
			}
		}
		if (TargetCell == null)
		{
			gameObject.Obliterate();
			return false;
		}
		if (Original.IsPlayer())
		{
			IComponent<GameObject>.PlayUISound("Sounds/Abilities/sfx_ability_mutation_evilTwin_spawn");
		}
		gameObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_evilTwin_spawn");
		TargetCell.AddObject(gameObject);
		gameObject.MakeActive();
		if (MakeExtras)
		{
			if (Actor.GetIntProperty("MentalCloneMultiplier") > 0)
			{
				for (int i = 0; i < ((Actor != null) ? new int?(Actor.GetIntProperty("MentalCloneMultiplier") - 1) : ((int?)null)); i++)
				{
					CreateEvilTwin(Original, Prefix, TargetCell, null, ColorString, Actor, null, MakeExtras: false, Description);
				}
			}
			if (Original.IsPlayer())
			{
				IComponent<GameObject>.PlayUISound("Sounds/Abilities/sfx_ability_mutation_evilTwin_spawn");
			}
			gameObject?.PlayWorldSound("Sounds/Abilities/sfx_ability_mutation_evilTwin_spawn");
			WasReplicatedEvent.Send(Original, Actor, gameObject, text, Temporary: true);
			ReplicaCreatedEvent.Send(gameObject, Actor, Original, text, Temporary: true);
			if (!Message.IsNullOrEmpty() && Original.IsPlayer())
			{
				Popup.Show(Message);
			}
			if (!MessageForActor.IsNullOrEmpty() && Actor.IsPlayer())
			{
				Popup.Show(MessageForActor);
			}
		}
		return true;
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (!base.WantEvent(ID, Cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (!E.Object.OnWorldMap())
		{
			string text = E.Object.CurrentZone?.ZoneID;
			if (!text.IsNullOrEmpty() && !Visited.ContainsKey(text))
			{
				Visited[text] = true;
				if (!E.Object.DisplayName.Contains("[Creature]"))
				{
					if (GlobalConfig.GetBoolSetting("EvilTwinLevelBased"))
					{
						if (Due > 0 && CHANCE_PER_ZONE_LEVEL_BASED.in100() && CreateEvilTwin(ParentObject, DEFECT_PREFIX, null, DEFECT_MESSAGE, "&K", null, null, MakeExtras: true, DEFECT_DESCRIPTION))
						{
							Due--;
						}
					}
					else if (CHANCE_PER_ZONE_NON_LEVEL_BASED.in100())
					{
						CreateEvilTwin(ParentObject, DEFECT_PREFIX, null, DEFECT_MESSAGE, "&K", null, null, MakeExtras: true, DEFECT_DESCRIPTION);
					}
				}
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantCreature(ParentObject))
		{
			E.Add("glass", BaseElementWeight);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterLevelGainedEvent E)
	{
		if (E.Actor.IsPlayer() && GlobalConfig.GetBoolSetting("EvilTwinLevelBased") && (BASE_CHANCE_PER_LEVEL_LEVEL_BASED + CHANCE_PER_LEVEL_LEVEL_BASED * ParentObject.Stat("Level")).in100())
		{
			Due++;
		}
		return base.HandleEvent(E);
	}

	public override void RegisterActive(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register(The.Game, PooledEvent<AfterLevelGainedEvent>.ID);
	}
}
