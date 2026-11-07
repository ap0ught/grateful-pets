using System;
using XRL.World.Parts;

namespace XRL.World.AI.GoalHandlers;

[Serializable]
public class DustAnUrnGoal : GoalHandler
{
	public GlobalLocation Target;

	public bool Dusted;

	public DustAnUrnGoal()
	{
	}

	public DustAnUrnGoal(GlobalLocation Target)
	{
		this.Target = Target;
	}

	public DustAnUrnGoal(string ZoneID, int Wx, int Wy, int Xx, int Yx, int Zx, int X, int Y)
	{
		Target = new GlobalLocation(ZoneID, Wx, Wy, Xx, Yx, Zx, X, Y);
	}

	public override bool Finished()
	{
		return Dusted;
	}

	public override void Create()
	{
	}

	public void MoveToAndDustUrn()
	{
		if (Dusted)
		{
			FailToParent();
			return;
		}
		Cell cell = ParentObject.CurrentZone.GetCell(Target.CellX, Target.CellY);
		if (ParentObject.DistanceTo(cell) <= 1)
		{
			GameObject gameObject = null;
			int i = 0;
			for (int count = cell.Objects.Count; i < count; i++)
			{
				GameObject gameObject2 = cell.Objects[i];
				if (gameObject2.HasPart("EaterUrn"))
				{
					gameObject = gameObject2;
					break;
				}
			}
			if (gameObject != null)
			{
				ParentBrain.DidXToY("dust", gameObject, null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: false, IndefiniteObject: true);
				gameObject.DustPuff();
				Dusted = true;
			}
			else
			{
				AIUrnDuster.UrnFoundDestroyed(ParentObject.CurrentZone);
				FailToParent();
			}
		}
		else
		{
			ParentBrain.PushGoal(new MoveTo(cell, careful: false, overridesCombat: false, 1));
		}
	}

	public override void TakeAction()
	{
		if (ParentObject.InZone(Target.ZoneID))
		{
			MoveToAndDustUrn();
		}
		else
		{
			ParentBrain.PushGoal(new MoveToZone(Target));
		}
	}
}
