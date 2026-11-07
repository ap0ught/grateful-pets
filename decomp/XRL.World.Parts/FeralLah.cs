using System;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class FeralLah : IPart
{
	public int FearChance = 100;

	public int FearCooldown;

	public int PodCooldown;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("AIAttackRange");
		Registrar.Register("BeforeTakeAction");
		Registrar.Register("BeginTakeAction");
		base.Register(Object, Registrar);
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeginTakeAction")
		{
			if (!ParentObject.IsFrozen() && PodCooldown > 0)
			{
				PodCooldown--;
			}
		}
		else if (E.ID == "BeforeTakeAction")
		{
			if (!ParentObject.IsNowhere() && !ParentObject.IsFrozen() && ParentObject.Target != null)
			{
				Cell randomElement = ParentObject.CurrentCell.GetEmptyAdjacentCells().GetRandomElement();
				if (randomElement != null && PodCooldown <= 0)
				{
					PodCooldown = Stat.Random(5, 7);
					GameObject gameObject = GameObject.Create("Feral Lah Pod");
					gameObject.PartyLeader = ParentObject;
					gameObject.Brain.TakeBaseAllegiance(ParentObject);
					gameObject.MakeActive();
					randomElement.AddObject(gameObject);
					ParentObject.Splatter("&g.");
					gameObject.ParticleBlip("&go", 10, 0L);
					ParentObject.UseEnergy(1000);
					return false;
				}
				if (ParentObject.Target.DistanceTo(ParentObject) <= 1)
				{
					Fear();
				}
			}
		}
		else if (E.ID == "AIAttackRange")
		{
			ParentObject.UseEnergy(1000);
			return false;
		}
		return base.FireEvent(E);
	}

	public void Fear()
	{
		FearCooldown--;
		if (FearCooldown < 0 && FearChance.in100())
		{
			FearCooldown = 20 + Stat.Roll("1d6");
			FearAura.PulseAura(ParentObject);
		}
	}
}
