using System;
using System.Collections.Generic;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LiquidProximityTriggersEffect : IPart
{
	public string LiquidID = "blood";

	public int Range = 10;

	public bool LiquidMustBeConnectedByLiquid = true;

	public string EffectName = "Frenzied";

	public string Duration = "20";

	public bool Stack;

	public override bool SameAs(IPart p)
	{
		LiquidProximityTriggersEffect liquidProximityTriggersEffect = p as LiquidProximityTriggersEffect;
		if (liquidProximityTriggersEffect.LiquidID != LiquidID)
		{
			return false;
		}
		if (liquidProximityTriggersEffect.Range != Range)
		{
			return false;
		}
		if (liquidProximityTriggersEffect.LiquidMustBeConnectedByLiquid != LiquidMustBeConnectedByLiquid)
		{
			return false;
		}
		if (liquidProximityTriggersEffect.EffectName != EffectName)
		{
			return false;
		}
		if (liquidProximityTriggersEffect.Duration != Duration)
		{
			return false;
		}
		if (liquidProximityTriggersEffect.Stack != Stack)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckForEffectTrigger();
	}

	public bool CheckForEffectTrigger()
	{
		if (!Stack && ParentObject.HasEffect(EffectName))
		{
			return false;
		}
		if (!IsLiquidProximate())
		{
			return false;
		}
		TriggerEffect();
		return true;
	}

	public bool IsLiquidProximate()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return false;
		}
		GameObject openLiquidVolume = cell.GetOpenLiquidVolume();
		if (LiquidMustBeConnectedByLiquid && openLiquidVolume == null)
		{
			return false;
		}
		if (openLiquidVolume != null && openLiquidVolume.LiquidVolume?.ContainsLiquid(LiquidID) == true)
		{
			return true;
		}
		int i = 0;
		for (int count = cell.Objects.Count; i < count; i++)
		{
			LiquidCovered effect = cell.Objects[i].GetEffect<LiquidCovered>();
			if (effect != null)
			{
				LiquidVolume liquid = effect.Liquid;
				if (liquid != null && liquid.ContainsLiquid(LiquidID))
				{
					return true;
				}
			}
		}
		List<Cell> list = Event.NewCellList();
		List<Cell> list2 = Event.NewCellList();
		List<Cell> list3 = Event.NewCellList();
		list.Add(cell);
		list2.AddRange(cell.GetLocalAdjacentCells());
		for (int j = 1; j <= Range; j++)
		{
			list3.Clear();
			list3.AddRange(list2);
			list.AddRange(list3);
			list2.Clear();
			int k = 0;
			for (int count2 = list3.Count; k < count2; k++)
			{
				Cell cell2 = list3[k];
				openLiquidVolume = cell2.GetOpenLiquidVolume();
				if (LiquidMustBeConnectedByLiquid && openLiquidVolume == null)
				{
					continue;
				}
				if (openLiquidVolume != null && openLiquidVolume.LiquidVolume?.ContainsLiquid(LiquidID) == true)
				{
					return true;
				}
				int l = 0;
				for (int count3 = cell2.Objects.Count; l < count3; l++)
				{
					LiquidCovered effect2 = cell2.Objects[l].GetEffect<LiquidCovered>();
					if (effect2 != null)
					{
						LiquidVolume liquid2 = effect2.Liquid;
						if (liquid2 != null && liquid2.ContainsLiquid(LiquidID))
						{
							return true;
						}
					}
				}
				foreach (Cell localAdjacentCell in cell2.GetLocalAdjacentCells())
				{
					if (!list.Contains(localAdjacentCell) && !list2.Contains(localAdjacentCell))
					{
						list2.Add(localAdjacentCell);
					}
				}
			}
		}
		return false;
	}

	public bool TriggerEffect()
	{
		Type type = ModManager.ResolveType("XRL.World.Effects." + EffectName);
		if (type == null)
		{
			MetricsManager.LogError("tried to trigger nonexistent effect " + EffectName);
			return false;
		}
		if (!(Activator.CreateInstance(type) is Effect effect))
		{
			MetricsManager.LogError("failed to create instance of effect " + EffectName);
			return false;
		}
		effect.Duration = Duration.RollCached();
		return ParentObject.ApplyEffect(effect);
	}
}
