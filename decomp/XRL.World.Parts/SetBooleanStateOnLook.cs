using System;

namespace XRL.World.Parts;

[Serializable]
public class SetBooleanStateOnLook : IPart
{
	public string State;

	public bool LookedAt;

	public SetBooleanStateOnLook()
	{
	}

	public SetBooleanStateOnLook(string State)
		: this()
	{
		this.State = State;
	}

	public override bool SameAs(IPart Part)
	{
		if (Part is SetBooleanStateOnLook setBooleanStateOnLook)
		{
			return setBooleanStateOnLook.State == State;
		}
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AutoexploreObjectEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AutoexploreObjectEvent E)
	{
		if (!E.AutogetOnlyMode && E.Command != "Look" && !LookedAt && HasUnsetState())
		{
			E.Command = "Look";
		}
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterLookedAt");
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "AfterLookedAt" && !LookedAt)
		{
			The.Game.SetBooleanGameState(State, Value: true);
			LookedAt = true;
		}
		return base.FireEvent(E);
	}

	public bool HasUnsetState()
	{
		if (State != null)
		{
			return !The.Game.GetBooleanGameState(State);
		}
		return false;
	}
}
