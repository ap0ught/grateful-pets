using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class AxonsDeflated : Effect, ITierInitialized
{
	public int Penalty;

	public string Source;

	public AxonsDeflated()
	{
		DisplayName = "{{r|sluggish}}";
	}

	public AxonsDeflated(int Duration, int Penalty, string Source)
		: this()
	{
		base.Duration = Duration;
		this.Penalty = Penalty;
		this.Source = Source;
	}

	public void Initialize(int Tier)
	{
		Duration = 10;
		Penalty = 10;
	}

	public override int GetEffectType()
	{
		return 117448704;
	}

	public override string GetDetails()
	{
		return "-" + Penalty + " Quickness";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyAxonsDeflated", "Effect", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You start to feel sluggish.", 'r');
		}
		StatShifter.SetStatShift("Speed", -Penalty);
		return true;
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == SingletonEvent<BeginTakeActionEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeginTakeActionEvent E)
	{
		Cell cell = Object?.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			Duration = 0;
		}
		else if (Duration > 0 && Duration != 9999)
		{
			Duration--;
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "\u0003";
			E.ColorString = "&r";
		}
		return true;
	}
}
