using System;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class WalltrapAcid : IPart
{
	public int JetLength = 6;

	public string JetDamage = "3d6";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WalltrapTrigger");
		base.Register(Object, Registrar);
	}

	public void Flame(Cell C)
	{
		if (C == null)
		{
			return;
		}
		foreach (GameObject item in C.GetObjectsWithPart("Combat", (GameObject obj) => obj.PhaseMatches(ParentObject)))
		{
			item.TakeDamage(JetDamage.RollCached(), "from a plume of acid!", "Acid", null, null, null, ParentObject, null, null, null, Accidental: false, Environmental: true);
		}
		GameObject gameObject = GameObject.Create("AcidPool");
		gameObject.AddPart(new Temporary(3, "AcidGas10"));
		C.AddObject(gameObject);
	}

	public void FireJet(string D)
	{
		Cell cellFromDirection = ParentObject.CurrentCell;
		for (int i = 0; i < JetLength; i++)
		{
			cellFromDirection = cellFromDirection.GetCellFromDirection(D);
			if (cellFromDirection == null || cellFromDirection.IsSolid(ForFluid: true))
			{
				break;
			}
			Flame(cellFromDirection);
			if (!cellFromDirection.ParentZone.IsActive() || !cellFromDirection.IsVisible())
			{
				continue;
			}
			for (int j = 0; j < 3; j++)
			{
				string text = "&C";
				int num = Stat.Random(1, 2);
				if (num == 1)
				{
					text = "&G";
				}
				if (num == 2)
				{
					text = "&g";
				}
				int num2 = Stat.Random(1, 2);
				if (num2 == 1)
				{
					text += "^g";
				}
				if (num2 == 2)
				{
					text += "^G";
				}
				ParticleManager particleManager = XRLCore.ParticleManager;
				string text2 = ".";
				int num3 = Stat.Random(1, 3);
				if (num3 == 1)
				{
					text2 = "ø";
				}
				if (num3 == 2)
				{
					text2 = "ù";
				}
				if (num3 == 3)
				{
					text2 = "ú";
				}
				particleManager.Add(text + text2, cellFromDirection.X, cellFromDirection.Y, 0f, 0f, 10 + 2 * i + (6 - 2 * j));
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
						FireJet(text);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
