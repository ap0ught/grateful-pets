using System;
using System.Collections.Generic;
using XRL.Core;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class WalltrapShock : IPart
{
	public int JetLength = 3;

	public string JetDamageRange = "2d6";

	public string JetVoltage = "1d3";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WalltrapTrigger");
		base.Register(Object, Registrar);
	}

	public void Zap(Cell C)
	{
		if (C == null)
		{
			return;
		}
		using List<GameObject>.Enumerator enumerator = C.GetObjectsWithPart("Physics", (GameObject obj) => obj.PhaseMatches(ParentObject)).GetEnumerator();
		if (enumerator.MoveNext())
		{
			_ = enumerator.Current;
			ParentObject.Discharge(C, JetVoltage.RollCached(), 0, JetDamageRange, null, ParentObject, ParentObject);
		}
	}

	public void ZapBolt(string D)
	{
		Cell cellFromDirection = ParentObject.CurrentCell;
		for (int i = 0; i < JetLength; i++)
		{
			cellFromDirection = cellFromDirection.GetCellFromDirection(D);
			if (cellFromDirection == null)
			{
				break;
			}
			Zap(cellFromDirection);
			if (cellFromDirection.ParentZone.IsActive() && cellFromDirection.IsVisible())
			{
				for (int j = 0; j < 3; j++)
				{
					string text = (50.in100() ? "&W" : "&Y");
					XRLCore.ParticleManager.Add(text + (char)Stat.Random(191, 198), cellFromDirection.X, cellFromDirection.Y, 0f, 0f, 10 + 2 * i + (6 - 2 * j));
				}
			}
			if (cellFromDirection.IsSolid(ForFluid: true))
			{
				break;
			}
		}
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "WalltrapTrigger")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null && !IsBroken() && !IsRusted() && !IsEMPed())
			{
				string[] cardinalDirectionList = Directions.CardinalDirectionList;
				foreach (string text in cardinalDirectionList)
				{
					Cell cellFromDirection = cell.GetCellFromDirection(text);
					if (cellFromDirection != null && !cellFromDirection.IsSolid(ForFluid: true))
					{
						ZapBolt(text);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
