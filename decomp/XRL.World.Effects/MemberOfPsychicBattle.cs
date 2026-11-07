using System;
using System.Text;

namespace XRL.World.Effects;

[Serializable]
public class MemberOfPsychicBattle : Effect
{
	public GameObjectReference other;

	public int turnsRemaining = 10;

	public bool isAttacker;

	public MemberOfPsychicBattle()
	{
		Duration = 9999;
	}

	public MemberOfPsychicBattle(GameObject other, bool isAttacker)
		: this()
	{
		this.other = other.Reference();
		this.isAttacker = isAttacker;
	}

	public override int GetEffectType()
	{
		int num = 32770;
		if (!isAttacker)
		{
			num |= 0x2000000;
		}
		return num;
	}

	public override string GetDescription()
	{
		return "locked in psychic battle";
	}

	public override string GetDetails()
	{
		StringBuilder stringBuilder = Event.NewStringBuilder();
		stringBuilder.Append("Locked in psychic battle with a foe. (").Append(turnsRemaining.Things("turn")).Append(" remaining)");
		if (turnsRemaining <= 5 && Object.CanHaveNosebleed())
		{
			stringBuilder.Append("\nHas a bloody nose.");
		}
		return stringBuilder.ToString();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != BeforeRenderEvent.ID && ID != SingletonEvent<BeforeTakeActionEvent>.ID)
		{
			return ID == SingletonEvent<EndTurnEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(EndTurnEvent E)
	{
		if (IsInvalid())
		{
			Object?.RemoveEffect(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeTakeActionEvent E)
	{
		if (IsInvalid())
		{
			Object.RemoveEffect(this);
		}
		if (Object != null && !Object.IsPlayer() && isAttacker)
		{
			Object.PassTurn();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(BeforeRenderEvent E)
	{
		if (other.Object == null)
		{
			Object.RemoveEffect(this);
			return true;
		}
		Cell cell = Object.CurrentCell;
		if (cell != null && (Object.IsPlayer() || other.Object.IsPlayer()))
		{
			cell.ParentZone?.AddExplored(cell.X, cell.Y, 0);
			cell.ParentZone?.AddLight(cell.X, cell.Y, 0, LightLevel.Omniscient);
		}
		return base.HandleEvent(E);
	}

	public bool IsInvalid()
	{
		GameObject gameObject = other?.Object;
		if (gameObject == null)
		{
			return true;
		}
		if (gameObject.IsInvalid())
		{
			return true;
		}
		if (!gameObject.InSameZone(Object))
		{
			return true;
		}
		if (!gameObject.HasEffect<MemberOfPsychicBattle>())
		{
			return true;
		}
		if (isAttacker && !Object.IsPlayer() && Object.IsAlliedTowards(gameObject))
		{
			return true;
		}
		return false;
	}
}
