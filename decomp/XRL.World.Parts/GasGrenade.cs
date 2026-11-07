using System;
using System.Collections.Generic;
using XRL.Rules;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class GasGrenade : IGrenade
{
	public string GasObject = "PoisonGas";

	public int Density = 20;

	public int Level = 1;

	public override bool SameAs(IPart p)
	{
		GasGrenade gasGrenade = p as GasGrenade;
		if (gasGrenade.GasObject != GasObject)
		{
			return false;
		}
		if (gasGrenade.Density != Density)
		{
			return false;
		}
		if (gasGrenade.Level != Level)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID)
		{
			return ID == GetComponentAdjacentNavigationWeightEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(5);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(5);
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, 0f, Combat: true);
		List<Cell> adjacentCells = C.GetAdjacentCells();
		adjacentCells.Add(C);
		bool flag = ParentObject.HasEffect<Phased>();
		bool flag2 = ParentObject.HasEffect<Omniphase>();
		Event obj = Event.New("CreatorModifyGas", "Gas", (object)null);
		foreach (Cell item in adjacentCells)
		{
			GameObject gameObject = GameObject.Create(GasObject);
			Gas part = gameObject.GetPart<Gas>();
			part.Creator = Actor;
			part.Density = Density;
			part.Level = Level;
			if (flag)
			{
				gameObject.ForceApplyEffect(new Phased(Stat.Random(23, 32)));
			}
			if (flag2)
			{
				gameObject.ForceApplyEffect(new Omniphase(Stat.Random(46, 64)));
			}
			obj.SetParameter("Gas", part);
			Actor?.FireEvent(obj);
			item.AddObject(gameObject);
		}
		DidX("explode", null, "!");
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
