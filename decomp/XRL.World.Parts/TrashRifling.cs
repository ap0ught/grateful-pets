using System;
using System.Collections.Generic;
using XRL.World.AI.GoalHandlers;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Parts;

[Serializable]
public class TrashRifling : IPart
{
	public const string SUPPORT_TYPE = "TrashRifling";

	public static readonly string COMMAND_NAME = "CommandToggleTrashRifling";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Rifle through Trash", COMMAND_NAME, "Skills", null, "%", null, Toggleable: true, DefaultToggleState: true, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: true);
		base.Initialize();
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<AIBoredEvent>.ID && ID != PooledEvent<CommandEvent>.ID && ID != AutoexploreObjectEvent.ID && ID != PooledEvent<NeedPartSupportEvent>.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		bool flag = ParentObject.HasPart<Customs_TrashDivining>();
		bool flag2 = ParentObject.HasPart<Tinkering_Scavenger>();
		if (flag2 & flag)
		{
			stats.Set("RiflingSkills", "Scavenger,TrashDivining");
		}
		else if (flag2)
		{
			stats.Set("RiflingSkills", "Scavenger");
		}
		else if (flag)
		{
			stats.Set("RiflingSkills", "TrashDivining");
		}
		stats.Set("Chance", 5);
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIBoredEvent E)
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell != null && ParentObject.Brain != null && IsMyActivatedAbilityAIUsable(ActivatedAbilityID) && IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			List<GameObject> list = Event.NewGameObjectList();
			foreach (GameObject item in cell.ParentZone.FastSquareVisibility(cell.X, cell.Y, 8, "Garbage", ParentObject))
			{
				if (ParentObject.HasLOSTo(item))
				{
					list.Add(item);
				}
			}
			if (list.Count > 1)
			{
				list.Sort((GameObject a, GameObject b) => ParentObject.DistanceTo(a).CompareTo(ParentObject.DistanceTo(b)));
			}
			GameObject randomElement = list.GetRandomElement();
			if (randomElement != null)
			{
				Cell cell2 = randomElement.CurrentCell;
				if (cell2 != null)
				{
					ParentObject.Brain.PushGoal(new MoveTo(cell2, careful: false, overridesCombat: false, 0, wandering: false, global: false, juggernaut: false, 3));
				}
			}
			else
			{
				ParentObject.Brain.Think("I can't find any trash.");
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			ToggleMyActivatedAbility(ActivatedAbilityID);
			AutoAct.ResetZoneAutoexploreAction(Garbage.RIFLE_INTERACTION);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "TrashRifling" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command == null && E.Item.HasPart<Garbage>() && IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			E.Command = Garbage.RIFLE_INTERACTION;
		}
		return base.HandleEvent(E);
	}

	public bool IsActive()
	{
		if (IsMyActivatedAbilityToggledOn(ActivatedAbilityID))
		{
			return IsMyActivatedAbilityVoluntarilyUsable(ActivatedAbilityID);
		}
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
