using System;
using XRL.World.Effects;

namespace XRL.World.Parts;

[Serializable]
public class LeakWhenBroken : IPart
{
	public string PercentPerTurn = "10-20";

	public override bool SameAs(IPart p)
	{
		if ((p as LeakWhenBroken).PercentPerTurn != PercentPerTurn)
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
		CheckLeaking(Amount);
	}

	private void CheckLeaking(int Turns = 1)
	{
		if (!IsBroken())
		{
			return;
		}
		LiquidVolume liquidVolume = ParentObject.LiquidVolume;
		if (liquidVolume == null || liquidVolume.Volume <= 0 || liquidVolume.IsOpenVolume())
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < Turns; i++)
		{
			int num2 = PercentPerTurn.RollCached();
			int num3 = liquidVolume.Volume * num2 / 100;
			if (num3 < 1 && num2 > 0)
			{
				num3 = 1;
			}
			num += num3;
		}
		if (num > 0)
		{
			DistributeLiquid(liquidVolume.Split(num));
		}
	}

	private void DistributeLiquid(LiquidVolume produced)
	{
		if (produced == null || produced.Volume <= 0)
		{
			return;
		}
		GameObject objectContext = ParentObject.GetObjectContext();
		if (ParentObject.GetCellContext() == null)
		{
			objectContext.GetCellContext();
		}
		Cell cell = ParentObject.GetCurrentCell();
		if (cell == null || cell.ParentZone.IsWorldMap())
		{
			return;
		}
		if (Visible())
		{
			IComponent<GameObject>.AddPlayerMessage(ParentObject.The + ParentObject.ShortDisplayName + ParentObject.GetVerb("leak") + " " + produced.Volume + " " + ((produced.Volume == 1) ? "dram" : "drams") + " of " + produced.GetLiquidName() + ".");
		}
		ParentObject.ApplyEffect(new LiquidCovered(produced, 1));
		LiquidVolume V;
		if (produced.Volume > 0 && !cell.IsSolid(ForFluid: true) && cell.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
		{
			V = GO.LiquidVolume;
			if (V.MaxVolume != -1 && V.Collector && V.Volume < V.MaxVolume)
			{
				V.MixWith(produced);
				return false;
			}
			return true;
		}) && cell.ForeachObjectWithPart("LiquidVolume", (GameObject GO) =>
		{
			V = GO.LiquidVolume;
			if (V.MaxVolume == -1)
			{
				V.MixWith(produced);
				return false;
			}
			return true;
		}))
		{
			GameObject gameObject = GameObject.Create("Water");
			V = gameObject.LiquidVolume;
			string groundLiquid = cell.GroundLiquid;
			if (string.IsNullOrEmpty(groundLiquid))
			{
				V.InitialLiquid = produced.GetLiquidDesignation();
			}
			else
			{
				V.Volume = produced.Volume;
				V.InitialLiquid = groundLiquid;
				V.MixWith(produced);
			}
			cell.AddObject(gameObject);
		}
	}
}
