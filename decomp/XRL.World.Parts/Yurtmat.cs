using System;

namespace XRL.World.Parts;

[Serializable]
public class Yurtmat : IPart
{
	public int Bonus = 2;

	public override bool SameAs(IPart p)
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EquippedEvent.ID && ID != UnequippedEvent.ID)
		{
			return ID == GetShortDescriptionEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EquippedEvent E)
	{
		E.Actor.RegisterEvent(this, EnteredCellEvent.ID, 0, Serialize: true);
		CheckCamouflage();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(UnequippedEvent E)
	{
		E.Actor.UnregisterEvent(this, EnteredCellEvent.ID);
		StatShifter.RemoveStatShifts(E.Actor);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		CheckCamouflage();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(GetShortDescriptionEvent E)
	{
		E.Postfix.AppendRules(Bonus.Signed() + " DV while occupying the same tile as foliage");
		return base.HandleEvent(E);
	}

	private void CheckCamouflage()
	{
		GameObject equipped = ParentObject.Equipped;
		Cell cell = equipped?.CurrentCell;
		if (cell != null)
		{
			if (cell.HasObjectWithPartOtherThan(typeof(PlantProperties), equipped))
			{
				StatShifter.DefaultDisplayName = "camouflage";
				StatShifter.SetStatShift(equipped, "DV", Bonus);
			}
			else
			{
				StatShifter.RemoveStatShifts(equipped);
			}
		}
	}
}
