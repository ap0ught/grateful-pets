using System;
using System.Linq;
using XRL.Rules;
using XRL.World.AI;
using XRL.World.AI.GoalHandlers;

namespace XRL.World.Parts;

[Serializable]
public class GivesTreat : IPart
{
	public int Chance = 1500;

	public string TreatTable = "PetMushroomTreats";

	public override bool SameAs(IPart p)
	{
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("BeforeTakeAction");
		base.Register(Object, Registrar);
	}

	public bool ShouldFetch(GameObject o)
	{
		if (o.ShouldAutoget() && !o.IsOpenLiquidVolume() && o.GetCurrentCell() != null)
		{
			return !o.GetCurrentCell().IsSolid();
		}
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "BeforeTakeAction" && ParentObject.CurrentCell != null && Stat.Random(1, Chance) <= 1 && !ParentObject.Brain.Goals.Items.Any((GoalHandler i) => i.GetType() == typeof(GiveATreatToPartyLeader)))
		{
			ParentObject.Brain.PushGoal(new GiveATreatToPartyLeader(TreatTable));
			ParentObject.UseEnergy(1000);
			return false;
		}
		return base.FireEvent(E);
	}
}
