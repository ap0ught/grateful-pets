using System;
using System.Text;
using XRL.Core;
using XRL.Rules;
using XRL.UI;

namespace XRL.World.Parts;

[Serializable]
public class WalltrapGas : IPart
{
	public int JetLength = 6;

	public string GasBlueprint = "AcidGas";

	public string Density = "1d80+20";

	public string PrimaryColor = "G";

	public string SecondaryColor = "g";

	public override bool SameAs(IPart p)
	{
		WalltrapGas walltrapGas = p as WalltrapGas;
		if (walltrapGas.JetLength != JetLength)
		{
			return false;
		}
		if (walltrapGas.GasBlueprint != GasBlueprint)
		{
			return false;
		}
		if (walltrapGas.Density != Density)
		{
			return false;
		}
		if (walltrapGas.PrimaryColor != PrimaryColor)
		{
			return false;
		}
		if (walltrapGas.SecondaryColor != SecondaryColor)
		{
			return false;
		}
		return true;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WalltrapTrigger");
		base.Register(Object, Registrar);
	}

	public void DeployGas(Cell C)
	{
		if (C != null)
		{
			GameObject gameObject = GameObject.Create(GasBlueprint);
			Gas part = gameObject.GetPart<Gas>();
			if (part != null)
			{
				part.Density = Density.RollCached();
			}
			C.AddObject(gameObject);
		}
	}

	public void ReleaseGas(Cell C, string D)
	{
		for (int i = 0; i < JetLength; i++)
		{
			C = C.GetCellFromDirection(D);
			if (C == null || C.IsSolid(ForFluid: true))
			{
				break;
			}
			DeployGas(C);
			if (!C.ParentZone.IsActive() || !C.IsVisible())
			{
				continue;
			}
			for (int j = 0; j < 3; j++)
			{
				StringBuilder stringBuilder = Event.NewStringBuilder();
				stringBuilder.Append("&");
				stringBuilder.Append((Stat.Random(1, 2) == 1) ? PrimaryColor : SecondaryColor);
				stringBuilder.Append("^");
				stringBuilder.Append((Stat.Random(1, 2) == 1) ? PrimaryColor : SecondaryColor);
				ParticleManager particleManager = XRLCore.ParticleManager;
				char value = '.';
				switch (Stat.Random(1, 3))
				{
				case 1:
					value = 'ø';
					break;
				case 2:
					value = 'ù';
					break;
				case 3:
					value = 'ú';
					break;
				}
				stringBuilder.Append(value);
				particleManager.Add(stringBuilder.ToString(), C.X, C.Y, 0f, 0f, 10 + 2 * i + (6 - 2 * j));
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
						ReleaseGas(cell, text);
					}
				}
			}
		}
		return base.FireEvent(E);
	}
}
