using System;
using XRL.Core;

namespace XRL.World.Effects;

[Serializable]
public class NocturnalApexed : Effect, ITierInitialized
{
	public int MoveSpeedShift = -10;

	public int AgilityShift = 6;

	public string Source;

	public NocturnalApexed()
	{
		DisplayName = "{{r|prowling}}";
	}

	public NocturnalApexed(int Duration)
		: this()
	{
		base.Duration = Duration;
	}

	public NocturnalApexed(int Duration, int MoveSpeedShift, int AgilityShift, GameObject Source)
		: this(Duration)
	{
		this.MoveSpeedShift = MoveSpeedShift;
		this.AgilityShift = AgilityShift;
		this.Source = Source.ID;
	}

	public void Initialize(int Tier)
	{
		Duration = 100;
	}

	public override int GetEffectType()
	{
		return 83886084;
	}

	public override bool SameAs(Effect e)
	{
		return false;
	}

	public override string GetDetails()
	{
		return "+6 Agility\n+10 Move Speed";
	}

	public override bool Apply(GameObject Object)
	{
		if (!Object.FireEvent(Event.New("ApplyNocturnalApex", "Event", this)))
		{
			return false;
		}
		if (Object.IsPlayer())
		{
			IComponent<GameObject>.AddPlayerMessage("You start to prowl.", 'g');
		}
		ApplyChanges();
		return true;
	}

	private void ApplyChanges()
	{
		StatShifter.SetStatShift("MoveSpeed", MoveSpeedShift);
		StatShifter.SetStatShift("Agility", AgilityShift);
	}

	private void UnapplyChanges()
	{
		StatShifter.RemoveStatShifts();
	}

	public override void Remove(GameObject Object)
	{
		UnapplyChanges();
		base.Remove(Object);
	}

	public override bool Render(RenderEvent E)
	{
		if (Duration > 0 && XRLCore.CurrentFrame % 20 > 10)
		{
			E.RenderString = "\u0003";
			E.ColorString = "&K";
		}
		return true;
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

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AfterDeepCopyWithoutEffects");
		Registrar.Register("BeforeDeepCopyWithoutEffects");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeDeepCopyWithoutEffects")
		{
			UnapplyChanges();
		}
		else if (E.ID == "AfterDeepCopyWithoutEffects")
		{
			ApplyChanges();
		}
		return base.FireEvent(E);
	}
}
