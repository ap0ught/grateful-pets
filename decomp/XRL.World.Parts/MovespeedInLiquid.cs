using System;

namespace XRL.World.Parts;

[Serializable]
public class MovespeedInLiquid : IPart
{
	public int MS = 50;

	public int MinVolume = 200;

	public int CurrentBonus;

	public string Liquid;

	public bool ShowInShortDescription = true;

	public override bool SameAs(IPart p)
	{
		MovespeedInLiquid movespeedInLiquid = p as MovespeedInLiquid;
		if (movespeedInLiquid.MS != MS)
		{
			return false;
		}
		if (movespeedInLiquid.MinVolume != MinVolume)
		{
			return false;
		}
		if (movespeedInLiquid.Liquid != Liquid)
		{
			return false;
		}
		if (movespeedInLiquid.ShowInShortDescription != ShowInShortDescription)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != GetShortDescriptionEvent.ID)
		{
			return ID == UnequippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterPartEvent(this, "EndTurn");
		E.Actor.RegisterPartEvent(this, "EnteredCell");
		CurrentBonus = 0;
		CheckLiquid();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		if (CurrentBonus > 0)
		{
			if (E.Actor != null)
			{
				StatShifter.RemoveStatShift(E.Actor, "MoveSpeed");
			}
			CurrentBonus = 0;
		}
		E.Actor.UnregisterPartEvent(this, "EndTurn");
		E.Actor.UnregisterPartEvent(this, "EnteredCell");
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		if (ShowInShortDescription && MS != 0)
		{
			E.Postfix.Append("\n&C");
			if (MS > 0)
			{
				E.Postfix.Append('+').Append(MS);
			}
			else
			{
				E.Postfix.Append(MS);
			}
			E.Postfix.Append(" move speed while moving through at least ").Append(MinVolume).Append(' ')
				.Append((MinVolume == 1) ? "dram" : "drams")
				.Append(" of ")
				.Append(string.IsNullOrEmpty(Liquid) ? "liquid" : Liquid);
		}
		return base.HandleEvent(E);
	}

	private void CheckLiquid()
	{
		GameObject equipped = ParentObject.Equipped;
		if (equipped == null)
		{
			return;
		}
		Cell cell = equipped.CurrentCell;
		if (cell == null)
		{
			return;
		}
		bool flag = false;
		int i = 0;
		for (int count = cell.Objects.Count; i < count; i++)
		{
			LiquidVolume liquidVolume = cell.Objects[i].LiquidVolume;
			if (liquidVolume != null && liquidVolume.IsOpenVolume() && liquidVolume.Volume >= MinVolume && (string.IsNullOrEmpty(Liquid) || liquidVolume.Amount(Liquid) >= MinVolume))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (CurrentBonus == 0)
			{
				CurrentBonus = MS;
				StatShifter.SetStatShift(equipped, "MoveSpeed", -MS);
			}
		}
		else if (CurrentBonus > 0)
		{
			StatShifter.RemoveStatShift(equipped, "MoveSpeed");
			CurrentBonus = 0;
		}
	}

	public override bool AllowStaticRegistration()
	{
		return false;
	}

	public override bool FireEvent(Event E)
	{
		if (E.ID == "EnteredCell" || E.ID == "EndTurn")
		{
			CheckLiquid();
		}
		return base.FireEvent(E);
	}
}
