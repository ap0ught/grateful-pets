using System;
using System.Collections.Generic;
using ConsoleLib.Console;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LiquidProximityFrenzies : IPart
{
	public string LiquidID = "blood";

	public int Range = 10;

	public bool LiquidMustBeConnectedByLiquid = true;

	public string Duration = "10";

	public bool Stack;

	public int QuicknessBonus = 10;

	public int MaxKillRadiusBonus = 10;

	public int BerserkDuration = 5;

	public bool BerserkImmediately;

	public bool BerserkOnDealDamage;

	public bool PreferBleedingTarget;

	public string Message = "=subject.T==subject.directionIfAny= =verb:are= frenzied by the scent of =liquid=!";

	public override bool SameAs(IPart p)
	{
		LiquidProximityFrenzies liquidProximityFrenzies = p as LiquidProximityFrenzies;
		if (liquidProximityFrenzies.LiquidID != LiquidID)
		{
			return false;
		}
		if (liquidProximityFrenzies.Range != Range)
		{
			return false;
		}
		if (liquidProximityFrenzies.LiquidMustBeConnectedByLiquid != LiquidMustBeConnectedByLiquid)
		{
			return false;
		}
		if (liquidProximityFrenzies.Duration != Duration)
		{
			return false;
		}
		if (liquidProximityFrenzies.Stack != Stack)
		{
			return false;
		}
		if (liquidProximityFrenzies.QuicknessBonus != QuicknessBonus)
		{
			return false;
		}
		if (liquidProximityFrenzies.MaxKillRadiusBonus != MaxKillRadiusBonus)
		{
			return false;
		}
		if (liquidProximityFrenzies.BerserkDuration != BerserkDuration)
		{
			return false;
		}
		if (liquidProximityFrenzies.BerserkImmediately != BerserkImmediately)
		{
			return false;
		}
		if (liquidProximityFrenzies.BerserkOnDealDamage != BerserkOnDealDamage)
		{
			return false;
		}
		if (liquidProximityFrenzies.PreferBleedingTarget != PreferBleedingTarget)
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
		if (!Stack && ParentObject.HasEffect<Frenzied>())
		{
			return false;
		}
		GameObject proximateLiquidPool = GetProximateLiquidPool();
		if (proximateLiquidPool == null)
		{
			return false;
		}
		TriggerEffect(proximateLiquidPool);
		return true;
	}

	public GameObject GetProximateLiquidPool()
	{
		Cell cell = ParentObject.CurrentCell;
		if (cell == null)
		{
			return null;
		}
		GameObject openLiquidVolume = cell.GetOpenLiquidVolume();
		if (LiquidMustBeConnectedByLiquid && openLiquidVolume == null)
		{
			return null;
		}
		if (openLiquidVolume != null && openLiquidVolume.LiquidVolume?.ContainsLiquid(LiquidID) == true)
		{
			return openLiquidVolume;
		}
		int i = 0;
		for (int count = cell.Objects.Count; i < count; i++)
		{
			GameObject gameObject = cell.Objects[i];
			LiquidCovered effect = gameObject.GetEffect<LiquidCovered>();
			if (effect != null)
			{
				LiquidVolume liquid = effect.Liquid;
				if (liquid != null && liquid.ContainsLiquid(LiquidID))
				{
					return gameObject;
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
					return openLiquidVolume;
				}
				int l = 0;
				for (int count3 = cell2.Objects.Count; l < count3; l++)
				{
					GameObject gameObject2 = cell2.Objects[l];
					LiquidCovered effect2 = gameObject2.GetEffect<LiquidCovered>();
					if (effect2 != null)
					{
						LiquidVolume liquid2 = effect2.Liquid;
						if (liquid2 != null && liquid2.ContainsLiquid(LiquidID))
						{
							return gameObject2;
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
		return null;
	}

	public bool TriggerEffect(GameObject obj = null)
	{
		Frenzied e = new Frenzied(Duration.RollCached(), QuicknessBonus, MaxKillRadiusBonus, BerserkDuration, BerserkImmediately, BerserkOnDealDamage, PreferBleedingTarget);
		if (!ParentObject.ApplyEffect(e))
		{
			return false;
		}
		if (!Message.IsNullOrEmpty() && Visible())
		{
			string text = Message;
			if (text.Contains("=liquid="))
			{
				text = text.Replace("=liquid=", ColorUtility.StripFormatting(LiquidVolume.GetLiquid(LiquidID).Name));
			}
			text = GameText.VariableReplace(text, ParentObject, obj);
			if (!text.IsNullOrEmpty())
			{
				IComponent<GameObject>.AddPlayerMessage(text);
			}
		}
		return true;
	}
}
