using System;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasAsh : IObjectGasBehavior
{
	public string GasType = "Ash";

	public int GasLevel = 1;

	public override bool SameAs(IPart p)
	{
		GasAsh gasAsh = p as GasAsh;
		if (gasAsh.GasType != GasType)
		{
			return false;
		}
		if (gasAsh.GasLevel != GasLevel)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID && ID != GetAdjacentNavigationWeightEvent.ID)
		{
			return ID == GetNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && !E.Unbreathing && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyAshPoison") && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 2 + 5, 70);
				}
			}
			else
			{
				E.MinWeight(30);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetAdjacentNavigationWeightEvent E)
	{
		if (!E.IgnoreGases && !E.Unbreathing && E.PhaseMatches(ParentObject))
		{
			if (E.Smart)
			{
				E.Uncacheable = true;
				if (CheckGasCanAffectEvent.Check(E.Actor, ParentObject) && (E.Actor == null || (E.Actor.FireEvent("CanApplyAshPoison") && E.Actor.PhaseMatches(ParentObject))))
				{
					E.MinWeight(GasDensityStepped() / 10 + 1, 14);
				}
			}
			else
			{
				E.MinWeight(6);
			}
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		UpdateOpacity();
		return base.HandleEvent(E);
	}

	public override bool ApplyGas(Cell Cell)
	{
		bool result = base.ApplyGas(Cell);
		UpdateOpacity();
		return result;
	}

	public override bool ApplyGas(GameObject Object)
	{
		if (Object == ParentObject)
		{
			return false;
		}
		if (!Object.Respires)
		{
			return false;
		}
		Gas baseGas = BaseGas;
		if (!CheckGasCanAffectEvent.Check(Object, ParentObject, baseGas))
		{
			return false;
		}
		if (!Object.FireEvent("CanApplyAshPoison"))
		{
			return false;
		}
		if (!CanApplyEffectEvent.Check<AshPoison>(Object))
		{
			return false;
		}
		if (!Object.PhaseMatches(ParentObject))
		{
			return false;
		}
		int num = GetRespiratoryAgentPerformanceEvent.GetFor(Object, ParentObject, baseGas);
		if (num <= 0)
		{
			return false;
		}
		Object.RemoveEffect<AshPoison>();
		AshPoison ashPoison = new AshPoison(Stat.Random(1, 10), baseGas.Creator);
		ashPoison.Damage = GasLevel * 2;
		Object.ApplyEffect(ashPoison);
		return Object.TakeDamage((int)Math.Max(Math.Floor((double)(num + 1) / 10.0), 1.0), "from %t {{K|choking ash}}!", "InhaleDanger Asphyxiation Gas", null, null, null, baseGas.Creator, null, null, null, Accidental: false, Environmental: true);
	}

	public void UpdateOpacity()
	{
		bool flag = GasDensity() >= 40;
		if (ParentObject.Render.Occluding != flag)
		{
			ParentObject.Render.Occluding = flag;
			ParentObject.CurrentCell?.ClearOccludeCache();
		}
	}
}
