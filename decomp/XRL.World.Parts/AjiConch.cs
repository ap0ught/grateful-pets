using System;
using System.Collections.Generic;

namespace XRL.World.Parts;

[Serializable]
public class AjiConch : IPart
{
	public const string ABILITY_NAME = "Blow Aji Conch";

	public const string COMMAND_NAME = "ActivateAjiConch";

	public const int CONE_LENGTH = 4;

	public const int CONE_ANGLE = 30;

	public const int GAS_DENSITY = 800;

	public const int GAS_LEVEL = 5;

	public string CommandID;

	public Guid ActivatedAbilityID = Guid.Empty;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public void CollectStats(Templates.StatCollector stats)
	{
		stats.CollectCooldownTurns(MyActivatedAbility(ActivatedAbilityID, ParentObject.Equipped), GetCooldown());
	}

	public int GetCooldown()
	{
		return 150;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AIGetOffensiveItemListEvent.ID && ID != PooledEvent<CommandEvent>.ID && ID != EquippedEvent.ID && ID != UnequippedEvent.ID && ID != GetInventoryActionsEvent.ID && ID != InventoryActionEvent.ID)
		{
			return ID == SingletonEvent<BeforeAbilityManagerOpenEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeAbilityManagerOpenEvent E)
	{
		DescribeMyActivatedAbility(ActivatedAbilityID, CollectStats, ParentObject.Equipped);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AIGetOffensiveItemListEvent E)
	{
		if (E.Actor == ParentObject.Equipped && E.Actor.IsActivatedAbilityAIUsable(ActivatedAbilityID) && E.Distance <= 4 && !CommandID.IsNullOrEmpty())
		{
			E.Add("ActivateAjiConch", 1, ParentObject, Inv: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		if (ParentObject.IsEquippedProperly(E.Part))
		{
			ActivatedAbilityID = E.Actor.AddDynamicCommand(out CommandID, "ActivateAjiConch", "Blow Aji Conch", "Items", null, "\a", null, Toggleable: false, DefaultToggleState: false, ActiveToggle: false, IsAttack: false, IsRealityDistortionBased: false, IsWorldMapUsable: false, Silent: false, AIDisable: false, AlwaysAllowToggleOff: true, AffectedByWillpower: true, TickPerTurn: false, Distinct: true);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.RemoveActivatedAbility(ref ActivatedAbilityID);
		E.Actor.UnregisterPartEvent(this, CommandID);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == CommandID)
		{
			ActivateAjiConch();
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetInventoryActionsEvent E)
	{
		if (E.Actor == ParentObject.Equipped && E.Actor.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			E.AddAction("Blow", "blow", "ActivateAjiConch", null, 'b', FireOnActor: false, 20);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(InventoryActionEvent E)
	{
		if (E.Command == "ActivateAjiConch")
		{
			ActivateAjiConch();
			E.RequestInterfaceExit();
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private void ActivateAjiConch()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null || !equipped.IsActivatedAbilityUsable(ActivatedAbilityID))
		{
			return;
		}
		string text = PopulationManager.GenerateOne("DynamicObjectsTable:AjiConch")?.Blueprint;
		if (text.IsNullOrEmpty())
		{
			return;
		}
		List<Cell> list = equipped.Physics.PickCone(4, 30, AllowVis.Any, null, "Blow Aji Conch");
		if (list == null)
		{
			return;
		}
		GameObjectBlueprint blueprint = GameObjectFactory.Factory.GetBlueprint(text);
		bool flag = blueprint.GetTag("GasGenerationAddSeeping").EqualsNoCase("true") || blueprint.GetPartParameter("Gas", "Seeping", Default: false);
		if (!flag)
		{
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].IsSolid())
				{
					list.RemoveAt(num);
				}
			}
		}
		if (list.Count == 0)
		{
			list.Add(equipped.CurrentCell);
		}
		equipped.CooldownActivatedAbility(ActivatedAbilityID, GetCooldown());
		equipped.PlayWorldSound("Sounds/Interact/sfx_interact_conch_blow");
		IComponent<GameObject>.XDidY(equipped, "blow", "into the conch of the Aji", null, null, null, equipped, null, UseFullNames: false, IndefiniteSubject: false, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
		Event obj = Event.New("CreatorModifyGas", "Gas", (object)null);
		foreach (Cell item in list)
		{
			GameObject gameObject = GameObjectFactory.Factory.CreateObject(blueprint);
			Gas part = gameObject.GetPart<Gas>();
			if (flag)
			{
				part.Seeping = true;
			}
			part.Creator = equipped;
			part.Density = 800 / list.Count;
			part.Level = 5;
			obj.SetParameter("Gas", part);
			equipped.FireEvent(obj);
			item.AddObject(gameObject);
			The.Core.RenderDelay(25, Interruptible: false);
		}
	}
}
