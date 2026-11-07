using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class WalltrapFire : IPart
{
	public int JetLength = 6;

	public string JetDamage = "3d5+12";

	public string JetTemperature = "2d50+200";

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override void Register(GameObject Object, IEventRegistrar Registrar)
	{
		Registrar.Register("WalltrapTrigger");
		base.Register(Object, Registrar);
	}

	public bool ShouldFlame(GameObject Object)
	{
		if (!ParentObject.PhaseMatches(Object))
		{
			return false;
		}
		if (Object.IsCombatObject())
		{
			return true;
		}
		Object.HasTagOrProperty("Plant");
		if (Object.isFurniture())
		{
			return true;
		}
		return false;
	}

	public void Flame(Cell C)
	{
		string jetDamage = JetDamage;
		if (C == null)
		{
			return;
		}
		foreach (GameObject @object in C.GetObjects(ShouldFlame))
		{
			@object.TemperatureChange(JetTemperature.RollCached(), ParentObject);
			if (25.in100() && IComponent<GameObject>.Visible(@object))
			{
				@object.Smoke();
			}
		}
		foreach (GameObject objectsViaEvent in C.GetObjectsViaEventList())
		{
			if (objectsViaEvent.IsCombatObject() || objectsViaEvent.HasPart<CrossFlameOnStep>() || objectsViaEvent.HasPart<BurnOffGas>())
			{
				objectsViaEvent.TakeDamage(jetDamage.RollCached(), "from a {{fiery|jet of flames}}!", "Fire Heat", null, null, null, null, ParentObject, null, null, Accidental: false, Environmental: true);
			}
		}
	}

	public void FireJet(string D)
	{
		Cell cellFromDirection = ParentObject.CurrentCell;
		for (int i = 0; i < JetLength; i++)
		{
			cellFromDirection = cellFromDirection.GetCellFromDirection(D);
			if (cellFromDirection != null)
			{
				Flame(cellFromDirection);
				if (cellFromDirection.ParentZone.IsActive() && cellFromDirection.IsVisible())
				{
					cellFromDirection.Flameburst();
				}
				if (cellFromDirection.IsSolid(ForFluid: true))
				{
					break;
				}
				continue;
			}
			break;
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
