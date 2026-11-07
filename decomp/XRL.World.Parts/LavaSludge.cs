using System;

namespace XRL.World.Parts;

[Serializable]
public class LavaSludge : IPart
{
	public const string DEATH_REASON = "You cooled into a block of shale.";

	public const string THIRD_PERSON_DEATH_REASON_SUFFIX = " @@cooled into a block of shale.";

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckTemperature();
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade))
		{
			return ID == BeforeDieEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(BeforeDieEvent E)
	{
		if (E.Reason != "You cooled into a block of shale.")
		{
			Cell cell = ParentObject.CurrentCell;
			if (cell != null)
			{
				DidX("cool", "into a block of shale", null, null, null, null, ParentObject);
				ParentObject.Die(null, "phase change", "You cooled into a block of shale.", ParentObject.It + " @@cooled into a block of shale.", Accidental: true);
				if (!cell.IsSolid())
				{
					cell.AddObject(GameObject.Create("Shale"));
				}
				return false;
			}
		}
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void CheckTemperature()
	{
		if (ParentObject.IsAflame())
		{
			return;
		}
		Cell cell = ParentObject.CurrentCell;
		if (cell != null)
		{
			if (!cell.IsSolid())
			{
				cell.AddObject(GameObject.Create("Shale"));
			}
			DidX("cool", "into a block of shale", null, null, null, null, ParentObject);
			ParentObject.Die(null, "phase change", "You cooled into a block of shale.", ParentObject.It + " @@cooled into a block of shale.", Accidental: true);
		}
	}
}
