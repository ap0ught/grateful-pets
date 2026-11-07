using System;
using XRL.World.Parts.Mutation;

namespace XRL.World.Effects;

[Serializable]
public class TimeDilated : ITimeDilated
{
	public GameObject Mutant;

	public TimeDilated()
	{
	}

	public TimeDilated(GameObject Mutant)
		: this()
	{
		this.Mutant = Mutant;
	}

	public override bool DoTimeDilationVisualEffects()
	{
		TimeDilation part = Mutant.GetPart<TimeDilation>();
		if (part != null)
		{
			return part.Duration > 0;
		}
		return false;
	}

	public override bool Apply(GameObject Object)
	{
		bool flag = base.Apply(Object);
		if (flag)
		{
			Sync();
		}
		return flag;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("EnteredCell");
		Registrar.Register("EndTurn");
		base.Register(Object, Registrar);
	}

	private bool SyncInner()
	{
		if (!GameObject.Validate(ref Mutant))
		{
			return false;
		}
		if (!GameObject.Validate(Mutant) || !GameObject.Validate(Object))
		{
			return false;
		}
		TimeDilation part = Mutant.GetPart<TimeDilation>();
		if (part == null || part.Duration <= 0)
		{
			return false;
		}
		if (Duration > 0)
		{
			double num = part.ParentObject.RealDistanceTo(Object);
			if (num > (double)part.Range)
			{
				return false;
			}
			double num2 = TimeDilation.CalculateQuicknessPenaltyMultiplier(num, part.Range, part.Level);
			UnapplyChanges();
			SpeedPenalty = Math.Max((int)((double)Object.BaseStat("Speed") * num2), 1);
			ApplyChanges();
		}
		return true;
	}

	public void Sync()
	{
		if (!SyncInner())
		{
			Duration = 0;
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EndTurn" || E.ID == "EnteredCell")
		{
			Sync();
		}
		return base.FireEvent(E);
	}
}
