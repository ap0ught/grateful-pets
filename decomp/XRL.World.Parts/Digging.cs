using System;
using XRL.UI;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public class Digging : IPart
{
	public const string SUPPORT_TYPE = "Digging";

	public static readonly string COMMAND_NAME = "CommandDig";

	public Guid ActivatedAbilityID = Guid.Empty;

	public override void Initialize()
	{
		ActivatedAbilityID = AddMyActivatedAbility("Dig", COMMAND_NAME, "Maneuvers", null, "â");
		base.Initialize();
	}

	public override void Remove()
	{
		RemoveMyActivatedAbility(ref ActivatedAbilityID);
		base.Remove();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<CommandEvent>.ID && ID != GetMovementCapabilitiesEvent.ID && ID != PooledEvent<NeedPartSupportEvent>.ID)
		{
			return ID == PooledEvent<ShouldAttackToReachTargetEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetMovementCapabilitiesEvent E)
	{
		E.Add("Dig To Square", COMMAND_NAME, 1500, MyActivatedAbility(ActivatedAbilityID));
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CommandEvent E)
	{
		if (E.Command == COMMAND_NAME)
		{
			if (ParentObject.OnWorldMap())
			{
				return ParentObject.Fail("You cannot dig on the world map.");
			}
			Cell cell = ParentObject.CurrentCell;
			Cell cell2 = PickDestinationCell(9999999, AllowVis.Any, Locked: false, IgnoreSolid: false, IgnoreLOS: false, RequireCombat: true, PickTarget.PickStyle.EmptyCell, "Dig to where?");
			if (cell2 != null && cell != null)
			{
				AutoAct.Setting = "d" + cell.X + "," + cell.Y + "," + cell2.X + "," + cell2.Y;
				ParentObject.ForfeitTurn();
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(NeedPartSupportEvent E)
	{
		if (E.Type == "Digging" && !PartSupportEvent.Check(E, this))
		{
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ShouldAttackToReachTargetEvent E)
	{
		if (!E.ShouldAttack && E.Actor == ParentObject && E.Object.IsDiggable() && OkayToDamageEvent.Check(E.Object, E.Actor))
		{
			E.ShouldAttack = true;
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}
}
