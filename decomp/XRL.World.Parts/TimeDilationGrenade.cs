using System;
using XRL.Core;
using XRL.Rules;
using XRL.World.Parts.Mutation;

namespace XRL.World.Parts;

[Serializable]
public class TimeDilationGrenade : IGrenade
{
	public int Range = 9;

	public int Level = 1;

	public override bool SameAs(IPart p)
	{
		if ((p as TimeDilationGrenade).Level != Level)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != GetComponentNavigationWeightEvent.ID && ID != GetComponentAdjacentNavigationWeightEvent.ID)
		{
			return ID == PooledEvent<GetItemElementsEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetComponentNavigationWeightEvent E)
	{
		E.MinWeight(3);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetComponentAdjacentNavigationWeightEvent E)
	{
		E.MinWeight(2);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetItemElementsEvent E)
	{
		if (E.IsRelevantObject(ParentObject))
		{
			E.Add("time", 10);
		}
		return base.HandleEvent(E);
	}

	protected override bool DoDetonate(Cell C, GameObject Actor = null, GameObject ApparentTarget = null, bool Indirect = false)
	{
		PlayWorldSound(GetPropertyOrTag("DetonatedSound"), 1f, 0f, Combat: true);
		DidX("discorporate", null, "!");
		for (int i = 0; i < Stat.RandomCosmetic(1, 3); i++)
		{
			float num = (float)Stat.RandomCosmetic(4, 14) / 3f;
			for (int j = 0; j < 360; j++)
			{
				XRLCore.ParticleManager.Add("@", C.X, C.Y, (float)Math.Sin((double)(float)j * 0.017) / num, (float)Math.Cos((double)(float)j * 0.017) / num);
			}
		}
		TimeDilation.ApplyField(ParentObject, Range, Independent: true, 15, Level);
		ParentObject.Destroy(null, Silent: true);
		return true;
	}
}
