using System;
using XRL.Rules;

namespace XRL.World.Parts;

[Serializable]
public class DeployWith : IPart
{
	public string Blueprint;

	public bool SolidOkay;

	public bool SeepingOkay;

	public bool SameCell;

	public string PreferredDirection;

	public int Chance = 100;

	public bool CarryOverOwner = true;

	public override bool SameAs(IPart p)
	{
		DeployWith deployWith = p as DeployWith;
		if (deployWith.Blueprint != Blueprint)
		{
			return false;
		}
		if (deployWith.SolidOkay != SolidOkay)
		{
			return false;
		}
		if (deployWith.SeepingOkay != SeepingOkay)
		{
			return false;
		}
		if (deployWith.SameCell != SameCell)
		{
			return false;
		}
		if (deployWith.PreferredDirection != PreferredDirection)
		{
			return false;
		}
		if (deployWith.Chance != Chance)
		{
			return false;
		}
		if (deployWith.CarryOverOwner != CarryOverOwner)
		{
			return false;
		}
		return true;
	}

	public override bool CanGenerateStacked()
	{
		return false;
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != EnteredCellEvent.ID)
		{
			return ID == ZoneBuiltEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(EnteredCellEvent E)
	{
		GameObject parentObject = ParentObject;
		if (parentObject != null && parentObject.CurrentZone?.Built == true)
		{
			PerformDeploy();
			ParentObject.RemovePart(this);
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ZoneBuiltEvent E)
	{
		PerformDeploy();
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	public void PerformDeploy()
	{
		if (!Chance.in100())
		{
			return;
		}
		Cell ourCell = ParentObject.CurrentCell;
		if (ourCell == null)
		{
			return;
		}
		Cell targetCell = null;
		if (SameCell)
		{
			targetCell = ourCell;
		}
		else
		{
			int num = 0;
			ourCell.ForeachLocalAdjacentCell((Cell altCell) =>
			{
				if (SolidOkay || !altCell.IsSolid(SeepingOkay))
				{
					if (!PreferredDirection.IsNullOrEmpty() && ourCell.GetDirectionFromCell(altCell) == PreferredDirection)
					{
						targetCell = altCell;
						return false;
					}
					num++;
				}
				return true;
			});
			if (targetCell == null && num > 0)
			{
				int selected = Stat.Random(1, num);
				int pos = 0;
				ourCell.ForeachLocalAdjacentCell((Cell altCell) =>
				{
					if ((SolidOkay || !altCell.IsSolid(SeepingOkay)) && ++pos == selected)
					{
						targetCell = altCell;
						return false;
					}
					return true;
				});
			}
		}
		if (targetCell == null)
		{
			return;
		}
		if (CarryOverOwner)
		{
			string owner = ParentObject.Owner;
			GameObjectFactory.ProcessSpecification(Blueprint, (GameObject obj) =>
			{
				if (!owner.IsNullOrEmpty() && obj.Physics != null)
				{
					obj.Physics.Owner = owner;
				}
				targetCell.AddObject(obj);
			}, null, 1, 0, 0, null, "DeployWith");
		}
		else
		{
			GameObjectFactory.ProcessSpecification(Blueprint, (GameObject obj) =>
			{
				targetCell.AddObject(obj);
			});
		}
	}
}
