using System;

namespace XRL.World.Effects;

[Serializable]
public class Overburdened : Effect
{
	public Overburdened()
	{
		DisplayName = "{{K|overburdened}}";
		Duration = 1;
	}

	public override string GetDetails()
	{
		return "Unable to move.";
	}

	public override bool Apply(GameObject Object)
	{
		if (Object.HasEffect<Overburdened>())
		{
			return false;
		}
		if (Object.IsPlayer() || Visible())
		{
			Object.ParticleText("*overburdened*", 'K');
		}
		return true;
	}

	public override int GetEffectType()
	{
		return 33554560;
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
				E.Object.Fail("You can't fly while overburdened.");
			}
			return false;
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("IsMobile");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "IsMobile" && Duration > 0 && !Object.IsTryingToJoinPartyLeader())
		{
			return false;
		}
		return base.FireEvent(E);
	}
}
