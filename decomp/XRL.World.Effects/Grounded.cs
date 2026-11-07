using System;
using XRL.Rules;
using XRL.World.Capabilities;
using XRL.World.Parts.Skill;

namespace XRL.World.Effects;

[Serializable]
public class Grounded : Effect, ITierInitialized
{
	public Grounded()
	{
		DisplayName = "{{w|grounded}}";
	}

	public Grounded(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public void Initialize(int Tier)
	{
		Duration = Stat.Random(100, 1000);
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override int GetEffectType()
	{
		return 83886336;
	}

	public override bool UseStandardDurationCountdown()
	{
		return true;
	}

	public override string GetDetails()
	{
		return "Cannot fly.";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == PooledEvent<CanChangeMovementModeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CanChangeMovementModeEvent E)
	{
		if (E.Object == Object && E.To == "Flying")
		{
			if (E.ShowMessage)
			{
				E.Object.Fail("You can't fly right now.");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.IsCreature)
		{
			return false;
		}
		if (!Object.FireEvent(Event.New("ApplyGrounded")) || !ApplyEffectEvent.Check(Object, "Grounded", this))
		{
			return false;
		}
		Flight.Fall(Object);
		Acrobatics_Jump.SyncAbility(Object, Silent: true);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		base.Remove(Object);
		Acrobatics_Jump.SyncAbility(Object, Silent: true);
	}
}
