using System;

namespace XRL.World.Parts;

[Serializable]
public class HologramProjector : IPoweredPart
{
	public string ACT_CMD = "ActivateHologramProjector";

	public string Blueprint;

	public string Direction = "S";

	public GameObject Hologram;

	[NonSerialized]
	public GameObject Surface;

	public HologramProjector()
	{
		WorksOnSelf = true;
		IsPowerSwitchSensitive = true;
		NameForStatus = "Holography";
	}

	public override void Write(GameObject Basis, SerializationWriter Writer)
	{
		Writer.WriteOptimized(Blueprint);
		Writer.WriteOptimized(Direction);
		Writer.WriteGameObject(Hologram);
	}

	public override void Read(GameObject Basis, SerializationReader Reader)
	{
		Blueprint = Reader.ReadOptimizedString();
		Direction = Reader.ReadOptimizedString();
		Hologram = Reader.ReadGameObject();
	}

	public override bool WantTurnTick()
	{
		return true;
	}

	public override void TurnTick(long TimeTick, int Amount)
	{
		CheckStatus(UseCharge: true, Amount);
	}

	public void CheckStatus(bool UseCharge = false, int MultipleCharge = 1)
	{
		if (IsReady(UseCharge: true, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, MultipleCharge, null, UseChargeIfUnpowered: false, 0L))
		{
			Enable();
		}
		else if (GameObject.Validate(ref Hologram))
		{
			Disable();
		}
	}

	public void Enable(bool Silent = false)
	{
		if (GameObject.Validate(ref Hologram) || Blueprint.IsNullOrEmpty())
		{
			return;
		}
		Cell cell = GetProjectionSurface()?.CurrentCell;
		if (cell != null)
		{
			GameObject gameObject = (Hologram = cell.AddObject(Blueprint));
			gameObject.RequirePart<ExistenceSupport>().SupportedBy = ParentObject;
			if (gameObject.TryGetPart<HologramMaterial>(out var Part))
			{
				Part.FlickerFrame = 2;
			}
			if (!Silent)
			{
				gameObject.Physics.DidX("appear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			}
		}
	}

	public void Disable(bool Silent = false)
	{
		if (GameObject.Validate(ref Hologram))
		{
			if (!Silent)
			{
				Hologram.Physics.DidX("disappear", null, null, null, null, null, null, UseFullNames: false, IndefiniteSubject: true, null, null, DescribeSubjectDirection: false, DescribeSubjectDirectionLate: true);
			}
			Hologram.Destroy(null, Silent);
		}
	}

	public void SetBlueprint(string Blueprint, bool Silent = false)
	{
		if (!(Blueprint == this.Blueprint))
		{
			this.Blueprint = Blueprint;
			if (GameObject.Validate(ref Hologram))
			{
				Disable(Silent);
				Enable(Silent);
			}
		}
	}

	public override bool WantEvent(int ID, int Cascade)
	{
		if (ID != PooledEvent<CheckExistenceSupportEvent>.ID)
		{
			return ID == PowerSwitchFlippedEvent.ID;
		}
		return true;
	}

	public override bool HandleEvent(PowerSwitchFlippedEvent E)
	{
		CheckStatus();
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(CheckExistenceSupportEvent E)
	{
		if (Hologram == E.Object && IsReady(UseCharge: false, IgnoreCharge: false, IgnoreLiquid: false, IgnoreBootSequence: false, IgnoreBreakage: false, IgnoreRust: false, IgnoreEMP: false, IgnoreRealityStabilization: false, IgnoreSubject: false, IgnoreLocallyDefinedFailure: false, 1, null, UseChargeIfUnpowered: false, 0L))
		{
			return false;
		}
		return base.HandleEvent(E);
	}

	public GameObject GetProjectionSurface()
	{
		if (GameObject.Validate(Surface) && ParentObject.GetDirectionToward(Surface) == Direction)
		{
			return Surface;
		}
		Surface = null;
		for (Cell localCellFromDirection = ParentObject.CurrentCell.GetLocalCellFromDirection(Direction); localCellFromDirection != null; localCellFromDirection = localCellFromDirection.GetLocalCellFromDirection(Direction))
		{
			GameObject firstObjectWithPropertyOrTag = localCellFromDirection.GetFirstObjectWithPropertyOrTag("ProjectionSurface");
			if (firstObjectWithPropertyOrTag != null)
			{
				return Surface = firstObjectWithPropertyOrTag;
			}
		}
		return null;
	}

	public override bool GetActivePartLocallyDefinedFailure()
	{
		if (!Blueprint.IsNullOrEmpty())
		{
			return !GameObject.Validate(GetProjectionSurface());
		}
		return true;
	}

	public override string GetActivePartLocallyDefinedFailureDescription()
	{
		if (Blueprint.IsNullOrEmpty())
		{
			return "ProjectionGraphMissing";
		}
		return "ProjectionSurfaceMissing";
	}
}
