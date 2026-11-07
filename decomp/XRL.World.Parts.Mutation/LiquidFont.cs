using System;
using System.Collections.Generic;

namespace XRL.World.Parts.Mutation;

[Serializable]
public class LiquidFont : BaseMutation
{
	public bool Prefill = true;

	public int Chance = 100;

	public int EnergyCost = 1000;

	public string Amount = "500";

	public int MaxAmount = 800;

	public int MaxRadius = 3;

	public string Cooldown = "1d20";

	public string Liquid = "lava";

	public int CooldownLeft;

	public override string GetLevelText(int Level)
	{
		return "You ooze fluids with the best of them.\n";
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			if (ID == EnteredCellEvent.ID)
			{
				return Prefill;
			}
			return false;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		if (Prefill)
		{
			Prefill = false;
			Bubble(80);
		}
		return base.HandleEvent(E);
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		BubbleTick(Amount);
	}

	public void BubbleTick(int ticks = 1)
	{
		CooldownLeft -= ticks;
		if (ParentObject.OnWorldMap())
		{
			return;
		}
		int num = 0;
		while (CooldownLeft < 0)
		{
			CooldownLeft += Cooldown.RollCached();
			if (Chance.in100())
			{
				num++;
			}
		}
		if (num > 0)
		{
			Bubble(num);
			ParentObject.ParticleBlip("&" + LiquidVolume.GetLiquid(Liquid).GetColor() + "~", 10, 0L);
		}
	}

	public void Bubble(int n = 1)
	{
		for (int i = 1; i < MaxRadius; i++)
		{
			if (n <= 0)
			{
				break;
			}
			List<Cell> list = ParentObject.GetCurrentCell().GetLocalAdjacentCells(i).ShuffleInPlace();
			for (int j = 0; j < list.Count; j++)
			{
				GameObject openLiquidVolume = list[j].GetOpenLiquidVolume();
				if (openLiquidVolume == null)
				{
					Cell cell = list[j];
					openLiquidVolume = GameObject.Create("AcidPool");
					openLiquidVolume.LiquidVolume.Fill(Liquid, Amount.RollCached());
					cell.AddObject(openLiquidVolume);
					UseEnergy(EnergyCost);
					n--;
					if (n <= 0)
					{
						break;
					}
					continue;
				}
				LiquidVolume liquidVolume = openLiquidVolume.LiquidVolume;
				if (liquidVolume.Amount(Liquid) < MaxAmount)
				{
					liquidVolume.MixWith(new LiquidVolume(Liquid, Amount.RollCached()));
					UseEnergy(EnergyCost);
					n--;
					if (n <= 0)
					{
						break;
					}
				}
			}
		}
	}
}
