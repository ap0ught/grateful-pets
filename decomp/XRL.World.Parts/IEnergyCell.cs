using System;
using XRL.World.Anatomy;
using XRL.World.Capabilities;

namespace XRL.World.Parts;

[Serializable]
public abstract class IEnergyCell : IRechargeable, IContextRelationManager
{
	public string SlotType = "EnergyCell";

	public GameObject SlottedIn;

	public abstract bool HasAnyCharge();

	public abstract bool HasCharge(int Amount);

	public abstract int GetCharge();

	public abstract int GetChargePercentage();

	public abstract string ChargeStatus();

	public abstract void TinkerInitialize();

	public abstract void UseCharge(int Amount);

	public abstract void SetChargePercentage(int Percentage);

	public abstract void RandomizeCharge();

	public abstract void MaximizeCharge();

	public virtual string GetSlotTypeName()
	{
		return EnergyStorage.GetSlotTypeName(SlotType);
	}

	public virtual string GetSlotTypeShortName()
	{
		return EnergyStorage.GetSlotTypeShortName(SlotType);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != PooledEvent<GetContextEvent>.ID && ID != RemoveFromContextEvent.ID && ID != PooledEvent<ReplaceInContextEvent>.ID)
		{
			return ID == TryRemoveFromContextEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(GetContextEvent E)
	{
		if (GameObject.Validate(ref SlottedIn))
		{
			E.ObjectContext = SlottedIn;
			E.Relation = 5;
			E.RelationManager = this;
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(ReplaceInContextEvent E)
	{
		ReplaceCell(E.Replacement);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(RemoveFromContextEvent E)
	{
		ReplaceCell(null);
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(TryRemoveFromContextEvent E)
	{
		ReplaceCell(null);
		return base.HandleEvent(E);
	}

	private void ReplaceCell(GameObject Replacement)
	{
		if (GameObject.Validate(ref SlottedIn) && SlottedIn.TryGetPart<EnergyCellSocket>(out var Part) && Part.Cell == ParentObject)
		{
			Part.SetCell(Replacement);
		}
	}

	public GameObject GetSlottedIn()
	{
		GameObject.Validate(ref SlottedIn);
		return SlottedIn;
	}

	public bool RestoreContextRelation(GameObject Object, GameObject ObjectContext, Cell CellContext, BodyPart BodyPartContext, int Relation, bool Silent = true)
	{
		if (Relation == 5 && ObjectContext != null)
		{
			EnergyCellSocket part = SlottedIn.GetPart<EnergyCellSocket>();
			if (part != null)
			{
				if (part.Cell != Object || SlottedIn != ObjectContext)
				{
					part.SetCell(Object);
				}
				return true;
			}
		}
		return false;
	}
}
