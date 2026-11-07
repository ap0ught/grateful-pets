using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class StickyOnHit : IPart
{
	public string Duration = "12";

	public string SaveTarget = "25";

	public string SaveVs = "Entangle Stuck Restraint";

	public int MaxWeight = 1000;

	public override bool SameAs(IPart p)
	{
		StickyOnHit stickyOnHit = p as StickyOnHit;
		if (stickyOnHit.Duration != Duration)
		{
			return false;
		}
		if (stickyOnHit.SaveTarget != SaveTarget)
		{
			return false;
		}
		if (stickyOnHit.MaxWeight != MaxWeight)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == AfterThrownEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(AfterThrownEvent E)
	{
		Entangle();
		ParentObject.Destroy();
		return base.HandleEvent(E);
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("ProjectileHit");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "ProjectileHit")
		{
			Entangle(E.GetGameObjectParameter("Defender"));
		}
		return base.FireEvent(E);
	}

	public void Entangle(GameObject Target = null)
	{
		if (Target == null)
		{
			Cell cell = ParentObject.GetCurrentCell();
			if (cell != null)
			{
				Target = cell.GetCombatTarget(ParentObject, IgnoreFlight: true, IgnoreAttackable: false, IgnorePhase: false, 0, null, null, null, null, null, AllowInanimate: false);
			}
		}
		if (Target != null && Target.ApplyEffect(new Stuck(Duration.RollCached(), SaveTarget.RollCached(), SaveVs, ParentObject)))
		{
			IComponent<GameObject>.XDidYToZ(Target, "get", "entangled in", ParentObject, null, "!", null, null, null, Target);
		}
	}
}
