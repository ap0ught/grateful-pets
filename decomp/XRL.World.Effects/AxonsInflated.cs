using System;

namespace XRL.World.Effects;

[Serializable]
public class AxonsInflated : Effect, ITierInitialized
{
	public int Bonus;

	public string Source;

	public AxonsInflated()
	{
		DisplayName = "{{g|hyper-responsive}}";
	}

	public AxonsInflated(int Duration, int Bonus, GameObject Source)
		: this()
	{
		base.Duration = Duration;
		this.Bonus = Bonus;
		this.Source = Source.ID;
	}

	public void Initialize(int Tier)
	{
		Duration = 10;
		Bonus = 40;
	}

	public override int GetEffectType()
	{
		return 83894272;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return Bonus.Signed() + " Quickness\nWill become sluggish soon.";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyAxonsInflated", "Effect", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("The hurdles that separate the will and the way begin to collapse.", 'g');
		}
		StatShifter.SetStatShift("Speed", Bonus);
		Object?.PlayWorldSound("Sounds/StatusEffects/sfx_statusEffect_positiveVitality");
		return true;
	}

	public override void Remove(GameObject Object)
	{
		StatShifter.RemoveStatShifts();
		Object.ApplyEffect(new AxonsDeflated(10, 10, Source));
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
		Cell cell = base.Object?.CurrentCell;
		if (cell == null || cell.OnWorldMap())
		{
			Duration = 0;
		}
		else
		{
			GameObject Object = GameObject.FindByID(Source);
			if (!GameObject.Validate(ref Object) || Object.Implantee != base.Object)
			{
				Duration = 0;
			}
			else if (Duration > 0 && Duration != 9999)
			{
				Duration--;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0)
		{
			E.RenderEffectIndicator("\u0003", "Items/sw_axions.bmp", "&C", "C", 10, 50);
		}
		return true;
	}
}
