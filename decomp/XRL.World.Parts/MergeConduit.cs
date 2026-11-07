using System;
using XRL.World.Tinkering;

namespace XRL.World.Parts;

[Serializable]
public class MergeConduit : IPart
{
	public string ModPart;

	public string DestroyIfPart;

	public string SkipIfPart;

	public bool AllowExistenceSupport;

	public override bool SameAs(IPart p)
	{
		MergeConduit mergeConduit = p as MergeConduit;
		if (mergeConduit.ModPart != ModPart)
		{
			return false;
		}
		if (mergeConduit.DestroyIfPart != DestroyIfPart)
		{
			return false;
		}
		if (mergeConduit.SkipIfPart != SkipIfPart)
		{
			return false;
		}
		if (mergeConduit.AllowExistenceSupport != AllowExistenceSupport)
		{
			return false;
		}
		return base.SameAs(p);
	}

	public override bool WantEvent(int ID, int cascade)
	{
		if (!base.WantEvent(ID, cascade) && ID != AfterZoneBuiltEvent.ID)
		{
			return ID == PooledEvent<CheckSpawnMergeEvent>.ID;
		}
		return true;
	}

	public override bool HandleEvent(CheckSpawnMergeEvent E)
	{
		if (!DestroyIfPart.IsNullOrEmpty() && E.Other.HasPart(DestroyIfPart))
		{
			E.Object.Obliterate();
			return false;
		}
		if (!ModPart.IsNullOrEmpty() && ValidInstall(E.Other) && ItemModding.ApplyModification(E.Other, ModPart))
		{
			E.Object.Obliterate();
			return false;
		}
		return base.HandleEvent(E);
	}

	public override bool HandleEvent(AfterZoneBuiltEvent E)
	{
		ParentObject.RemovePart(this);
		return base.HandleEvent(E);
	}

	public override bool AllowStaticRegistration()
	{
		return true;
	}

	private bool ValidInstall(GameObject obj)
	{
		if (obj.IsTakeable())
		{
			return false;
		}
		if (!obj.IsWall() && !obj.IsDoor())
		{
			return false;
		}
		if (!obj.IsReal)
		{
			return false;
		}
		if (!AllowExistenceSupport && obj.HasPart<ExistenceSupport>())
		{
			return false;
		}
		if (obj.IsCombatObject())
		{
			return false;
		}
		if (!ModPart.IsNullOrEmpty() && obj.HasPart(ModPart))
		{
			return false;
		}
		if (!SkipIfPart.IsNullOrEmpty() && obj.HasPart(SkipIfPart))
		{
			return false;
		}
		return true;
	}
}
