using System;

namespace XRL.World;

[GameEvent(Cascade = 17, Cache = Cache.Pool)]
public class GetModRarityWeightEvent : PooledEvent<GetModRarityWeightEvent>
{
	public new static readonly int CascadeLevel = 17;

	public GameObject Object;

	public ModEntry Mod;

	public int BaseWeight;

	public int LinearAdjustment;

	public double FactorAdjustment = 1.0;

	public override int GetCascadeLevel()
	{
		return CascadeLevel;
	}

	public override bool Dispatch(IEventHandler Handler)
	{
		return Handler.HandleEvent(this);
	}

	public override void Reset()
	{
		base.Reset();
		Object = null;
		Mod = null;
		BaseWeight = 0;
		LinearAdjustment = 0;
		FactorAdjustment = 1.0;
	}

	public static int GetFor(GameObject Object, ModEntry Mod, int BaseWeight)
	{
		bool flag = true;
		int num = 0;
		double num2 = 1.0;
		if (flag)
		{
			bool flag2 = GameObject.Validate(ref Object) && Object.HasRegisteredEvent("GetModRarityWeight");
			bool flag3 = The.Player != null && The.Player.HasRegisteredEvent("GetModRarityWeight");
			if (flag2 | flag3)
			{
				Event obj = Event.New("GetModRarityWeight");
				obj.SetParameter("Object", Object);
				obj.SetParameter("Mod", Mod);
				obj.SetParameter("BaseWeight", BaseWeight);
				obj.SetParameter("LinearAdjustment", num);
				obj.SetParameter("FactorAdjustment", num2);
				if ((flag & flag2) && GameObject.Validate(ref Object))
				{
					flag = Object.FireEvent(obj);
				}
				if ((flag & flag3) && The.Player != null)
				{
					flag = The.Player.FireEvent(obj);
				}
				num = obj.GetIntParameter("LinearAdjustment");
				num2 = (double)obj.GetParameter("FactorAdjustment");
			}
		}
		if (flag)
		{
			bool flag4 = GameObject.Validate(ref Object) && Object.WantEvent(PooledEvent<GetModRarityWeightEvent>.ID, CascadeLevel);
			bool flag5 = The.Player != null && The.Player.WantEvent(PooledEvent<GetModRarityWeightEvent>.ID, CascadeLevel);
			if (flag4 | flag5)
			{
				GetModRarityWeightEvent getModRarityWeightEvent = PooledEvent<GetModRarityWeightEvent>.FromPool();
				getModRarityWeightEvent.Object = Object;
				getModRarityWeightEvent.Mod = Mod;
				getModRarityWeightEvent.BaseWeight = BaseWeight;
				getModRarityWeightEvent.LinearAdjustment = num;
				getModRarityWeightEvent.FactorAdjustment = num2;
				if ((flag & flag4) && GameObject.Validate(ref Object))
				{
					flag = Object.HandleEvent(getModRarityWeightEvent);
				}
				if ((flag & flag5) && The.Player != null)
				{
					flag = The.Player.HandleEvent(getModRarityWeightEvent);
				}
				num = getModRarityWeightEvent.LinearAdjustment;
				num2 = getModRarityWeightEvent.FactorAdjustment;
			}
		}
		return (int)Math.Round((double)(BaseWeight + num) * num2);
	}
}
